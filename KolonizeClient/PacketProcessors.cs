using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KolonizeNet;

namespace KolonizeClient
{
    public delegate void PlayerUpdate(PlayerInfo p);
    public delegate void ObjectUpdate(ObjectInfo p);
    public delegate void CellInfoUpdate(CellInfo c);
    public static class PacketProcessors
    {
        public static PlayerUpdate PlayerUpdateEvent;
        public static ObjectUpdate ObjectUpdateEvent;
        public static CellInfoUpdate CellInfoUpdateEvent;

        public static bool ProcessPacket(StreamWriter s, PacketTypes p, DataTypes d, byte[] buff, ref int offset)
        {
            switch (d)
            {
                //Package the Region Data and Send it
                case DataTypes.CELL_INFO: ProcessCellInfo(s, p, buff, ref offset); return true;
                case DataTypes.REGION_INFO: ProcessRegionInfo(s, p, buff, ref offset); return true;
                case DataTypes.PLAYER_INFO: ProcessPlayerInfo(s, p, buff, ref offset); return true;
                case DataTypes.OBJECT_INFO: ProcessObjectInfo(s, p, buff, ref offset); return true;
                default: return false;

            }

        }
        private static void ProcessPlayerInfo(StreamWriter s, PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {               
                case PacketTypes.REQUESTED:
                case PacketTypes.UPDATE:
                    {
                        PlayerInfo pi = NetHelpers.ConvertBytesToStruct<PlayerInfo>(buff, ref offset);
                        PlayerUpdateEvent?.Invoke(pi);

                    }
                    break;
            }
        }
        private static void ProcessObjectInfo(StreamWriter s, PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {
                case PacketTypes.REQUESTED:
                case PacketTypes.UPDATE:
                    {
                        ObjectInfo pi = NetHelpers.ConvertBytesToStruct<ObjectInfo>(buff, ref offset);
                        ObjectUpdateEvent?.Invoke(pi);

                    }
                    break;
            }
        }
        private static void ProcessRegionInfo(StreamWriter s, PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {
                case PacketTypes.UPDATE:
                    {
                        
                    }break;
            }
        }
        private static void ProcessCellInfo(StreamWriter s, PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {
                case PacketTypes.REQUESTED:
                    {
                        CellInfo cell = NetHelpers.ConvertBytesToStruct<CellInfo>(buff, ref offset);
                        CellInfoUpdateEvent?.Invoke(cell);
                    }
                    break;
            }
        }

    }
}
