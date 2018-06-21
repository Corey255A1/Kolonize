using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universe;
using Universe.Objects;
using KolonizeNet;
namespace KolonizeServer
{
    public class WorldNetIF
    {
        Server Net;
        public WorldNetIF(Server s)
        {
            Net = s;

            WorldInterface.AddMoverUpdateCB(MoverUpdated);
            WorldInterface.AddObjectCreatedCB(ObjectAdded);

        }

        public void PlayerUpdated(Player p)
        {
            //Determine what information needs to be sent
            //Back to the Client
            var coord = p.GetPosition();
            var v = p.GetVelocity();
            var pi = new PlayerInfo(PacketTypes.UPDATE)
            {
                id = p.Id,
                x = coord.x,
                y = coord.y,
                vx = v.x,
                vy = v.y
            };
            Net.SendToPlayer(p.Id, NetHelpers.ConvertStructToBytes(pi));
        }

        public void MoverUpdated(Moveable m)
        {
            //At Some point restrict to only updates around players
            var coord = m.GetPosition();
            var v = m.GetVelocity();
            var pi = new ObjectInfo(PacketTypes.UPDATE)
            {
                id = m.Id,
                objecttype = (int)m.ObjectType,
                x = coord.x,
                y = coord.y,
                vx = v.x,
                vy = v.y
            };
            Net.BroadcastToClients(NetHelpers.ConvertStructToBytes(pi));

        }
        public void ObjectAdded(WorldObject o)
        {
            //At Some point restrict to only updates around players
            var coord = o.GetPosition();
            var pi = new ObjectInfo(PacketTypes.UPDATE)
            {
                id = o.Id,
                objecttype = (int)o.ObjectType,
                x = coord.x,
                y = coord.y,
            };
            Net.BroadcastToClients(NetHelpers.ConvertStructToBytes(pi));
        }
    }
}
