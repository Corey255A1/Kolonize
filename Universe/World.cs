using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
namespace Universe
{
    public delegate void MoverUpdate(Moveable m);
    public class World
    {
        int WORLD_SIZE;
        WorldCell[,] Terrain;

        Dictionary<string, Moveable> MoverMap = new Dictionary<string, Moveable>();
        Dictionary<string, string> PlayerKeyMap = new Dictionary<string, string>();
        List<Moveable> Movers = new List<Moveable>();
        Queue<Moveable> MoverAdd = new Queue<Moveable>();
        Thread MoverThread;

        public MoverUpdate MoverUpdateEvent;

        static bool MoverThreadActive = true;


        public World(int size)
        {
            WORLD_SIZE = size;
            Terrain = new WorldCell[WORLD_SIZE, WORLD_SIZE];
            double[,] rawTerrain = new double[WORLD_SIZE, WORLD_SIZE];
            for (int x=0;x<WORLD_SIZE; ++x)
            {
                for (int y = 0; y < WORLD_SIZE; ++y)
                {
                    rawTerrain[x, y] = WorldCell.ComplexRandomNumber(x,y);
                }
            }

            rawTerrain = ApplyFilter(rawTerrain, 25);
            var maxmin = FindBounds(rawTerrain);
            Normalize(ref rawTerrain, maxmin.Item1, 0.3);


            for (int x = 0; x < WORLD_SIZE; ++x)
            {
                for (int y = 0; y < WORLD_SIZE; ++y)
                {
                    Terrain[x, y] = new WorldCell(WorldCell.GetCellTypeByWeight(rawTerrain[x, y]), x, y);
                }
            }


            MoverThread = new Thread(MoverUpdates);
            MoverThread.Start();

        }

        #region Updates
        private void MoverUpdateEventCB(Moveable m)
        {
            MoverUpdateEvent?.Invoke(m);
        }
        private void MoverUpdates()
        {
            Stopwatch sw = new Stopwatch();
            int c;
            int i;
            int t;
            while (MoverThreadActive)
            {
                if(MoverAdd.Count>0)
                {
                    Movers.AddRange(MoverAdd);
                    MoverAdd.Clear();
                }
                c = Movers.Count();
                sw.Restart();
                for (i = 0; i < c; i++)
                {
                    Movers[i].Move(Terrain);
                }
                sw.Stop();
                t = 30 - (int)sw.ElapsedMilliseconds;
                Thread.Sleep(t>0?t:0);
            }
        }
        #endregion
        
        #region Build the World
        private Tuple<double,double> FindBounds(double[,] terrain)
        {
            double max = terrain[0, 0];
            double min = terrain[0, 0];
            for (int x = 0; x < WORLD_SIZE; ++x)
            {
                for (int y = 0; y < WORLD_SIZE; ++y)
                {
                    if (terrain[x, y] > max) max = terrain[x, y];
                    else if (terrain[x, y] < min) min = terrain[x, y];
                }
            }
            return new Tuple<double, double>(max, min);
        }
        private void Normalize(ref double[,] terrain, double max, double offset)
        {
            for (int x = 0; x < WORLD_SIZE; ++x)
            {
                for (int y = 0; y < WORLD_SIZE; ++y)
                {
                    terrain[x, y] = terrain[x, y] / max - offset;
                }
            }
        }
        private Tuple<double,double[,]> BuildFilter(int size)
        {
            double step = 0.5;
            double total = 0;
            int halfSize = size / 2;
            double[,] filt = new double[size, size];
            int fx = 0;
            for (int x =0; x< halfSize; ++x)
            {
                int fy = 0;
                for(int y =0;y< halfSize; ++y)
                {
                    total += ((x + 1) * step * (y + 1));
                    filt[fx, fy] = ((x + 1) * step * (y + 1));
                    ++fy;
                }
                for (int y = halfSize; y >=0 ; --y)
                {
                    total += ((x + 1) * step * (y + 1));
                    filt[fx, fy] = ((x + 1) * step * (y + 1));
                    ++fy;
                }
                ++fx;
            }
            for (int x = halfSize; x >=0; --x)
            {
                int fy = 0;
                for (int y = 0; y < halfSize; ++y)
                {
                    total += ((x + 1) * step * (y + 1));
                    filt[fx, fy] = ((x + 1) * step * (y + 1));
                    ++fy;
                }
                for (int y = halfSize; y >= 0; --y)
                {
                    total += ((x + 1) * step * (y + 1));
                    filt[fx, fy] = ((x + 1) * step * (y + 1));
                    ++fy;
                }
                ++fx;
            }
            return new Tuple<double,double[,]>(total, filt);

        }
        private double[,] ApplyFilter(double[,] terrain, int size)
        {
            
            int halfSize = size / 2;
            int area = size * size;
            var filtParams = BuildFilter(size);
            var filt = filtParams.Item2;
            var norm = filtParams.Item1;
            double[,] newterrain = new double[WORLD_SIZE,WORLD_SIZE];
            for (int x = 0; x < WORLD_SIZE; ++x)
            {
                for (int y = 0; y < WORLD_SIZE; ++y)
                {
                    double s = 0;                    
                    int fy = 0;
                    for (int a = y - halfSize; a < y + halfSize+1; ++a)
                    {
                        int fx = 0;
                        for (int b = x - halfSize; b < x + halfSize+1; ++b)
                        {
                            if (a > 0 && a < WORLD_SIZE && b > 0 && b < WORLD_SIZE)
                                s = terrain[b, a] * filt[fx, fy] + s;
                            else
                                s = 0;
                            ++fx;
                        }
                        ++fy;
                    }
                    newterrain[x, y] = s;
                }
            }
            return newterrain;
        }
        #endregion

