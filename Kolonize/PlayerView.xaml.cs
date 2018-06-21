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
using System.Windows.Shapes;
using System.Windows.Threading;
using Universe;
using Universe.Objects;
namespace Kolonize
{
    /// <summary>
    /// Interaction logic for PlayerView.xaml
    /// </summary>
    public partial class PlayerView : Window
    {
        Player player;
        World worldref;
        Rectangle[,] DisplayGrid = new Rectangle[20,20];
        int TopX = 0;
        int TopY = 0;
        int ViewWidth = 20;
        int ViewHeight = 20;
        int PlayerViewPosX = 0;
        int PlayerViewPosY = 0;
        int PlayerWorldPosX = 0;
        int PlayerWorldPosY = 0;
        DispatcherTimer UpdateTimer;
        public PlayerView(World w)
        {
            worldref = w;
            InitializeComponent();
            player = new Player(500, 500, "Player1");
            player.SetDirection(1,-1);
            worldref.AddObject(player);

            for(int x=0; x < ViewWidth; ++x)
            {
                for(int y=0; y < ViewHeight; ++y)
                {
                    var r = new Rectangle()
                    {
                        Width = 20,
                        Height = 20,
                        Fill = Brushes.Black,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    DisplayGrid[x,y] = r;
                    Canvas.SetTop(r, y * 20);
                    Canvas.SetLeft(r, x * 20);
                    drawCanvas.Children.Add(r);
                }
            }
            var coord = player.GetPosition();
            PlayerWorldPosX = coord.x;
            PlayerWorldPosY = coord.y;
            TopX = PlayerWorldPosX - 10;
            TopY = PlayerWorldPosY - 10;
            foreach (var cell in worldref.GetRegionCells(TopX, PlayerWorldPosX + 10, TopY, PlayerWorldPosY + 10))
            {
                DisplayGrid[cell.X - TopX, cell.Y - TopY].Fill = CellToBrush(cell.WorldCellType);        
            }
            PlayerViewPosX = PlayerWorldPosX - TopX;
            PlayerViewPosY = PlayerWorldPosY - TopY;
            DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill = Brushes.CadetBlue;

            UpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            UpdateTimer.Tick += Update;
            UpdateTimer.Start();
        }

        public void Update(object sender, EventArgs e)
        {
            DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill = CellToBrush(worldref.GetCell(PlayerWorldPosX, PlayerWorldPosY).WorldCellType);
            var coord = player.GetPosition();
            PlayerWorldPosX = coord.x;
            PlayerWorldPosY = coord.y;
            PlayerViewPosX = PlayerWorldPosX - TopX;
            PlayerViewPosY = PlayerWorldPosY - TopY;
            if(PlayerViewPosX >= ViewWidth-3 || PlayerViewPosY >= ViewHeight-3 || PlayerViewPosX<3 || PlayerViewPosY<3)
            {
                TopX = PlayerWorldPosX - 10;
                TopY = PlayerWorldPosY - 10;
                foreach (var cell in worldref.GetRegionCells(TopX, PlayerWorldPosX + 10, TopY, PlayerWorldPosY + 10))
                {
                    DisplayGrid[cell.X - TopX, cell.Y - TopY].Fill = CellToBrush(cell.WorldCellType);
                }
                PlayerViewPosX = PlayerWorldPosX - TopX;
                PlayerViewPosY = PlayerWorldPosY - TopY;
            }
            DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill = Brushes.CadetBlue;
        }

        public Brush CellToBrush(CellType t)
        {
            switch (t)
            {

                case CellType.WATER: return Brushes.Blue;
                case CellType.SAND: return Brushes.Yellow;
                case CellType.DIRT: return Brushes.ForestGreen;
                case CellType.ICE: return Brushes.LightCyan;
                case CellType.ROCK: return Brushes.Gray;
                case CellType.LAVA: return Brushes.Red;
                case CellType.SPACE:
                default:
                    return Brushes.Black;
            }
        }

    }
}
