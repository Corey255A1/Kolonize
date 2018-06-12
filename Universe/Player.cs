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
        private int StepsLeft = -1;
        public void SetDirection(int dir, int paces)
        {
            switch(dir)
            {
                case -1: SetVelocity(0, 0); break;
                case 0: SetVelocity(0, -.1f); break;
                case 1: SetVelocity(.1f, 0); break;
                case 2: SetVelocity(0, .1f); break;
                case 3: SetVelocity(-.1f, 0); break;
            }
            StepsLeft = paces;
        }
        public override void Move(WorldCell[,] map)
        {
            var pos = GetPosition();
            base.Move(map);
            if (pos.x != (int)X || pos.y != (int)Y)
            {
                //If we moved, deduct a step
                if (StepsLeft > 0)
                {
                    if (--StepsLeft == 0)
                    {
                        SetVelocity(0, 0);
                    }
                }
            }
            //Else we are a negative number and that means keep moving
        }
    }
}
