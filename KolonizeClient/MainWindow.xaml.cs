﻿using System;
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
        //Keeping with the 20x20 view port for now, eventually get all world cells in a cache
        //Not have to to keep querying the the net for them... TODO
        Rectangle[,] DisplayGrid = new Rectangle[20, 20];
        byte[,] WorldCells = new byte[20, 20];

        int TopX = 0;
        int TopY = 0;
        int ViewWidth = 20;
        int ViewHeight = 20;
        int PlayerViewPosX = 0;
        int PlayerViewPosY = 0;
        int PlayerWorldPosX = 0;
        int PlayerWorldPosY = 0;
        Dictionary<string, ObjectInfo> OtherObjects = new Dictionary<string, ObjectInfo>();
        DispatcherTimer DrawTimer;
        Client theClient;
        bool UpdateLock = false;
        //Dictionary<string, DrawingBrush> ResourceMap = new Dictionary<string, DrawingBrush>();
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
                        StrokeThickness = 0
                    };
                    DisplayGrid[x, y] = r;
                    Canvas.SetTop(r, y * 20);
                    Canvas.SetLeft(r, x * 20);
                    drawCanvas.Children.Add(r);
                }
            }
            DrawTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            DrawTimer.Tick += Draw;
            DrawTimer.Start();

            //ResourceMap["Grass"] = (DrawingBrush)this.FindResource("Grass");
            //ResourceMap["Water"] = (DrawingBrush)this.FindResource("Water");
        }

        public void Draw(object sender, EventArgs e)
        {
            if (UpdateLock) return;
            for(int x=0;x<20;++x)
            {
                for(int y=0;y<20;++y)
                {
                    DisplayGrid[x, y].Fill = CellToBrush(WorldCells[x,y]);
                }
            }
            DisplayGrid[PlayerViewPosX, PlayerViewPosY].Fill = Brushes.CadetBlue;
            foreach (var p in OtherObjects.Values)
            {
                if (p.x > TopX && p.x < TopX + 20 && p.y > TopY && p.y < TopY + 20)
                {
                    //Make this like the CellToBrush
                    Brush brush;
                    if (p.objecttype == WorldConstants.TYPE_MARKER)
                        brush = Brushes.Chocolate;
                    else if (p.objecttype == WorldConstants.TYPE_PLAYER)
                        brush = Brushes.DarkRed;
                    else
                        brush = Brushes.DarkGoldenrod;
                    DisplayGrid[p.x - TopX, p.y - TopY].Fill = brush;
                }
            }
        }

        public void Update(ObjectInfo p)
        {            
            Dispatcher.BeginInvoke(new Action(()=> {
                UpdateLock = true;
                if (p.id == theClient.PlayerName)
                {
                    PlayerWorldPosX = p.x;
                    PlayerWorldPosY = p.y;
                    PlayerViewPosX = PlayerWorldPosX - TopX;
                    PlayerViewPosY = PlayerWorldPosY - TopY;
                    if (PlayerViewPosX >= ViewWidth - 3 || PlayerViewPosY >= ViewHeight - 3 || PlayerViewPosX < 3 || PlayerViewPosY < 3)
                    {
                        TopX = PlayerWorldPosX - 10;
                        TopY = PlayerWorldPosY - 10;
                        theClient.GetRegionCells(TopX, TopX + 20, TopY, TopY + 20);                        
                        PlayerViewPosX = PlayerWorldPosX - TopX;
                        PlayerViewPosY = PlayerWorldPosY - TopY;
                    }          
                  
                }
                else
                {
                    if(!OtherObjects.ContainsKey(p.id))
                    {
                        OtherObjects.Add(p.id, p);
                    }
                    else
                    {
                        OtherObjects[p.id] = p;
                    }
                }
                UpdateLock = false;
            }));
        }

        public Brush CellToBrush(byte t)
        {
            switch (t)
            {

                case WorldConstants.WATER: return Brushes.Blue; //return ResourceMap["Water"];
                case WorldConstants.SAND: return Brushes.Yellow;//return (DrawingBrush)this.FindResource("Sand");
                case WorldConstants.DIRT: return Brushes.ForestGreen;//return ResourceMap["Grass"];
                case WorldConstants.ICE: return Brushes.LightCyan;
                case WorldConstants.ROCK: return Brushes.Gray;
                case WorldConstants.LAVA: return Brushes.Red;
                case WorldConstants.SPACE:
                default:
                    return Brushes.Black;
            }
        }

        private void RxCellInfo(CellInfo cell)
        {
            if (cell.x - TopX < 20 && cell.y - TopY < 20 && cell.x - TopX>=0 && cell.y - TopY>=0)
            {
                WorldCells[cell.x - TopX, cell.y - TopY] = cell.cellType;
            }
        }

        private void MyPlayerInfo(PlayerInfo p)
        {
            PlayerWorldPosX = p.x;
            PlayerWorldPosY = p.y;
            TopX = p.x - 10;
            TopY = p.y - 10;
            //Eventually get either a large region or all of the world
            //Or maybe the server only sends the cells for places the player has been?
            theClient.GetRegionCells(TopX, TopX + 20, TopY, TopY + 20);

            PlayerViewPosX = PlayerWorldPosX - TopX;
            PlayerViewPosY = PlayerWorldPosY - TopY;          

        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            theClient = new Client(userNameBox.Text, passwordBox.Password, serverBox.Text);
            theClient.RegisterForCellInfo(RxCellInfo);            
            theClient.RegisterForPlayerUpdates(MyPlayerInfo);
            theClient.RegisterForObjectUpdates(Update);
            theClient.GetPlayerInfo();

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(theClient != null)
            {
                switch(e.Key)
                {
                    case Key.W:
                        theClient.SetPlayerDirection(0,1); break;
                    case Key.S:
                        theClient.SetPlayerDirection(2,1); break;
                    case Key.A:
                        theClient.SetPlayerDirection(3,1); break;
                    case Key.D:
                        theClient.SetPlayerDirection(1,1); break;
                    case Key.L:
                        theClient.CreateMarker(); break;
                }
            }
        }
    }
}
