using UnityEngine;
using System.Collections;

using Pomelo.DotNetClient;
using LitJson;
using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;

using Proto;
//using Proto.gate;
//using Proto.connector;
//using Proto.chat;
using LuaInterface;
using LuaFramework;

public class pomeloBehaviour : Manager
{
    [HideInInspector]
    public PomeloClient pc;
    [TextArea(3, 10)]
    public string HandShakeCache;

    public string uid = "1";

    //public event Action connectEvent;
    public event Action closeEvent;
    public event Action updateClientEvent;

    // Use this for initialization
    void Start()
    {
        //ConnectToGate();
    }

    [ExecuteInEditMode]
    void OnDestroy()
    {
        CloseClient(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (pc != null)
        {
            pc.poll();
        }
    }

    public void CloseClient()
    {
        if (pc != null)
        {
            pc.close();
            //pc.poll();
            if (this.closeEvent != null)
            {
                this.closeEvent();
            }
            pc = null;

            this.UpdateClient();
        }
    }

    public string GetHandShakeCache()
    {
        if(pc != null)
        {
            return pc.HandShakeCache;
        }
        return "";
    }

    public void ConnectToGate()
    {
        closeEvent += OnGateServerDisconnect;
        ConnectServer(AppConst.SocketAddress, AppConst.GatePort, ServerType.GATE, Pomelo.DotNetClient.ClientProtocolType.NORMAL, HandShakeCache);
    }

    public void OnGateServerDisconnect()
    {
        closeEvent -= OnGateServerDisconnect;
        Debug.LogError("Connect gate server failed!");
    }

    public void OnConnectToGate()
    {
        this.HandShakeCache = GetHandShakeCache();
        //gate logic can moveto logicclient
        //gateHandler.queryEntry(uid, delegate (gateHandler.queryEntry_result result)
        //{
        //connectEvent -= OnConnectToGate;
        //closeEvent -= OnGateServerDisconnect;

        //CloseClient();

        //if (result.code == 500)
        //{
        //    TODO
        //    }

        //if (result.code == 200)
        //{
        //    connectEvent += OnConnectToConnector;
        //    ConnectServer(result.host, result.port, ServerType.CONNECTOR, Pomelo.DotNetClient.ClientProtocolType.NORMAL, HandShakeCache);
        //}

        //TODO other event
        //});
        Debug.Log("Gate Connected");
        closeEvent -= OnGateServerDisconnect;
        CallMethod("OnConnectToGate");
    }

    public void OnConnectToConnector()
    {
        //register push messages and then login in lua
        Debug.Log("Connector connected");
        closeEvent += OnServerDisconnect;
        CallMethod("OnConnectToConnector");
    }

    public void OnServerDisconnect()
    {
        closeEvent -= OnServerDisconnect;
        CallMethod("OnServerDisconnect");
    }

    //TODO TLS "C7773B9D1BF0C5C956C88C60440FA23C3889A403"
    public bool ConnectServer(string host, int port, ServerType serverType,
        ClientProtocolType eProtoType = ClientProtocolType.NORMAL,
        string HandShakeCache = "",
        byte[] clientcert = null, string clientpwd = "", string cathumbprint = null)
    {
        //if (eProtoType == ClientProtocolType.TLS)
        //{
        //    if (clientcert == null || cathumbprint == null)
        //    {
        //        return false;
        //    }
        //}

        //this.CloseClient();
        pc = new PomeloClient(eProtoType, clientcert, "", cathumbprint);
        pc.Connect(host, port, HandShakeCache, delegate ()
        {
            if (pc.IsConnected)
            {
                this.UpdateClient();
                pc.HandShake(null, delegate (JsonData data)
                {
                    //if (this.connectEvent != null)
                    //{
                    //    this.connectEvent();
                    //}
                    if (serverType == ServerType.GATE)
                    {
                        OnConnectToGate();
                    }
                    else if (serverType == ServerType.CONNECTOR)
                    {
                        OnConnectToConnector();
                    }
                });
            }           
        },
        delegate ()
        {
            if (this.closeEvent != null)
            {
                this.closeEvent();
            }

        }
        );

        return true;
    }


    private void UpdateClient()
    {
        Proto.Version.resetClient(pc);
    }

    public static object[] CallMethod(string func, params object[] args)
    {
        return Util.CallMethod("Pomelo", func, args);
    }

    public void SendRequest(string route, JsonData msg, LuaFunction call_back)
    {
        pc.request(route, msg, call_back);
    }

    public void Notify(string route, JsonData msg)
    {
        pc.notify(route, msg);
    }

    public void On(string eventName, LuaFunction call_back)
    {
        pc.on(eventName, call_back);
    }
}
