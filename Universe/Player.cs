using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe
{
    public class Player : Moveable
    {
        public Player(int x, int y, string id, int size = 1) : base(x, y, id, size) { }

        public void SetDirection(int dir)
        {
            switch(dir)
            {
                case -1: SetVelocity(0, 0); break;
                case 0: SetVelocity(0, -.1f); break;
                case 1: SetVelocity(.1f, 0); break;
                case 2: SetVelocity(0, .1f); break;
                case 3: SetVelocity(-.1f, 0); break;
            }
        }
    }
}
