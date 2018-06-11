using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Universe;
namespace Kolonize
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        World w;
        PlayerView pv;
        public int WorldSize = 1000;
        //For Testing and Visualization Purposes
        public MainWindow()
        {
            InitializeComponent();
            w = new World(WorldSize);
            int size = WorldSize;
            //int size = 100;
            var pf = PixelFormats.Bgr24;
            int rawStride = (size * pf.BitsPerPixel + 7) / 8;

            byte[] img = new byte[rawStride * size];
            int p = 0;
            //foreach (var wc in w.GetRegionCells(x1,x2,y1,y2)
            foreach (var wc in w.GetCells())
            {
                var c = CellToBrush(wc.WorldCellType);
                img[p] = c.B;
                img[p + 1] = c.G;
                img[p + 2] = c.R;
                p+=3;
            }
            BitmapSource b = BitmapSource.Create(size, size, 96, 96, pf, null, img, rawStride);
            Image disp = new Image()
            {
                Width = 500,
                Source = b
            };

            pv = new PlayerView(w);
            pv.Show();
            drawCanvas.Children.Add(disp);

        }
        public Color CellToBrush(CellType t)
        {
            switch(t)
            {
                
                case CellType.WATER: return Colors.Blue;
                case CellType.SAND: return Colors.Yellow;
                case CellType.DIRT: return Colors.ForestGreen;
                case CellType.ICE: return Colors.LightCyan;
                case CellType.ROCK: return Colors.Gray;
                case CellType.LAVA: return Colors.Red;
                case CellType.SPACE:
                default:
                    return Colors.Black;
            }
        }
    }
}
