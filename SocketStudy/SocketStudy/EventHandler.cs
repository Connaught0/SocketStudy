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
        }

    }
}
