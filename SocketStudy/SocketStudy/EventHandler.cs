using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketStudy
{
    class EventHandler
    {
        public static void OnDisconnect(ClientState cs)
        {
            Console.WriteLine("OnDisconnect"+cs.socket.RemoteEndPoint.ToString());
            string desc = cs.socket.RemoteEndPoint.ToString ( );
            string sendStr = "Leave|" + desc + ",";
            foreach(ClientState c in MainClass.Clients.Values)
            {
                MainClass.Send ( c, sendStr );
            }
        }

    }
}
