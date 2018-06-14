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
        TcpClient theClient;
        NetworkStream theStream;
        byte[] buff = new byte[512];
        public ClientHandler(TcpClient t)
        {
            theClient = t;
            theStream = theClient.GetStream();
            theStream.BeginRead(buff, 0, buff.Length, ClientRead, this);
        }
        public override string ToString()
        {
            return theClient.Client.RemoteEndPoint.ToString();
        }

        public void SendPacket(byte[] b)
        {
            theStream.Write(b, 0, b.Length);
        }
        private void ClientRead(IAsyncResult ar)
        {
            try
            {
                int bytes = theStream.EndRead(ar);
                if (bytes > 0)
                {
                    int off = 0;
                    var packetData = NetHelpers.GetHeaderInfo(buff, off);
                    foreach (var packet in PacketProcessors.ProcessPacket(packetData.Item1, packetData.Item2, buff))
                    {
                        if (packet != null)
                        {
                            theStream.Write(packet, 0, packet.Length);
                        }
                    }

                }
                theStream.BeginRead(buff, 0, buff.Length, ClientRead, this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ClientStatusEvent?.Invoke(this, "Disconnected");
            }
        }
    }
}
