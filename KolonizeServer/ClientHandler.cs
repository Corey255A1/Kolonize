using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using KolonizeNet;
namespace KolonizeServer
{
    public delegate void ClientUpdate(ClientHandler ch, string msg);
    public class ClientHandler
    {
        public ClientUpdate ClientStatusEvent;
        public string PlayerID
        {
            get
            {
                if (theProcessor == null) return "";
                else return theProcessor.PlayerID;
            }
        }
        TcpClient theClient;
        StreamProcessor StreamController;
        PlayerProcessor theProcessor;
        public ClientHandler(TcpClient t)
        {
            theClient = t;
            theProcessor = new PlayerProcessor();
            StreamController = new StreamProcessor(theClient.GetStream(), theProcessor.ProcessPacket);
            StreamController.StreamStatusEvent += StreamStatus;
        }
        public override string ToString()
        {
            return theClient.Client.RemoteEndPoint.ToString();
        }

        private void StreamStatus(StreamProcessor sp, string msg)
        {
            ClientStatusEvent?.Invoke(this, msg);
        }

        public void SendPacket(byte[] b)
        {
            try
            {
                StreamController.StreamWrite(b);
            }
            catch
            {
                ClientStatusEvent?.Invoke(this, "Disconnected");
            }
        }
    }
}
