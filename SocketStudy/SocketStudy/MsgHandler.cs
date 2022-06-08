using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace SocketStudy
{
    internal class MsgHandler
    {
        public static void MsgEnter(ClientState cs,string msgArgs)
        {
            Console.WriteLine("MsgEnter"+msgArgs);
            string[] split=msgArgs.Split(',');
            string desc=split[0]; 
            float x=float.Parse(split[1]);
            float y=float.Parse(split[2]); 
            float z=float.Parse(split[3]);
            float eulY=float.Parse(split[4]);

            cs.hp = 100;
            cs.x = x;
            cs.y = y;
            cs.z = z;
            cs.eulY = eulY;

            string sendStr = "Enter|" + msgArgs;
            foreach(ClientState c in MainClass.Clients.Values)
            {
                MainClass.Send ( c, sendStr );
            }
        }
        public static void MsgList( ClientState cs, string msgArgs )
        {
            Console.WriteLine ( "MsgEnter" + msgArgs );
            string sendStr = "List|";
            foreach(ClientState c in MainClass.Clients.Values)
            {
                sendStr += c.socket.RemoteEndPoint.ToString ( ) + ",";
                sendStr += c.x.ToString ( ) + ",";
                sendStr += c.y.ToString ( ) + ",";
                sendStr += c.z.ToString ( ) + ",";
                sendStr += c.eulY.ToString ( ) + ",";
                sendStr += c.hp.ToString ( ) + ",";
            }
            MainClass.Send ( cs, sendStr ); 
        }
        public static void MsgMove(ClientState cs ,string msgArgs)
        {
            string[] split = msgArgs.Split ( ',' );
            string desc = split[0];
            float x = float.Parse ( split[1] );
            float y = float.Parse ( split[2] );
            float z = float.Parse ( split[3] );
           
            
            cs.x = x;
            cs.y = y;
            cs.z = z;

            string sendStr = "Move|" + msgArgs;
            foreach (ClientState c in MainClass.Clients.Values)
            {
                MainClass.Send ( c, sendStr );
            }
        }
        public static void MsgAttack( ClientState cs,string msgArgs)
        {
            string sendStr = "Attack|"+msgArgs;
            foreach (ClientState c in MainClass.Clients.Values)
            {
                MainClass.Send ( c, sendStr );
            }
        }

    }
}
