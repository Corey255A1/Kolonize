using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KolonizeNet;
using Universe;
namespace KolonizeServer
{
    public static class PacketProcessors
    {
        public static IEnumerable<byte[]> ProcessPacket(PacketTypes p, DataTypes d, byte[] buff)
        {
            switch (d)
            {
                //Package the Region Data and Send it
                case DataTypes.REGION_INFO: return ProcessRegionInfo(p, buff);
                case DataTypes.PLAYER_INFO: return ProcessPlayerInfo(p, buff);
                case DataTypes.PLAYER_CONTROL: return ProcessPlayerControl(p, buff);
                default: return null;

            }

        }


        private static IEnumerable<byte[]> ProcessPlayerControl(PacketTypes p, byte[] buff)
        {
            int offset = 0;
            switch (p)
            {
                    //Using Player Control Packet now.
                    case PacketTypes.SET:
                        {
                        PlayerControl pi = NetHelpers.ConvertBytesToStruct<PlayerControl>(buff, ref offset);
                        Player playa = WorldInterface.GetPlayer(pi.id, pi.key);
                        if (playa != null)
                        {
                            playa.SetDirection(pi.direction, pi.paces);
                        }
                        yield break;
                        }
            }
            yield break;
        }

        private static IEnumerable<byte[]> ProcessPlayerInfo(PacketTypes p, byte[] buff)
        {
            int offset = 0;
            switch (p)
            {
                case PacketTypes.REQUEST:
                    {
                        PlayerInfo pi = NetHelpers.ConvertBytesToStruct<PlayerInfo>(buff, ref offset);
                        Player playa = WorldInterface.GetPlayer(pi.id, pi.key);
                        if (playa != null)
                        {
                            var coord = playa.GetPosition();
                            var vel = playa.GetVelocity();
                            pi = new PlayerInfo(PacketTypes.REQUESTED)
                            {
                                id = playa.Id,
                                x = coord.x,
                                y = coord.y,
                                vx = vel.x,
                                vy = vel.y
                            };
                            yield return NetHelpers.ConvertStructToBytes(pi);
                        }

                    }
                    break;
                //Using Player Control Packet now.
                //case PacketTypes.SET:
                //    {
                //        PlayerInfo pi = NetHelpers.ConvertBytesToStruct<PlayerInfo>(buff, ref offset);
                //        Player playa = WorldInterface.GetPlayer(pi.id);
                //        playa.SetVelocity(pi.vx, pi.vy);                      
                //        yield break;
                //    }
            }
            yield break;
        }

        private static IEnumerable<byte[]> ProcessRegionInfo(PacketTypes p, byte[] buff)
        {
            int offset = 0;
            switch (p)
            {
                case PacketTypes.REQUEST:
                    {
                        RegionInfo ri = NetHelpers.ConvertBytesToStruct<RegionInfo>(buff, ref offset);
                        CellInfo c = new CellInfo(PacketTypes.REQUESTED);
                        List<WorldCell> region = new List<WorldCell>(WorldInterface.GetRegionCells(ri.x1, ri.x2, ri.y1, ri.y2));
                        int count = region.Count;
                        foreach (WorldCell wc in region)
                        {
                            c.cellType = (byte)wc.WorldCellType;
                            c.remainingCells = --count;
                            c.x = wc.X;
                            c.y = wc.Y;
                            
                            yield return NetHelpers.ConvertStructToBytes(c);
                        }
                    }break;
            }
            yield break;
        }

    }
}
