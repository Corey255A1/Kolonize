using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using KolonizeNet;
using Universe;
using System.Net.WebSockets;
namespace KolonizeServer
{    

    public class Server
    {
        TcpListener theServer;
        HttpListener theHttpListener = new HttpListener();
        WorldNetIF theWorldNet;
        List<ClientHandler> clientList = new List<ClientHandler>();
        public ClientUpdate ClientStatusUpdate;
        private bool ServerActive = false;
        public Server()
        {
            ServerActive = true;
            theServer = new TcpListener(IPAddress.Any , 15647);
            theHttpListener.Prefixes.Add("http://localhost:15648/");
            theWorldNet = new WorldNetIF(this);
            
            theServer.Start();//Before Hosting this to the world, have to open the port. Otherwise get an Access Denied           
            theServer.BeginAcceptTcpClient(ClientConnected, theServer);
            BeginWebSocketHandler();
        }


        private async void BeginWebSocketHandler()
        {
            theHttpListener.Start();
            while(ServerActive)
            {
                var httpcontext = await theHttpListener.GetContextAsync();
                if (httpcontext.Request.IsWebSocketRequest)
                {
                    Console.WriteLine("Got A Websocket!!!!!!!!!!");
                    var websocketcontext = await httpcontext.AcceptWebSocketAsync(null);
                    var ch = new WebSocketClientHandler(websocketcontext.WebSocket, httpcontext.Request.RemoteEndPoint.Address.ToString());
                    ch.ClientStatusEvent += ClientStatus;
                    clientList.Add(ch);
                    ClientStatusUpdate?.Invoke(ch, "Connected");
                }
                else
                {
                    httpcontext.Response.StatusCode = 404;
                    httpcontext.Response.Close();
                }
            }

        }

        private void ClientConnected(IAsyncResult ar)
        {
            try
            {
                TcpClient t = theServer.EndAcceptTcpClient(ar);
                var ch = new TCPClientHandler(t);
                ch.ClientStatusEvent += ClientStatus;
                clientList.Add(ch);
                ClientStatusUpdate?.Invoke(ch, "Connected");
                theServer.BeginAcceptTcpClient(ClientConnected, theServer);
            }
            catch
            {
                Console.WriteLine("Something weird happened");
            }
        }
        private void ClientStatus(ClientHandler ch, string msg)
        {
            if(msg == "Disconnected")
            {
                clientList.Remove(ch);
            }
            ClientStatusUpdate?.Invoke(ch, msg);
        }

        public void Close()
        {
            theServer.Stop();
            theServer = null;
        }

        public void BroadcastToClients(byte[] b)
        {
            foreach (var c in clientList)
            {
                c.SendPacket(b);
            }
        }
        public void SendToPlayer(string playerID, byte[] b)
        {
            foreach (var c in clientList)
            {
                if(c.PlayerID == playerID)
                {
                    c.SendPacket(b);
                }//Could be more than 1 client per player
            }
        }








    }
}
