using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe.Objects
{
    public class KolonyMarker: WorldObject
    {
        private static int KOLONY_MARKER_COUNT = 0;
        private string Owner;
        private float Radius = 10f;
        private int Health = 100;
        public KolonyMarker(int x, int y, string id, int size = 1) : base(x, y, id, size)
        {
            ObjectType = WorldObjectTypes.MARKER;
            Id = "MARKER:" + (++KOLONY_MARKER_COUNT);
        }
        public void SetOwner(string owner)
        {
            Owner = owner;
        }
    }
}
