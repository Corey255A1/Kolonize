using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe
{

    public class WorldObject
    {
        protected double X = 0;
        protected double Y = 0;
        protected int Size = 1;
        public string Id = "";
        public WorldObject(int x, int y, string id, int size=1)
        {
            X = x; Y = y; Size = size; Id = id;
        }
        public Coord GetPosition()
        {
            return new Coord() { x = (int)X, y = (int)Y };
        }
    }
    public struct Coord
    {
        public int x;
        public int y;
    }
    public struct Vector
    {
        public float x;
        public float y;
    }
    public class Moveable : WorldObject
    {
        public MoverUpdate PositionUpdated;
        protected float Vx = 0;
        protected float Vy = 0;
        protected int NormVx = 0;
        protected int NormVy = 0;
        protected bool CanSwim = false;
        protected bool CanWalk = true;
        protected bool CanClimb = false;

        public Moveable(int x, int y, string  id, int size = 1) : base(x, y, id, size) { }

        private int futureX=0;
        private int futureY=0;
        public void SetVelocity(float vx, float vy)
        {
            Vx = vx;
            Vy = vy;
            NormVx = Vx > 0 ? 1 : Vx < 0 ? -1 : 0;
            NormVy = Vy > 0 ? 1 : Vy < 0 ? -1 : 0;
        }
        public Vector GetVelocity()
        {
            return new Vector() { x = Vx, y = Vy };
        }
        public virtual void Move(WorldCell[,] map, WorldObject[,] objects)
        {
            //if we have no velocity, don't move
            if (Vx == 0 && Vy == 0) return;

            futureX = (int)(X + Vx);
            futureY = (int)(Y + Vy);
            //Bound Check;
            if(futureX<0 || futureX>map.Length-1 || futureY<0 || futureY>map.Length-1)
            {
                SetVelocity(0, 0);
                return;
            }

            WorldCell next = map[futureX, futureY];            
            bool apply = false;
            switch(next.WorldCellType)
            {
                case CellType.WATER:
                    if (CanSwim) apply = true; break;
                case CellType.SAND:
                case CellType.DIRT:
                    if (CanWalk) apply = true; break;
                case CellType.ROCK:
                    if (CanClimb) apply = true; break;
            }
            if(apply)
            {
                bool invoke = false;
                if ((int)X != futureX || (int)Y != futureY)
                {
                    //We moved a cell
                    WorldObject wobj = objects[futureX, futureY];
                    //Only One Object per Cell!
                    if (wobj != null)
                    {
                        apply = false;
                        SetVelocity(0, 0);
                        return;
                    }
                    //tell everyone
                    invoke = true;
                    //Clear our Current Cell
                    objects[(int)X, (int)Y] = null;
                    //Move to our new cell
                    objects[futureX, futureY] = this;
                }
                //update our pos
                X = (X + Vx);
                Y = (Y + Vy);
                //then tell everyone
                if(invoke) PositionUpdated?.Invoke(this);
            }
            else
            {
                SetVelocity(0, 0);
            }
           
        }
    }
}
