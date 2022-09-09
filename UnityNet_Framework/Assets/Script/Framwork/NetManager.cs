using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

public static class NetManager
{
    static Socket socket;
    static ByteArray readBuff;
    static Queue<ByteArray> writeQueue;

    #region 状态转变中

    static bool isClosing = false;
    static bool isConnecting = false;    

    #endregion

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

    #region 消息监听
    public delegate void MsgListener(MsgBase msgBase);

    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
    }

    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    private static void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }
    #endregion

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
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
            isConnecting = false;
        }
        catch(SocketException ex)
        {
            Debug.LogError("Socket Connect fail"+ex.ToString());
            FireEvent ( NetEvent.ConnectFail, ex.ToString ( ) );
            isConnecting=false;
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count == 0)
            {
                Close();
                return;
            }

            readBuff.writeIdx += count;
            OnReceiveData();
            if (readBuff.remain < 8)
            {
                readBuff.moveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }

            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);

        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail"+ex.ToString());
        }
    }

    public static void OnReceiveData()
    {
        if (readBuff.length <= 2)
        {
            return;
        }

        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if(readBuff.length<bodyLength)
            return;
        readBuff.readIdx += 2;
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            Debug.Log("OnReceiveData MsgBase.deCodename Fail");
            return;
        }

        readBuff.readIdx += nameCount;
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        lock (msgList)
        {
            msgList.Add(msgBase);
        }

        msgCount++;
        if (readBuff.length > 2)
        {
            OnReceiveData();
        }
    }

    public static void Close()
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }

        if (isConnecting)
        {
            return;
        }

        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close,"");
        }
    }

    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }

        if (isConnecting)
        {
            return;
        }

        if (isClosing)
        {
            return;
        }

        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes,0,sendBytes,2,nameBytes.Length);
        Array.Copy(bodyBytes,0,sendBytes,2+nameBytes.Length,bodyBytes.Length);
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }

        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected)
        {
            return;
        }

        int count = socket.EndSend(ar);
        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }

        ba.readIdx += count;
        if (ba.length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.First();

            }
        }

        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
        }
        else if(isClosing)
        {
            socket.Close();
        }
    }

    #region 心跳机制

    public static bool isUsePing = true;
    public static int pingInterval = 30;//心跳间隔
    private static float lastPingTime = 0;
    private static float lastPongTime = 0;

    private static void PingUpdate()
    {
        if (!isUsePing) return;
        if (Time.time - lastPingTime > pingInterval)
        {
            SysMsg.MsgPing msgPing = new SysMsg.MsgPing();
            Send(msgPing);
            lastPingTime = Time.time;
        }

        if (Time.time - lastPongTime > pingInterval * 4)
        {
            Close();
        }
    }

    #endregion

    private static List<MsgBase> msgList = new List<MsgBase>();
    private static int msgCount = 0;
    private readonly static int MAX_MESSAGE_FIRE = 10;

    private static void initState()
    {
        socket=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue=new Queue<ByteArray> ();
        isConnecting = false;
        isClosing = false;
        msgList = new List<MsgBase>();
        msgCount = 0;
        lastPingTime = Time.time;
        lastPongTime = Time.time;
        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong",OnMsgPong);
        }
    }

    private static void OnMsgPong(MsgBase msgbase)
    {
        lastPongTime = Time.time;
    }


    public static  void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    public static void MsgUpdate()
    {
        if (msgCount == 0)
        {
            return;
        }

        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }

            if (msgBase != null)
            {
                FireMsg(msgBase.protoName,msgBase);
            }
            else
            {
                break;
            }
        }
    }
}

