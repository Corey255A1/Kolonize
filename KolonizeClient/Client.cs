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
        public string PassKey = "";
        TcpClient theClient;        
        NetworkStreamProcessor StreamController;
        public Client(string playername, string password, string hostname)
        {
            PlayerName = playername;
            PassKey = password; //Definitely not secure ... Don't use your normal password! ;)
            theClient = new TcpClient(hostname, 15647);
            StreamController = new NetworkStreamProcessor(theClient.GetStream(), PacketProcessors.ProcessPacket);
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
            ri.regionDataTypes = WorldConstants.REGION_INFO_ALL;
            StreamController.StreamWrite(NetHelpers.ConvertStructToBytes(ri));

        }
        public void GetPlayerInfo()
        {
            PlayerInfo playa = new PlayerInfo(PacketTypes.REQUEST);
            playa.id = PlayerName;
            playa.key = PassKey;
            byte[] b = NetHelpers.ConvertStructToBytes(playa);
            StreamController.StreamWrite(b);
        }

        public void SetPlayerDirection(int dir, int moveCount)
        {
            PlayerControl playa = new PlayerControl(PacketTypes.SET);
            playa.id = PlayerName;
            playa.key = PassKey;
            playa.direction = dir;
            playa.paces = moveCount;
            byte[] b = NetHelpers.ConvertStructToBytes(playa);
            StreamController.StreamWrite(b);
        }

        public void CreateMarker()
        {
            PlayerPerformAction p = new PlayerPerformAction(PacketTypes.SET);
            p.id = PlayerName;
            p.key = PassKey;
            p.ActionID = 0;
            StreamController.StreamWrite(NetHelpers.ConvertStructToBytes(p));
        }

    }
}
