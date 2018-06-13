//Corey Wunderlich 2018
//"Kolonize" Server Packet Processors

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
                case DataTypes.OBJECT_INFO: return ProcessObjectInfo(p, buff);
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

                            //Send over object positions in the region surrounding our player
                            //DON'T DO THIS YET... Still thinking about it, Client needs to support it 
                            //foreach (var obj in WorldInterface.GetRegionObjects(coord.x-10,coord.x+10,coord.y-10,coord.y+10))
                            //{

                            //    var oc = obj.GetPosition();
                            //    var oi = new ObjectInfo(PacketTypes.REQUESTED)
                            //    {
                            //        id = obj.Id,
                            //        x = oc.x,
                            //        y = oc.y,
                            //        vx = 0,
                            //        vy = 0,
                            //    };
                            //    yield return NetHelpers.ConvertStructToBytes(oi);

                            //}


                        }
                        

                    }
                    break;
            }
            yield break;
        }
        private static IEnumerable<byte[]> ProcessObjectInfo(PacketTypes p, byte[] buff)
        {
            int offset = 0;
            switch (p)
            {
                case PacketTypes.REQUEST:
                    {
                    }
                    break;
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
