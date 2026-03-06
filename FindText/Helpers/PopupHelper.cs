using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace FindText.Helpers
{

    /// <summary>
    /// 依赖资源： BackgroundBrush ， HighlightBrush ,
    /// </summary>
    /// 
    internal class PopupHelper
    {

        static Popup? _pop;

        public static void ShowPopupMessage(FrameworkElement parent, string message, int delay = 3)
        {
            TextBlock msg = new TextBlock()
            {
                FontSize = 14,                
                Text = message,
                Margin = new Thickness(6, 3, 6, 3),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = App.Current.Resources["ForegroundBrush"] as SolidColorBrush,
                TextWrapping = TextWrapping.Wrap

            };
            ShowPopupMessage(parent, msg, delay);
        }

        public static void ShowPopupMessage(FrameworkElement parent, FrameworkElement content)
        {
            ShowPopupMessage(parent, content, 3);
        }

        public static void ShowPopupMessage(FrameworkElement parent, FrameworkElement content, int delay)
        {
            if (_pop != null)
                return;

            _pop = new Popup();
            _pop.AllowsTransparency = true;
            _pop.PlacementTarget = parent;
            Border bd = new Border()
            {
                Margin = new Thickness(4),
                BorderBrush = App.Current.Resources["HighlightBrush2"] as SolidColorBrush,
                BorderThickness = new Thickness(2),
                Background = App.Current.Resources["Brush06"] as SolidColorBrush,
                MinHeight = 26,
                MinWidth = 48,
            };
            Grid grid = new Grid();

            grid.Children.Add(content);
            TextBlock sec = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(6, 0, 6, 0),
                Foreground = App.Current.Resources["Brush03"] as SolidColorBrush,
                FontSize = 13
            };

            if (delay > 3)
            {
                grid.Children.Add(sec);
            }

            bd.Child = grid;
            _pop.Child = bd;
            _pop.Placement = PlacementMode.Bottom;
            _pop.PopupAnimation = PopupAnimation.Slide;
            _pop.HorizontalOffset = content.ActualWidth * -1;
            _pop.StaysOpen = true;

            _pop.Opened += (s1, e1) =>
            {
                int time = delay - 1 ;
                sec.Text = $"{time}";
                DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 1) };
                timer.Tick += (ss, ee) =>
                {
                    time--;
                    sec.Text = $"{time}";

                    if (time <= 0)
                    {
                        _pop.IsOpen = false;
                        _pop.Child = null;
                        _pop = null;
                        timer.Stop();
                        timer = null;
                    }
                };
                timer.Start();
            };
            _pop.IsOpen = true;

        }





    }
}