        public IEnumerable<WorldCell> GetCells()
        {
            for (int x = 0; x < WORLD_SIZE; ++x)
            {
                for (int y = 0; y < WORLD_SIZE; ++y)
                {
                    yield return Terrain[x, y];
                }
            }
        }
        public IEnumerable<WorldCell> GetRegionCells(int x1, int x2, int y1, int y2)
        {
            //Bound Check
            if (x1 < 0) x1 = 0;
            else if (x1 > WORLD_SIZE) x1 = WORLD_SIZE - 2;
            if (x2 < 0) x2 = 1;
            else if (x2 > WORLD_SIZE) x2 = WORLD_SIZE - 1;
            if (y1 < 0) y1 = 0;
            else if (y1 > WORLD_SIZE) y1 = WORLD_SIZE - 2;
            if (y2 < 0) y2 = 1;
            else if (x2 > WORLD_SIZE) y2 = WORLD_SIZE - 1;

            if (x1 < x2 && y1 < y2)
            {
                int width = x2 - x1;
                int height = y2 - y1;
                var region = new WorldCell[width, height];
                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        yield return Terrain[x + x1, y + y1];
                    }
                }
            }
            yield break;
        }
        public WorldCell GetCell(int x, int y)
        {
            return Terrain[x, y];
        }
        public WorldCell[,] GetRegion(int x1, int x2, int y1, int y2)
        {
            if (x1 < 0) x1 = 0;
            else if (x1 > WORLD_SIZE) x1 = WORLD_SIZE - 2;
            if (x2 < 0) x2 = 1;
            else if (x2 > WORLD_SIZE) x2 = WORLD_SIZE - 1;
            if (y1 < 0) y1 = 0;
            else if (y1 > WORLD_SIZE) y1 = WORLD_SIZE - 2;
            if (y2 < 0) y2 = 1;
            else if (x2 > WORLD_SIZE) y2 = WORLD_SIZE - 1;
            if (x1<x2 && y1<y2)
            {
                int width = x2 - x1;
                int height = y2 - y1;
                var region = new WorldCell[width, height];
                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        region[x, y] = Terrain[x + x1, y + y1];
                    }
                }
                return region;
            }
            return null;
        }
        
        public Player GetPlayer(string name, string key)
        {
            if(MoverMap.ContainsKey(name))
            {
                if (PlayerKeyMap.ContainsKey(name) && PlayerKeyMap[name] == key)
                {

                    WorldObject p = MoverMap[name];
                    if (p.GetType() == typeof(Player))
                    {
                        return p as Player;
                    }
                }
            }
            else
            {
                PlayerKeyMap.Add(name, key);
                return CreateNewPlayer(name);
            }
            return null;
        }

        public Player CreateNewPlayer(string name)
        {
            int x = 0;
            int y = 0;
            foreach(var cell in GetCells())
            {
                if(cell.WorldCellType == CellType.DIRT || cell.WorldCellType == CellType.SAND)
                {
                    x = cell.X;
                    y = cell.Y;
                    break;
                }
            }
            var player = new Player(x, y, name);
            MoverMap.Add(name, player);
            AddObject(player);
            return player;
        }

        public void AddObject(WorldObject wo)
        {
            if(wo.GetType() == typeof(Moveable) || wo.GetType() == typeof(Player))
            {
                ((Moveable)wo).PositionUpdated += MoverUpdateEventCB;
                MoverAdd.Enqueue((Moveable)wo);

            }
        }
    }
}
