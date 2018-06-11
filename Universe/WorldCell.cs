using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe
{
    public enum CellType { SPACE, WATER, SAND, DIRT, ROCK, LAVA, ICE }
    public class WorldCell
    {
        static Random r = new Random();
        public CellType WorldCellType = CellType.SPACE;
        public readonly int X;
        public readonly int Y;
        public WorldCell(CellType type, int x, int y)
        {
            WorldCellType = type;
            X = x;
            Y = y;
        }


        private static Array types = Enum.GetValues(typeof(CellType));
        private static int TypeCount = types.Length;
        public static WorldCell RandomCell(int x, int y)
        {
            int val = r.Next() % TypeCount;
            return new WorldCell((CellType)val,x,y);
        }

        public static WorldCell WeightedRandomCell(int x, int y)
        {
            double val = r.NextDouble();
            return new WorldCell(GetCellTypeByWeight(val), x, y);
        }

        public static CellType GetCellTypeByWeight(double weight)
        {
            if (weight > 0.8)
            {
                return CellType.ICE;
            }
            //else if (weight > 0.85)
            //{
            //    return CellType.LAVA;
            //}
            else if (weight > 0.7)
            {
                return CellType.ROCK;
            }
            else if (weight > 0.5)
            {
                return CellType.DIRT;
            }
            else if (weight > 0.47)
            {
                return CellType.SAND;
            }
            else
            {
                return CellType.WATER;
            }
        }

        public static double ComplexRandomNumber(int x, int y)
        {
            double pA = (r.NextDouble() * 200 + 500);
            double pB = (r.NextDouble() * 100 + 200);
            double pC = (r.NextDouble() * 50 + 100);
            double pD = (r.NextDouble() * 20 + 50);
            double a = Math.Abs((Math.Cos(x / pA + pC) + Math.Sin(y / pA + pD)));
            double b = Math.Abs((Math.Cos(x / pB + pD) + Math.Cos(y / pB + pA)));
            double c = Math.Abs((Math.Cos(x / pC) + Math.Cos(y / pC)));
            double d = Math.Abs((Math.Cos(x / pD) + Math.Cos(y / pD)));
            return ((8 * a + 4 * b + 2 * c + d));
        }
    }
}
