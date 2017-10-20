using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;

public enum DisType {
    Exception,
    Disconnect,
}

public class SocketClient {
    private TcpClient client = null;
    private NetworkStream outStream = null;
    private MemoryStream memStream;
    private BinaryReader reader;

    private const int MAX_READ = 8192;
    private byte[] byteBuffer = new byte[MAX_READ];
    uint SequenceID = 1;

    // Use this for initialization
    public SocketClient() {
    }

    /// <summary>
    /// 注册代理
    /// </summary>
    public void OnRegister() {
        memStream = new MemoryStream();
        reader = new BinaryReader(memStream);
    }

    /// <summary>
    /// 移除代理
    /// </summary>
    public void OnRemove() {
        this.Close();
        if (reader != null)
        {
            reader.Close();
        }
        if (memStream != null)
        {
            memStream.Close();
        }
    }

    public bool connected
    {
        get { return client != null && client.Connected; }
    }

    // 同一时间只有一个重连
    private bool _is_conncting = false;
    private bool _is_ipv6 = false;
    private bool _ipv6_failed = false; // 优先使用ipv6保证苹果审核，但是一旦失败过就再也不用，应对坑爹运营商

    /// <summary>
    /// 连接服务器
    /// </summary>
    private delegate IPHostEntry GetHostEntryHandler(string ip);
    void ConnectServer(string host, int port) {
        if (_is_conncting)
        {
            return;
        }

        client = null;
        _is_ipv6 = false;
        if (Application.platform == RuntimePlatform.IPhonePlayer && Socket.OSSupportsIPv6 && !_ipv6_failed)
        {
            GetHostEntryHandler callback = new GetHostEntryHandler(Dns.GetHostEntry);
            IAsyncResult result = callback.BeginInvoke(host, null, null);
            if (result.AsyncWaitHandle.WaitOne(2000, false))
            {
                IPHostEntry hostInfo = callback.EndInvoke(result);
                if (hostInfo != null)
                {
                    foreach (IPAddress ip in hostInfo.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            client = new TcpClient(AddressFamily.InterNetworkV6);
                            host = ip.ToString();
                            _is_ipv6 = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                // 反向查询失败

            }
        }
        if (client == null)
        {
            client = new TcpClient();
        }
        client.SendTimeout = 2000;
        client.ReceiveTimeout = 2000;
        client.NoDelay = true;

        try {
            _is_conncting = true;
            Debug.LogWarning(string.Format("sending connecting ...{0}:{1}", host, port));
            client.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
        } catch (Exception e) {
            OnDisconnected(DisType.Exception, "tcp connect failed!.." + e.Message);
        }
    }

    /// <summary>
    /// 连接上服务器
    /// </summary>
    void OnConnect(IAsyncResult asr) {
        _is_conncting = false;
        if (client == null || !client.Connected)
        {
            OnDisconnected(DisType.Disconnect, "tcp connect failed! server is offline!");
            if (_is_ipv6)
            {
                _ipv6_failed = true;
            }
            return;
        }

        outStream = client.GetStream();
        SequenceID = 1;

        try
        {
            client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
            NetworkManager.AddEvent(Protocal.Connect, new ByteBuffer());
        }
        catch (Exception e)
        {
            OnDisconnected(DisType.Exception, "tcp send failed!.." + e.Message);
        }
    }

    uint CheckSum(uint sec_id, byte [] buffer)
    {
        uint ret = sec_id + 20160601;
        
        for (int i = buffer.Length - 1; i >= 0; i--)
        {
            ret = (ret << 1) ^ buffer[i];
        }
        return ret;
    }

    /// <summary>
    /// 写数据
    /// </summary>
    void WriteMessage(byte[] message) {
        MemoryStream ms = null;
        using (ms = new MemoryStream()) {
            ms.Position = 0;
            BinaryWriter writer = new BinaryWriter(ms);
            ushort msglen = (ushort)(message.Length + 10);
            writer.Write(msglen);   // 服务端要求自己的也算，那就算进去吧
            // Debug.LogWarning(msglen);
            uint sum = CheckSum(SequenceID, message);
            writer.Write(sum);
            writer.Write(SequenceID++);
            writer.Write(message);
            writer.Flush();

            // Debug.LogWarning(string.Format("send len:{0} check_sum:{1} Sequnce:{2} bytes:{3}", msglen, sum, SequenceID-1, BytesToString(message)));
            if (client != null && client.Connected) {
                //NetworkStream stream = client.GetStream(); 
                byte[] payload = ms.ToArray();
                try
                {
                    outStream.Write(payload, 0, payload.Length);
                }
                catch (Exception e)
                {
                    OnDisconnected(DisType.Exception, "tcp send failed!.." + e.Message);
                }
            } else {
                if (client != null)
                {
                    client.Close();
                    client = null;
                }
               
                NetworkManager.AddEvent(Protocal.MessageFailed, new ByteBuffer(message));
            }
        }
    }

    /// <summary>
    /// 读取消息
    /// </summary>
    void OnRead(IAsyncResult asr) {
        int bytesRead = 0;
        try {
            lock (client.GetStream()) {         //分析完，再次监听服务器发过来的新消息
                bytesRead = client.GetStream().EndRead(asr);
                if (bytesRead < 1) {                //包尺寸有问题，断线处理
                    OnDisconnected(DisType.Disconnect, "BytesRead < 1");
                    return;
                }
                OnReceive(byteBuffer, bytesRead);   //分析数据包内容，抛给逻辑层
                Array.Clear(byteBuffer, 0, byteBuffer.Length);   //清空数组
                client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
            }
        } catch (Exception ex) {
            //PrintBytes();
            OnDisconnected(DisType.Exception, ex.Message);
        }
    }

    /// <summary>
    /// 丢失链接
    /// </summary>
    void OnDisconnected(DisType dis, string msg) {
        Close();   //关掉客户端链接
        SequenceID = 1;
        int protocal = dis == DisType.Exception ? Protocal.Exception : Protocal.Disconnect;
        NetworkManager.AddEvent(protocal, new ByteBuffer());
        Debug.LogError("Connection was closed:>" + msg);
    }

    /// <summary>
    /// 打印字节
    /// </summary>
    /// <param name="bytes"></param>
    string BytesToString(byte[] buff) {
        string returnStr = string.Empty;
        for (int i = 0; i < buff.Length; i++) {
            returnStr += buff[i].ToString("X2");
        }
        return returnStr;
    }

    /// <summary>
    /// 向链接写入数据流
    /// </summary>
    void OnWrite(IAsyncResult r) {
        try {
            outStream.EndWrite(r);
        } catch (Exception ex) {
            OnDisconnected(DisType.Exception, ex.Message);
        }
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    void OnReceive(byte[] bytes, int length) {
        memStream.Seek(0, SeekOrigin.End);
        memStream.Write(bytes, 0, length);
        //Reset to beginning
        memStream.Seek(0, SeekOrigin.Begin);
        while (RemainingBytes() > 2) {
            ushort messageLen = reader.ReadUInt16();
            messageLen -= 2;    // 服务器说发过来的也多加一个2
            if (RemainingBytes() >= messageLen) {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(reader.ReadBytes(messageLen));
                ms.Seek(0, SeekOrigin.Begin);
                OnReceivedMessage(ms);
            } else {
                //Back up the position two bytes
                memStream.Position = memStream.Position - 2;
                break;
            }
        }
        //Create a new stream with any leftover bytes
        byte[] leftover = reader.ReadBytes((int)RemainingBytes());
        memStream.SetLength(0);     //Clear
        memStream.Write(leftover, 0, leftover.Length);
    }

    /// <summary>
    /// 剩余的字节
    /// </summary>
    private long RemainingBytes() {
        return memStream.Length - memStream.Position;
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    /// <param name="ms"></param>
    void OnReceivedMessage(MemoryStream ms) {
        BinaryReader r = new BinaryReader(ms);
        byte[] message = r.ReadBytes((int)(ms.Length - ms.Position));
        //int msglen = message.Length;

        ByteBuffer buffer = new ByteBuffer(message);
        // 丢弃两个uint，服务器为了保持结构一致，checksum，sequnceid都留空
        int check_sum = buffer.ReadInt();
        int sequence = buffer.ReadInt();
        // Debug.LogWarning(string.Format("recv len:{0} check_sum:{1} sequnce:{2} buff:{3}",message.Length, check_sum, sequence, BytesToString(message)));
        NetworkManager.AddEvent(Protocal.Message, buffer);
    }

    /// <summary>
    /// 会话发送
    /// </summary>
    void SessionSend(byte[] bytes) {
        WriteMessage(bytes);
    }

    /// <summary>
    /// 关闭链接
    /// </summary>
    public void Close() {
        _is_conncting = false;
        if (client != null) {
            if (client.Connected) client.Close();
            client = null;
        }
    }

    /// <summary>
    /// 发送连接请求
    /// </summary>
    public void SendConnect() {
        ConnectServer(AppConst.SocketAddress, AppConst.SocketPort);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public uint SendMessage(ByteBuffer buffer) {
        SessionSend(buffer.ToBytes());
        buffer.Close();
        return SequenceID - 1;
    }
}
