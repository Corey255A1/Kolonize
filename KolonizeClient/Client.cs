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
        public string PlayerName = "Player1";
        TcpClient theClient;        
        StreamProcessor StreamController;
        public Client(string playername)
        {
            PlayerName = playername;
            theClient = new TcpClient("localhost", 15647);
            StreamController = new StreamProcessor(theClient.GetStream(), PacketProcessors.ProcessPacket);
        }
        public void RegisterForPlayerUpdates(PlayerUpdate p)
        {
            PacketProcessors.PlayerUpdateEvent += p;
        }
        public void RegisterForObjectUpdates(ObjectUpdate p)
        {
            PacketProcessors.ObjectUpdateEvent += p;
        }

        public void RegisterForCellInfo(CellInfoUpdate c)
        {
            PacketProcessors.CellInfoUpdateEvent += c;
        }

        //What we should do instead is get a larger region (if not the whole region)
        //Reduce the amount of times the cells need paged in.
        public void GetRegionCells(int x1, int x2, int y1, int y2)
        {
            //pauseAsync = true;
            RegionInfo ri = new RegionInfo(PacketTypes.REQUEST);
            ri.x1 = x1;
            ri.x2 = x2;
            ri.y1 = y1;
            ri.y2 = y2;

            int count = (ri.x2 - ri.x1) * (ri.y2 - ri.y1);

            byte[] b = NetHelpers.ConvertStructToBytes(ri);

            StreamController.StreamWrite(b);

        }
        public void GetPlayerInfo()
        {
            PlayerInfo playa = new PlayerInfo(PacketTypes.REQUEST);
            playa.id = PlayerName;
            byte[] b = NetHelpers.ConvertStructToBytes(playa);
            StreamController.StreamWrite(b);
        }

        public void SetPlayerDirection(int dir, int moveCount)
        {
            PlayerControl playa = new PlayerControl(PacketTypes.SET);
            playa.id = PlayerName;
            playa.direction = dir;
            playa.paces = moveCount;
            byte[] b = NetHelpers.ConvertStructToBytes(playa);
            StreamController.StreamWrite(b);
        }

        public void CreateMarker()
        {
            PlayerPerformAction p = new PlayerPerformAction(PacketTypes.SET);
            p.id = PlayerName;
            p.key = "";
            p.ActionID = 0;
            StreamController.StreamWrite(NetHelpers.ConvertStructToBytes(p));
        }

    }
}
