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
                sendStr += cs.socket.RemoteEndPoint.ToString ( ) + ",";
                sendStr += cs.x.ToString ( ) + ",";
                sendStr += cs.y.ToString ( ) + ",";
                sendStr += cs.z.ToString ( ) + ",";
                sendStr += cs.eulY.ToString ( ) + ",";
                sendStr += cs.hp.ToString ( ) + ",";
            }
            MainClass.Send ( cs, sendStr ); 
        }

    }
}
