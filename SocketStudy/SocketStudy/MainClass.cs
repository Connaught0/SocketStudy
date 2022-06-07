using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Reflection;


namespace SocketStudy
{
    class MainClass
    {
     
        static Socket listenfd;//监听
        public static Dictionary<Socket, ClientState> Clients = new Dictionary<Socket, ClientState>();


        static void Main(string[] args)
        {
            Console.WriteLine("Start---------");
            //socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipdr, 8888);
            listenfd.Bind(ipEp);

            listenfd.Listen(0);
            Console.WriteLine("Server Start---------");
            #region            PollServer 
            /*
            while(true)
            {
                if (listenfd.Poll(0, SelectMode.SelectRead))
                {
                    Readlistenfd(listenfd);
                }
                foreach(ClientState s in Clients.Values)
                {
                    Socket clientfd = s.socket;
                    if(clientfd.Poll(0,SelectMode.SelectRead))
                    {
                        if(!ReadClientfd(clientfd))
                        {
                            break;
                        }
                    }
                }
                System.Threading.Thread.Sleep(1);
            }
            */
            #endregion
            #region Async
            /*
             listenfd.BeginAccept(AcceptCallback, listenfd);
             Console.ReadLine();
            */
            #endregion
            #region SelectServer
            List<Socket> checkRead = new List<Socket>();
            while(true)
            {
                checkRead.Clear();
                checkRead.Add(listenfd);
                foreach(ClientState cs in Clients.Values)
                {
                    checkRead.Add(cs.socket);
                }
                Socket.Select(checkRead, null, null, 1000);
                foreach(Socket s in checkRead)
                {
                    if(s==listenfd)
                    {
                        Readlistenfd(s);
                    }
                    else
                    {
                        ReadClientfd(s);
                    }

                }
            }
            #endregion
        }
        #region PollServerMethod
        private static bool ReadClientfd(Socket clientfd)
        {
            ClientState state = Clients[clientfd];
            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuff);
            }
            catch(SocketException ex)
            {
                MethodInfo mei = typeof ( EventHandler ).GetMethod ( "OnDisconnect" );
                object[] ob = { state };
                mei.Invoke(null, ob);


                clientfd.Close();
                Clients.Remove(clientfd);
                Console.WriteLine("Receive SocketException" + ex.ToString());
                return false;
            }
            if (count == 0)
            {
                MethodInfo mei = typeof ( EventHandler ).GetMethod ( "OnDisconnect" );
                object[] ob = { state };
                mei.Invoke ( null, ob );

                clientfd.Close();
                Clients.Remove(clientfd);
                Console.WriteLine("Socket Close");
                return false;
            }
            string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
            Console.WriteLine("Receive" + recvStr);
            string[] split=recvStr.Split('|');
            string msgName = split[0];
            string msgArgs = split[1];
            string funName = "Msg" + msgName;
            MethodInfo mi = typeof ( MsgHandler ).GetMethod ( funName );
            object[] o ={ state,msgArgs};
            mi.Invoke(null, o); 
            

            /*  Echo Boardcast
            //string sendStr = clientfd.RemoteEndPoint.ToString()+":" + recvStr;
            string sendStr =  recvStr;

            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            foreach (ClientState cs in Clients.Values)
            {
                cs.socket.Send(sendBytes);
            }
            */
            return true;

        }

        private static void Readlistenfd(Socket listenfd)
        {
            Console.WriteLine("Accept");
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            Clients.Add(clientfd, state);
        }
        #endregion
        #region AsyncServerMethod
        private static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("Server Accept--------");
                Socket listenfd = ar.AsyncState as Socket;
                //listenfd end and pass it to clientfd
                Socket clientfd = listenfd.EndAccept(ar);
                //put clientfd to clientlist
                ClientState state = new ClientState();
                state.socket = clientfd;
                Clients.Add(clientfd, state);
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
                listenfd.BeginAccept(AcceptCallback, listenfd);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Socket Accept fail" + ex.ToString());

            }

        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                ClientState state = ar.AsyncState as ClientState;
                Socket clientfd = state.socket;
                int count = clientfd.EndReceive(ar);
                if(count==0)
                {
                    clientfd.Close();
                    Clients.Remove(clientfd);
                    Console.WriteLine("Socket Close");
                    return;
                }
                string recvStr = System.Text.Encoding.Default.GetString(state.readBuff,0,count);
                string sendStr = clientfd.RemoteEndPoint.ToString() + ":" + recvStr;
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
                foreach(ClientState s in Clients.Values)
                {
                    s.socket.Send(sendBytes);
                }
                /*echo
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes("echo" + recvStr);
                clientfd.Send(sendBytes);
                */
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
                 
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Socket Receive fail" + ex.ToString());
            }
        }
        #endregion
        public static void Send(ClientState cs,string msg)
        {
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(msg);
            cs.socket.Send(sendBytes);
        }
    }
}
