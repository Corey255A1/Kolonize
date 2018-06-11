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
using System.Windows.Threading;
using KolonizeNet;
namespace KolonizeClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Rectangle[,] DisplayGrid = new Rectangle[20, 20];
        int TopX = 0;
        int TopY = 0;
        int ViewWidth = 20;
        int ViewHeight = 20;
        int PlayerViewPosX = 0;
        int PlayerViewPosY = 0;
        int PlayerWorldPosX = 0;
        int PlayerWorldPosY = 0;
        //DispatcherTimer UpdateTimer;
        Client theClient;
        Brush PlayerCellColor;
        public MainWindow()
        {
            InitializeComponent();

            for (int x = 0; x < ViewWidth; ++x)
            {
                for (int y = 0; y < ViewHeight; ++y)
                {
                    var r = new Rectangle()
                    {
                        Width = 20,
                        Height = 20,
                        Fill = Brushes.Black,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    DisplayGrid[x, y] = r;
                    Canvas.SetTop(r, y * 20);
                    Canvas.SetLeft(r, x * 20);
                    drawCanvas.Children.Add(r);
                }
            }
        }

        public void Update(PlayerInfo p)
        {
            Dispatcher.BeginInvoke(new Action(()=> {
                if (p.id == theClient.PlayerName)
                {
                    DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill = PlayerCellColor;
                    PlayerWorldPosX = p.x;
                    PlayerWorldPosY = p.y;
                    PlayerViewPosX = PlayerWorldPosX - TopX;
                    PlayerViewPosY = PlayerWorldPosY - TopY;
                    if (PlayerViewPosX >= ViewWidth - 3 || PlayerViewPosY >= ViewHeight - 3 || PlayerViewPosX < 3 || PlayerViewPosY < 3)
                    {
                        TopX = PlayerWorldPosX - 10;
                        TopY = PlayerWorldPosY - 10;
                        foreach (var cell in theClient.GetRegionCells(TopX, TopX + 20, TopY, TopY + 20))
                        {
                            DisplayGrid[cell.x - TopX, cell.y - TopY].Fill = CellToBrush(cell.cellType);
                        }
                        PlayerViewPosX = PlayerWorldPosX - TopX;
                        PlayerViewPosY = PlayerWorldPosY - TopY;
                        theClient.RestartAsyncUpdate();
                    }
                    PlayerCellColor = DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill;
                    DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill = Brushes.CadetBlue;
                }
            }));
        }

        public Brush CellToBrush(byte t)
        {
            switch (t)
            {

                case WorldConstants.WATER: return Brushes.Blue;
                case WorldConstants.SAND: return Brushes.Yellow;
                case WorldConstants.DIRT: return Brushes.ForestGreen;
                case WorldConstants.ICE: return Brushes.LightCyan;
                case WorldConstants.ROCK: return Brushes.Gray;
                case WorldConstants.LAVA: return Brushes.Red;
                case WorldConstants.SPACE:
                default:
                    return Brushes.Black;
            }
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            theClient = new Client(userNameBox.Text);
            var p = theClient.GetPlayer();
            PlayerWorldPosX = p.x;
            PlayerWorldPosY = p.y;
            TopX = p.x - 10;
            TopY = p.y - 10;
            foreach (var cell in theClient.GetRegionCells(TopX, TopX+20, TopY, TopY+20))
            {
                DisplayGrid[cell.x - TopX, cell.y - TopY].Fill = CellToBrush(cell.cellType);
            }
            PlayerViewPosX = PlayerWorldPosX - TopX;
            PlayerViewPosY = PlayerWorldPosY - TopY;
            PlayerCellColor = DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill;
            DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill = Brushes.CadetBlue;

            theClient.StartAsyncUpdate(Update);
            //theClient.SetPlayerVelocity(0.05f, 0);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(theClient != null)
            {
                switch(e.Key)
                {
                    case Key.W:
                        theClient.SetPlayerVelocity(0, -0.5f); break;
                    case Key.S:
                        theClient.SetPlayerVelocity(0, 0.5f); break;
                    case Key.A:
                        theClient.SetPlayerVelocity(-0.5f,0); break;
                    case Key.D:
                        theClient.SetPlayerVelocity(0.5f, 0); break;
                }
            }
        }
    }
}
