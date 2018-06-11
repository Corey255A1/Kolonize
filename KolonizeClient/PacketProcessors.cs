using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KolonizeNet;

namespace KolonizeClient
{
    public delegate void PlayerUpdate(PlayerInfo p);
    public static class PacketProcessors
    {
        public static PlayerUpdate PlayerUpdateEvent;
        public static void ProcessPacket(PacketTypes p, DataTypes d, byte[] buff)
        {
            switch (d)
            {
                //Package the Region Data and Send it
                case DataTypes.REGION_INFO: ProcessRegionInfo(p, buff); break;
                case DataTypes.PLAYER_INFO: ProcessPlayerInfo(p, buff); break;
                default: return;

            }

        }
        private static void ProcessPlayerInfo(PacketTypes p, byte[] buff)
        {
            int offset = 0;
            switch (p)
            {
                case PacketTypes.UPDATE:
                    {
                        PlayerInfo pi = NetHelpers.ConvertBytesToStruct<PlayerInfo>(buff, ref offset);
                        PlayerUpdateEvent?.Invoke(pi);

                    }
                    break;
            }
        }

        private static void ProcessRegionInfo(PacketTypes p, byte[] buff)
        {
            int offset = 0;
            switch (p)
            {
                case PacketTypes.UPDATE:
                    {
                        
                    }break;
            }
        }

    }
}
