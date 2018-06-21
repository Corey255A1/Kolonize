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
            try
            {
                switch (d)
                {
                    //Package the Region Data and Send it
                    case DataTypes.CELL_INFO:
                        {
                            CellInfo ci = NetHelpers.ConvertBytesToStruct<CellInfo>(buff, ref offset);
                            ProcessCellInfo(s, p, ci);
                            return true;
                        }
                    case DataTypes.REGION_INFO:
                        {
                            RegionInfo ri = NetHelpers.ConvertBytesToStruct<RegionInfo>(buff, ref offset);
                            ProcessRegionInfo(s, p, ri);
                            return true;
                        }
                    case DataTypes.PLAYER_INFO:
                        {
                            PlayerInfo pi = NetHelpers.ConvertBytesToStruct<PlayerInfo>(buff, ref offset);
                            ProcessPlayerInfo(s, p, pi);
                            return true;
                        }
                    case DataTypes.OBJECT_INFO:
                        {
                            ObjectInfo pi = NetHelpers.ConvertBytesToStruct<ObjectInfo>(buff, ref offset);
                            ProcessObjectInfo(s, p, pi);
                            return true;
                        }
                    default:
                        return false;

                }
            }
            catch
            {
                //Convert Bytes throws an exception if there isn't enough bytes to convert
                return false;
            }

        }
        private static void ProcessPlayerInfo(StreamWriter s, PacketTypes p, PlayerInfo pi)
        {
            switch (p)
            {               
                case PacketTypes.REQUESTED:
                case PacketTypes.UPDATE:
                    {                        
                        PlayerUpdateEvent?.Invoke(pi);
                    }
                    break;
            }
        }
        private static void ProcessObjectInfo(StreamWriter s, PacketTypes p, ObjectInfo pi)
        {
            switch (p)
            {
                case PacketTypes.REQUESTED:
                case PacketTypes.UPDATE:
                    {                                                
                        ObjectUpdateEvent?.Invoke(pi);
                    }
                    break;
            }
        }
        private static void ProcessRegionInfo(StreamWriter s, PacketTypes p, RegionInfo ri)
        {
            switch (p)
            {
                case PacketTypes.UPDATE:
                    {
                        
                    }break;
            }
        }
        private static void ProcessCellInfo(StreamWriter s, PacketTypes p, CellInfo o)
        {
            switch (p)
            {
                case PacketTypes.REQUESTED:
                    {
                        
                        CellInfoUpdateEvent?.Invoke(o);
                    }
                    break;
            }
        }

    }
}
