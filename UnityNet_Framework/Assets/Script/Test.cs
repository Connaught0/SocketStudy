using System;
using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{


    void Start( )
    {
        NetManager.AddEventListener ( NetManager.NetEvent.ConnectSucc, onConnectSucc );
        NetManager.AddEventListener ( NetManager.NetEvent.ConnectFail, onConnectFail );
        NetManager.AddEventListener ( NetManager.NetEvent.Close, onConnectClose );
    }
    public void onConnectClick()
    {
        NetManager.Connect ( "192.168.0.25", 8888 );

    }
    private void onConnectSucc( string err )
    {
        Debug.Log( "ConnectSucc" );
    }

    private void onConnectClose( string err )
    {
        Debug.LogError ( "ConnectFail"+ err );
    }

    private void onConnectFail( string err )
    {
        Debug.LogError ( "ConnectClose" );
    }
}