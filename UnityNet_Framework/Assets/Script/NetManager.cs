using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Linq;
using System.Net.Http;

public static class NetManager
{
    static Socket socket;
    static ByteArray readBuff;
    static Queue<ByteArray> writeQueue;


    public enum NetEvent
    {
        ConnectSucc=1,
        ConnectFail=2,
        Close=3,
    }

    
    #region 事件监听
    public delegate void EventListener( string err );
    private static Dictionary<NetEvent, EventListener> eventListeners=new Dictionary<NetEvent, EventListener>();
/// <summary>
/// 添加事件监听
/// </summary>
/// <param name="netEvent">网络状态</param>
/// <param name="listener">事件</param>
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if(eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent]=listener;
        }
    }

    public static void DeleEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
        }
        if (eventListeners[netEvent]==null)
        {
            eventListeners.Remove(netEvent);
        }
    }

    private static void FireEvent(NetEvent netEvent,string err)
    {
        if(eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] ( err );
        }
    }

    #endregion

    static bool isConnecting = false;
    public static void Connect(string ip, int port)
    {
        if(socket!=null&&socket.Connected)
        {
            Debug.Log ( "Connect fail, Already Connect" );
            return;
        }
        if(isConnecting)
        {
            Debug.Log ( "Connect fail, isConnecting" );
            return;
        }
        initState ( );//Clean the readBuff
        socket.NoDelay = true;
        isConnecting = true;
        socket.BeginConnect ( ip, port, ConnectCallback, socket );
    }

    private static void ConnectCallback( IAsyncResult ar )
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect ( ar );
            Debug.Log ( "Scoket Connect succ" );
            FireEvent ( NetEvent.ConnectSucc, "" );
            isConnecting = false;
        }
        catch(SocketException ex)
        {
            Debug.LogError("Socket Connect fail"+ex.ToString());
            FireEvent ( NetEvent.ConnectFail, ex.ToString ( ) );
            isConnecting=false;
        }
    }

    private static void initState()
    {
        socket=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue=new Queue<ByteArray> ();
        isConnecting = false;
    }

}

