using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

using KolonizeNet;
namespace KolonizeClient
{
    public class Client
    {
        TcpClient theClient;
        NetworkStream theStream;
        public string PlayerName = "Player1";
        byte[] respbuff = new byte[1000];
        bool pauseAsync = false;
        public Client(string playername)
        {
            PlayerName = playername;
            theClient = new TcpClient("localhost", 15647);
            theStream = theClient.GetStream();            
        }
        public void StartAsyncUpdate(PlayerUpdate p)
        {
            PacketProcessors.PlayerUpdateEvent += p;
            theStream.BeginRead(respbuff, 0, respbuff.Length, ServerRead, this);
        }
        public void RestartAsyncUpdate()
        {
            theStream.BeginRead(respbuff, 0, respbuff.Length, ServerRead, this);
        }
        public void ServerRead(IAsyncResult ar)
        {
            if (pauseAsync) return;
            try
            {
                int bytes = theStream.EndRead(ar);
                if (bytes > 0)
                {
                    int off = 0;
                    var packetData = NetHelpers.GetHeaderInfo(respbuff, ref off);
                    PacketProcessors.ProcessPacket(packetData.Item1, packetData.Item2, respbuff);
                }
                theStream.BeginRead(respbuff, 0, respbuff.Length, ServerRead, this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public List<CellInfo> GetRegionCells(int x1, int x2, int y1, int y2)
        {
            pauseAsync = true;
            RegionInfo ri = new RegionInfo(PacketTypes.REQUEST);
            ri.x1 = x1;
            ri.x2 = x2;
            ri.y1 = y1;
            ri.y2 = y2;

            int count = (ri.x2 - ri.x1) * (ri.y2 - ri.y1);

            byte[] b = NetHelpers.ConvertStructToBytes(ri);

            theStream.Write(b, 0, b.Length);
            
            var celllist = new List<CellInfo>();
            
            int read = 0;
            int offset = 0;
            int readoffset = 0;
            //Expect CellInfo
            while (count>0)
            {
                read = theStream.Read(respbuff, readoffset, respbuff.Length - readoffset);
                if(read>0)
                {
                    read += readoffset;
                    readoffset = 0;
                    offset = 0;
                    do
                    {
                        var cell = NetHelpers.ConvertBytesToStruct<CellInfo>(respbuff, ref offset);
                        if (cell.header.dataType == (byte)DataTypes.CELL_INFO)
                        {
                            count = cell.remainingCells;
                            celllist.Add(cell);                            
                        }
                        else
                        {
                            readoffset = respbuff.Length - offset;
                            Array.Copy(respbuff, offset, respbuff, 0, readoffset);
                            offset = read;
                        }
                    } while (offset < read);
                    
                }                

            }
            pauseAsync = false;
            return celllist;

        }
        public PlayerInfo GetPlayer()
        {
            PlayerInfo playa = new PlayerInfo(PacketTypes.REQUEST);
            playa.id = PlayerName;
            byte[] b = NetHelpers.ConvertStructToBytes(playa);
            theStream.Write(b, 0, b.Length);

            int read = theStream.Read(respbuff, 0, respbuff.Length);
            int offset = 0;
            if(read > 0)
            {
                return NetHelpers.ConvertBytesToStruct<PlayerInfo>(respbuff, ref offset);
            }
            return playa;
        }
        public void SetPlayerVelocity(float vx, float vy)
        {
            PlayerInfo playa = new PlayerInfo(PacketTypes.SET);
            playa.id = PlayerName;
            playa.vx = vx;
            playa.vy = vy;
            byte[] b = NetHelpers.ConvertStructToBytes(playa);
            theStream.Write(b, 0, b.Length);
        }

    }
}
