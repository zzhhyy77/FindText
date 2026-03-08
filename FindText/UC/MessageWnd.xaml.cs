using System.Runtime.CompilerServices;
using System.Windows;

namespace FindText.UC
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWnd : Window
    {
        public static bool Show(string title, string msg)
        {
            MessageWnd wnd = new MessageWnd(title, msg);
            bool? rev = wnd.ShowDialog();
            if (rev == null || rev == false)
                return false;
            else
                return true;
        }

        public MessageWnd(string title, string message)
        {
            InitializeComponent();
            textblockTitle.Text = title;
            textblockMsg.Text = message;
            this.Owner = App.Current.MainWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
