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
        NetManager.AddMsgListener("MsgMove",OnMsgMove);
    }

    private void Update()
    {
        NetManager.Update();
    }

    private void OnMsgMove(MsgBase msgbase)
    {
        BattleMsg.MsgMove msgMove = (BattleMsg.MsgMove)msgbase;
        Debug.Log("OnMsgMove msg.x ="+ msgMove.x);
        Debug.Log("OnMsgMove msg.y ="+ msgMove.y);
        Debug.Log("OnMsgMove msg.z ="+ msgMove.z);

    }

    public void onConnectClick()
    {
        //NetManager.Connect ( "curve-game.club", 8888 );
        NetManager.Connect ( "192.168.56.1", 8888 );

    }

    public void OnCloseClick()
    {
        NetManager.Close();
    }

    public void OnMoveClick()
    {
        BattleMsg.MsgMove msg = new BattleMsg.MsgMove();
        msg.x = 10;
        msg.y = 20;
        msg.z = 32;
        NetManager.Send(msg);
    }
    private void onConnectSucc( string err )
    {
        Debug.Log( "ConnectSucc" );
    }

    private void onConnectClose( string err )
    {
        Debug.LogError ( "ConnectClose" );       
    }

    private void onConnectFail( string err )
    {

        Debug.LogError ( "ConnectFail"+ err );
    }
    
}