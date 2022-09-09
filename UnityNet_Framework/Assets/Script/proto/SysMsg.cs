using System;
using UnityEngine;
  public class SysMsg
  {
    public class MsgPing : MsgBase
    {
      public MsgPing()
      {
        protoName = "MsgPing";
      }
    }
    public class MsgPong : MsgBase
    {
      public MsgPong()
      {
        protoName = "MsgPong";
      }
    }
  }
