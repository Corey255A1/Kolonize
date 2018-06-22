using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe.Objects
{
    public class Player : Moveable
    {
        private int StepsLeft = -1;
        public Player(int x, int y, string id, int size = 1) : base(x, y, id, size)
        {
            ObjectType = WorldObjectTypes.PLAYER;
            MAX_SPEED = 0.2f;
        }      
        
        public void SetDirection(int dir, int paces)
        {
            SetDirection((Direction)dir, paces > 0 ? MAX_SPEED : 0);
            StepsLeft = paces;
        }
        public override void Move(WorldCell[,] map, WorldObject[,] objects)
        {
            var pos = GetPosition();
            base.Move(map, objects);
            if (StepsLeft > 0)
            {
                if (Vx == 0 && Vy == 0)
                {
                    StepsLeft = 0;
                    return;
                }
                if (pos.x != (int)X || pos.y != (int)Y)
                {
                    if (--StepsLeft == 0)
                    {
                        SetVelocity(0, 0);
                    }
                }
            }

            //Else we are a negative number and that means keep moving
        }
        public void PerformAction(uint action)
        {
            switch(action)
            {
                case 0:
                    CreateMaker();
                    break;
            }
        }
        private void CreateMaker()
        {
            int x = 0, y = 0;
            switch (Heading)
            {
                case 0: y = (int)Y - 1; x = (int)X; break;
                case 1: x = (int)X + 1; y = (int)Y; break;
                case 2: y = (int)Y + 1; x = (int)X; break;
                case 3: x = (int)X - 1; y = (int)Y; break;
            }
            if (x >= 0 && x < WorldInterface.WORLD_SIZE && y >= 0 && y < WorldInterface.WORLD_SIZE)
            {
                if (WorldInterface.IsObjectNull(x, y))
                {
                    KolonyMarker k = new KolonyMarker(x, y, "");
                    k.SetOwner(Id);
                    WorldInterface.AddObject(k);
                }
            }
        }
    }
}
