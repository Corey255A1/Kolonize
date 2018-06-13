using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe
{
    public static class WorldInterface
    {
        public static World theWorld;

        public static void InitializeWorld(int size)
        {
            theWorld = new World(size);
        }
        public static IEnumerable<WorldCell> GetCells()
        {
            return theWorld.GetCells();
        }
        public static IEnumerable<WorldCell> GetRegionCells(int x1, int x2, int y1, int y2)
        {
            return theWorld.GetRegionCells(x1, x2, y1, y2);
        }
        public static IEnumerable<WorldObject> GetRegionObjects(int x1, int x2, int y1, int y2)
        {
            return theWorld.GetRegionObjects(x1, x2, y1, y2);
        }
        public static WorldCell GetCell(int x, int y)
        {
            return theWorld.GetCell(x,y);
        }
        public static WorldCell[,] GetRegion(int x1, int x2, int y1, int y2)
        {
            return theWorld.GetRegion(x1, x2, y1, y2);
        }

        public static void AddObject(WorldObject wo)
        {
            theWorld.AddObject(wo);
        }

        public static Player GetPlayer(string name,string key)
        {
            return theWorld.GetPlayer(name,key);
        }
        public static void AddMoverUpdateCB(MoverUpdate m)
        {
            theWorld.MoverUpdateEvent += m;
        }
    }
}
