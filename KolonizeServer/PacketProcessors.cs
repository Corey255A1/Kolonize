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
    public class PlayerProcessor
    {
        public string PlayerID = "";

        public bool ProcessPacket(StreamWriter s, PacketTypes p, DataTypes d, byte[] buff, ref int offset)
        {
            switch (d)
            {
                //Package the Region Data and Send it
                case DataTypes.REGION_INFO: return ProcessRegionInfo(s, p, buff, ref offset);
                case DataTypes.PLAYER_INFO: return ProcessPlayerInfo(s, p, buff, ref offset);
                case DataTypes.OBJECT_INFO: return ProcessObjectInfo(s, p, buff, ref offset);
                case DataTypes.PLAYER_CONTROL: return ProcessPlayerControl(s, p, buff, ref offset);
                default: return false;

            }

        }

        private bool ProcessPlayerControl(StreamWriter s, PacketTypes p, byte[] buff, ref int offset)
        {
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
                        }return true;
            }
            return false;
        }
        private bool ProcessPlayerInfo(StreamWriter s, PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {
                case PacketTypes.REQUEST:
                    {
                        PlayerInfo pi = NetHelpers.ConvertBytesToStruct<PlayerInfo>(buff, ref offset);
                        Player playa = WorldInterface.GetPlayer(pi.id, pi.key);
                        if (playa != null)
                        {
                            PlayerID = pi.id;
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
                            s(NetHelpers.ConvertStructToBytes(pi));
                            

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
                        return true;
                    }
            }
            return false;
        }
        private bool ProcessObjectInfo(StreamWriter s, PacketTypes p, byte[] buff, ref int offset)
        {
            switch (p)
            {
                case PacketTypes.REQUEST:
                    {
                    }
                    return true;
            }
            return false;
        }

        private bool ProcessRegionInfo(StreamWriter s, PacketTypes p, byte[] buff, ref int offset)
        {
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

                            s(NetHelpers.ConvertStructToBytes(c));
                        }
                        return true;
                    }
            }
            return false;
        }

    }
}
