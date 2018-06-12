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
        List<ClientHandler> clientList = new List<ClientHandler>();
        public ClientUpdate ClientStatusUpdate;
        public Server()
        {
            
            theServer = new TcpListener(IPAddress.Any , 15647);
            theServer.Start();
            theServer.BeginAcceptTcpClient(ClientConnected, theServer);
            WorldInterface.AddMoverUpdateCB(MoverUpdated);
        }
        public void Close()
        {
            theServer.Stop();
        }

        private void ClientConnected(IAsyncResult ar)
        {
            TcpClient t = theServer.EndAcceptTcpClient(ar);
            var ch = new ClientHandler(t);
            ch.ClientStatusEvent += ClientStatus;
            clientList.Add(ch);
            ClientStatusUpdate?.Invoke(ch, "Connected");
            theServer.BeginAcceptTcpClient(ClientConnected, theServer);
        }

        private void MoverUpdated(Moveable m)
        {
            var coord = m.GetPosition();
            var v = m.GetVelocity();
            var pi = new PlayerInfo(PacketTypes.UPDATE)
            {
                id = m.Id,
                x = coord.x,
                y = coord.y,
                vx = v.x,
                vy = v.y
            };
            byte[] b = NetHelpers.ConvertStructToBytes(pi);
            foreach (var c in clientList)
            {
                c.SendPacket(b);
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



    }
}
