using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LightsOutUniversal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private LightsOutGame game;
        public MainPage()
        {            
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            game = new LightsOutGame();
            CreateGrid();
            DrawGrid();
        }

        private void CreateGrid()
        {
            // Remove all previously-existing rectangles
            boardCanvas.Children.Clear();

            SolidColorBrush white = new SolidColorBrush(Windows.UI.Colors.White);
            SolidColorBrush black = new SolidColorBrush(Windows.UI.Colors.Black);
            int rectSize = (int)boardCanvas.Width / game.GridSize;

            // Create rectangles for grid
            for (int r = 0; r < game.GridSize; r++)
            {
                for (int c = 0; c < game.GridSize; c++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Fill = white;
                    rect.Width = rectSize + 1;
                    rect.Height = rect.Width + 1;
                    rect.Stroke = black;

                    // Store each row and col as a Point
                    rect.Tag = new Point(r, c);

                    // Register event handler
                    rect.Tapped += Rect_TappedAsync;

                    // Put the rectangle at the proper location within the canvas
                    Canvas.SetTop(rect, r * rectSize);
                    Canvas.SetLeft(rect, c * rectSize);

                    // Add the new rectangle to the canvas' children
                    boardCanvas.Children.Add(rect);
                }
            }
        }

        private async void Rect_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            var move = (Point)rect.Tag;
            game.Move((int)move.X, (int)move.Y);
            DrawGrid();

            if (game.IsGameOver())
            {
                MessageDialog msgDialog = new MessageDialog("Congratulations! You've won!", "LightsOut!");

                // Add an OK button
                msgDialog.Commands.Add(new UICommand("OK"));

                // Show the message box and wait aynchrously for a button press
                IUICommand command = await msgDialog.ShowAsync();

                game.NewGame();
                DrawGrid();
            }
        }

        private void DrawGrid()
        {
            int index = 0;
            SolidColorBrush white = new SolidColorBrush(Windows.UI.Colors.White);
            SolidColorBrush black = new SolidColorBrush(Windows.UI.Colors.Black);

            // Set the colors of the rectangles
            for (int r = 0; r < game.GridSize; r++)
            {
                for (int c = 0; c < game.GridSize; c++)
                {
                    Rectangle rect = boardCanvas.Children[index] as Rectangle;
                    index++;
                    if (game.GetGridValue(r, c))
                    {
                        // On                      
                        rect.Fill = white;
                        rect.Stroke = black;
                    }
                    else
                    {
                        // Off
                        rect.Fill = black;
                        rect.Stroke = white;
                    }
                }
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            game.NewGame();
            DrawGrid();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            string json = JsonConvert.SerializeObject(game);
            ApplicationData.Current.LocalSettings.Values["grid"] = json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("grid"))
            {
                string json = ApplicationData.Current.LocalSettings.Values["grid"] as string;
                game = JsonConvert.DeserializeObject<LightsOutGame>(json);
                DrawGrid();
            }
        }
    }
}
