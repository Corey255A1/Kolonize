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
namespace KolonizeServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Server theServer;
        public int WorldSize = 1000;
        //For Testing and Visualization Purposes
        public MainWindow()
        {
            InitializeComponent();
            WorldInterface.InitializeWorld(WorldSize);

            var pf = PixelFormats.Bgr24;
            int rawStride = (WorldSize * pf.BitsPerPixel + 7) / 8;

            byte[] img = new byte[rawStride * WorldSize];
            int p = 0;
            foreach (var wc in WorldInterface.GetCells())
            {
                var c = CellToBrush(wc.WorldCellType);
                img[p] = c.B;
                img[p + 1] = c.G;
                img[p + 2] = c.R;
                p += 3;
            }
            BitmapSource b = BitmapSource.Create(WorldSize, WorldSize, 96, 96, pf, null, img, rawStride);
            Image disp = new Image()
            {
                Width = 500,
                Source = b
            };
            drawCanvas.Children.Add(disp);
            theServer = new Server();;
            theServer.ClientStatusUpdate += ClientStatus;
        }
        private void ClientStatus(ClientHandler ch, string msg)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if(msg == "Disconnected")
                {
                    clientsBox.Items.Remove(ch);
                }
                else if(msg == "Connected")
                {
                    clientsBox.Items.Add(ch);
                }

            }));
        }

        public Color CellToBrush(CellType t)
        {
            switch (t)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            theServer.Close();
        }
    }
}
