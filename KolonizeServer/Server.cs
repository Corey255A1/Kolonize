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
        public Server()
        {
            
            theServer = new TcpListener(IPAddress.Any , 15647);
            theHttpListener.Prefixes.Add("http://localhost:15648/");
            theWorldNet = new WorldNetIF(this);
            theServer.Start();//Before Hosting this to the world, have to open the port. Otherwise get an Access Denied
            theHttpListener.Start();
            theServer.BeginAcceptTcpClient(ClientConnected, theServer);
            theHttpListener.BeginGetContext(HTTPClientConnected, this);
        }

        private void HTTPClientConnected(IAsyncResult ar)
        {
            var context = theHttpListener.EndGetContext(ar);
            if(context.Request.IsWebSocketRequest)
            {
                Console.WriteLine("Got A Websocket!!!!!!!!!!");
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
            theHttpListener.BeginGetContext(HTTPClientConnected, this);
        }

        private void ClientConnected(IAsyncResult ar)
        {
            try
            {
                TcpClient t = theServer.EndAcceptTcpClient(ar);
                var ch = new ClientHandler(t);
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
