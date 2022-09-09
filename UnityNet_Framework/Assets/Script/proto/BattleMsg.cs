using System;
using UnityEngine;

public class BattleMsg
{
    public class MsgMove : MsgBase
    {
        public MsgMove()
        {
            protoName = "MsgMove";
        }

        public int x = 0;
        public int y = 0;
        public int z = 0;

    }

    public class MsgAttack : MsgBase
    {
        public MsgAttack()
        {
            protoName = "MsgAttack";
        }

        public string desc = " 49.234.67.58:6543";
    }

}
