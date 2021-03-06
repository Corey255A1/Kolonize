﻿//Corey Wunderlich 2018
//"Kolonize" Server Packet Processors

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KolonizeNet;
using Universe;
using Universe.Objects;
namespace KolonizeServer
{
    public class PlayerProcessor
    {
        public string PlayerID = "";

        public bool ProcessPacket(StreamWriter s, PacketTypes p, DataTypes d, byte[] buff, ref int offset)
        {
            try
            {
                switch (d)
                {
                    //Package the Region Data and Send it
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
                            ObjectInfo oi = NetHelpers.ConvertBytesToStruct<ObjectInfo>(buff, ref offset);
                            ProcessObjectInfo(s, p, oi);
                            return true;
                        }
                    case DataTypes.PLAYER_CONTROL:
                        {
                            PlayerControl oi = NetHelpers.ConvertBytesToStruct<PlayerControl>(buff, ref offset);
                            ProcessPlayerControl(s, p, oi);
                            return true;
                        }
                    case DataTypes.PLAYER_PERFORM_ACTION:
                        {
                            PlayerPerformAction oi = NetHelpers.ConvertBytesToStruct<PlayerPerformAction>(buff, ref offset);
                            ProcessPlayerAction(s, p, oi);
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

        private bool ProcessPlayerControl(StreamWriter s, PacketTypes p, PlayerControl pi)
        {
            switch (p)
            {
                    case PacketTypes.SET:
                        {
                            Player playa = WorldInterface.GetPlayer(pi.id, pi.key);
                            if (playa != null)
                            {
                                playa.SetDirection(pi.direction, pi.paces);
                            }
                        return true;
                        }
            }
            return false;
        }
        private bool ProcessPlayerAction(StreamWriter s, PacketTypes p, PlayerPerformAction pi)
        {
            switch (p)
            {
                case PacketTypes.SET:
                    {
                        WorldInterface.PerformAction(pi.id, pi.key, pi.ActionID);
                        return true;
                    }
            }
            return false;
        }
        private bool ProcessPlayerInfo(StreamWriter s, PacketTypes p, PlayerInfo pi)
        {
            switch (p)
            {
                case PacketTypes.REQUEST:
                    {                        
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
                            foreach (var obj in WorldInterface.GetRegionObjects(coord.x - 10, coord.x + 10, coord.y - 10, coord.y + 10))
                            {
                                if (obj == null) continue;
                                var oc = obj.GetPosition();
                                var oi = new ObjectInfo(PacketTypes.REQUESTED)
                                {
                                    id = obj.Id,
                                    objecttype = (int)obj.ObjectType,
                                    x = oc.x,
                                    y = oc.y,
                                    vx = 0,
                                    vy = 0,
                                };
                                s(NetHelpers.ConvertStructToBytes(oi));

                            }
                        }
                        return true;
                    }
            }
            return false;
        }
        private bool ProcessObjectInfo(StreamWriter s, PacketTypes p, ObjectInfo oi)
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
        private bool ProcessRegionInfo(StreamWriter s, PacketTypes p, RegionInfo ri)
        {
            switch (p)
            {
                case PacketTypes.REQUEST:
                    {
                        //Get Cell Type Info
                        if ((ri.regionDataTypes & WorldConstants.REGION_INFO_CELL) != 0)
                        {
                            CellInfo c = new CellInfo(PacketTypes.REQUESTED);
                            List<WorldCell> region = new List<WorldCell>(WorldInterface.GetRegionCells(ri.x1, ri.x2, ri.y1, ri.y2));
                            int count = region.Count;
                            foreach (WorldCell wc in region)
                            {
                                c.cellType = (byte)wc.WorldCellType;
                                c.remainingCells = --count; // I don't even know if this is necessary anymore...
                                c.x = wc.X;
                                c.y = wc.Y;

                                s(NetHelpers.ConvertStructToBytes(c));
                            }
                        }
                        //Get Object Info
                        if ((ri.regionDataTypes & WorldConstants.REGION_INFO_OBJECT) != 0 || (ri.regionDataTypes & WorldConstants.REGION_INFO_PLAYER) !=0)
                        {
                            bool obj = (ri.regionDataTypes & WorldConstants.REGION_INFO_OBJECT) != 0;
                            bool ply = (ri.regionDataTypes & WorldConstants.REGION_INFO_PLAYER) != 0;
                            ObjectInfo o = new ObjectInfo(PacketTypes.REQUESTED);
                            foreach(var wobject in WorldInterface.GetRegionObjects(ri.x1, ri.x2, ri.y1, ri.y2))
                            {
                                if (wobject == null) continue;
                                var pos = wobject.GetPosition();
                                if(wobject.ObjectType == WorldObjectTypes.PLAYER && ply)
                                {
                                    var vel = ((Player)wobject).GetVelocity();
                                    o.id = wobject.Id;
                                    o.x = pos.x;
                                    o.y = pos.y;
                                    o.vx = vel.x;
                                    o.vy = vel.y;
                                    o.objecttype = WorldConstants.TYPE_PLAYER;
                                    s(NetHelpers.ConvertStructToBytes(o));
                                }
                                else if(obj)
                                {
                                    o.id = wobject.Id;
                                    o.x = pos.x;
                                    o.y = pos.y;
                                    if (wobject.ObjectType == WorldObjectTypes.MARKER)
                                    {
                                        o.objecttype = WorldConstants.TYPE_MARKER;
                                    }
                                    else if (wobject.ObjectType == WorldObjectTypes.MOVEABLE)
                                    {
                                        var vel = ((Moveable)wobject).GetVelocity();
                                        o.objecttype = WorldConstants.TYPE_MOVEABLE;
                                        o.vx = vel.x;
                                        o.vy = vel.y;
                                    }
                                    else
                                    {
                                        o.objecttype = WorldConstants.TYPE_GENERIC;
                                    }
                                    s(NetHelpers.ConvertStructToBytes(o));
                                }
                            }


                        }
                        return true;
                    }
            }
            return false;
        }

    }
}
