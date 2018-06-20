using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using KolonizeNet;
using Universe;
namespace KolonizeServer
{    

    public class Server
    {
        TcpListener theServer;
        WorldNetIF theWorldNet;
        List<ClientHandler> clientList = new List<ClientHandler>();
        public ClientUpdate ClientStatusUpdate;
        public Server()
        {
            
            theServer = new TcpListener(IPAddress.Any , 15647);
            theWorldNet = new WorldNetIF(this);
            theServer.Start();
            theServer.BeginAcceptTcpClient(ClientConnected, theServer);

            WorldInterface.AddMoverUpdateCB(theWorldNet.MoverUpdated);
        }

        private void ClientConnected(IAsyncResult ar)
        {
            if (theServer == null) return;
            TcpClient t = theServer.EndAcceptTcpClient(ar);
            var ch = new ClientHandler(t);
            ch.ClientStatusEvent += ClientStatus;
            clientList.Add(ch);
            ClientStatusUpdate?.Invoke(ch, "Connected");
            theServer.BeginAcceptTcpClient(ClientConnected, theServer);
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
