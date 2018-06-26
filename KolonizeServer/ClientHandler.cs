using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Net;
using KolonizeNet;
namespace KolonizeServer
{
    public delegate void ClientUpdate(ClientHandler ch, string msg);

    public abstract class ClientHandler
    {
        protected StreamProcessor StreamController;
        protected PlayerProcessor theProcessor;
        public ClientUpdate ClientStatusEvent;
        public string PlayerID
        {
            get
            {
                if (theProcessor == null) return "";
                else return theProcessor.PlayerID;
            }
        }
        public abstract void SendPacket(byte[] b);
    }
    

    public class TCPClientHandler: ClientHandler
    {

        TcpClient theClient;

        public TCPClientHandler(TcpClient t)
        {
            theClient = t;
            theProcessor = new PlayerProcessor();
            StreamController = new NetworkStreamProcessor(theClient.GetStream(), theProcessor.ProcessPacket);
            ((NetworkStreamProcessor)StreamController).StreamStatusEvent += StreamStatus;
        }
        public override string ToString()
        {
            return theClient.Client.RemoteEndPoint.ToString();
        }

        private void StreamStatus(string msg)
        {
            ClientStatusEvent?.Invoke(this, msg);
        }

        public override void SendPacket(byte[] b)
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

    public class WebSocketClientHandler: ClientHandler
    {
        string IP;
        public WebSocketClientHandler(WebSocket ws, string ip)
        {
            IP = ip;
            theProcessor = new PlayerProcessor();
            StreamController = new WebSocketStreamProcessor(ws, theProcessor.ProcessPacket);
            ((WebSocketStreamProcessor)StreamController).StreamStatusEvent += StreamStatus;
        }
        public override string ToString()
        {
            return IP;
        }

        private void StreamStatus(string msg)
        {
            ClientStatusEvent?.Invoke(this, msg);
        }

        public override void SendPacket(byte[] b)
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
