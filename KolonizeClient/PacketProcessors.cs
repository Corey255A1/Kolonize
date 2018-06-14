using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KolonizeNet;

namespace KolonizeClient
{
    public delegate void PlayerUpdate(PlayerInfo p);
    public delegate void CellInfoUpdate(CellInfo c);
    public static class PacketProcessors
    {
        public static PlayerUpdate PlayerUpdateEvent;
        public static PlayerUpdate MyPlayerInfoEvent;
        public static CellInfoUpdate CellInfoUpdateEvent;

        public static bool ProcessPacket(PacketTypes p, DataTypes d, byte[] buff, ref int offset)
        {
            switch (d)
            {
                //Package the Region Data and Send it
                case DataTypes.CELL_INFO: ProcessCellInfo(p, buff, ref offset); return true;
                case DataTypes.REGION_INFO: ProcessRegionInfo(p, buff, ref offset); return true;
                case DataTypes.PLAYER_INFO: ProcessPlayerInfo(p, buff, ref offset); return true;
                case DataTypes.OBJECT_INFO: ProcessObjectInfo(p, buff, ref offset); return true;
                default: return false;

            }

        }
        private static void ProcessPlayerInfo(PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {
                case PacketTypes.REQUESTED:
                    {
                        PlayerInfo pi = NetHelpers.ConvertBytesToStruct<PlayerInfo>(buff, ref offset);
                        MyPlayerInfoEvent?.Invoke(pi);
                        MyPlayerInfoEvent = null;//One Shot
                    }
                    break;
                case PacketTypes.UPDATE:
                    {
                        PlayerInfo pi = NetHelpers.ConvertBytesToStruct<PlayerInfo>(buff, ref offset);
                        PlayerUpdateEvent?.Invoke(pi);

                    }
                    break;
            }
        }
        private static void ProcessObjectInfo(PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {
                case PacketTypes.REQUESTED:
                case PacketTypes.UPDATE:
                    {
                        ObjectInfo pi = NetHelpers.ConvertBytesToStruct<ObjectInfo>(buff, ref offset);
                        //PlayerUpdateEvent?.Invoke(pi);

                    }
                    break;
            }
        }
        private static void ProcessRegionInfo(PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {
                case PacketTypes.UPDATE:
                    {
                        
                    }break;
            }
        }
        private static void ProcessCellInfo(PacketTypes p, byte[] buff, ref int offset)
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
