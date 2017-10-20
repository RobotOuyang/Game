using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace LuaFramework {
    public class NetworkManager : Manager {
        private SocketClient socket;
        static Queue<KeyValuePair<int, ByteBuffer>> sEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

        SocketClient SocketClient {
            get { 
                if (socket == null)
                    socket = new SocketClient();
                return socket;                    
            }
        }

        public bool connected
        {
            get { return SocketClient.connected; }
        }

        void Awake() {
            Init();
        }

        void Init() {
            SocketClient.OnRegister();
        }

        public void OnInit() {
            CallMethod("Start");
        }

        public void Unload() {
            CallMethod("Unload");
        }

        /// <summary>
        /// ִ��Lua����
        /// </summary>
        public static object[] CallMethod(string func, params object[] args) {
            return Util.CallMethod("Network", func, args);
        }

        ///------------------------------------------------------------------------------------
        public static void AddEvent(int _event, ByteBuffer data) {
            sEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
        }

        /// <summary>
        /// ����Command�����ﲻ����ķ���˭��
        /// </summary>
        void Update() {
            if (sEvents.Count > 0) {
                while (sEvents.Count > 0) {
                    KeyValuePair<int, ByteBuffer> _event = sEvents.Dequeue();
                    facade.SendMessageCommand(NotiConst.DISPATCH_MESSAGE, _event);
                }
            }
        }

        void OnApplicationPause(bool paused)
        {
            //��������̨ʱpausedΪtrue������Ϊfalse
            CallMethod("OnApplicationPause", paused);
        }

        /// <summary>
        /// ������������
        /// </summary>
        public void SendConnect() {
            SocketClient.SendConnect();
        }

        public void Close()
        {
            SocketClient.Close();
        }

        /// <summary>
        /// ����SOCKET��Ϣ, ���ذ���sequenceId
        /// </summary>
        public uint SendMessage(ByteBuffer buffer) {
            return SocketClient.SendMessage(buffer);
        }

        /// <summary>
        /// ��������
        /// </summary>
        void OnDestroy() {
            SocketClient.OnRemove();
            Debug.Log("~NetworkManager was destroy");
        }
    }
}