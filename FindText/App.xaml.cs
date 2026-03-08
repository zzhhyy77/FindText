using FindText.Helpers;
using FindText.Themes;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace FindText
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            try
            {
                Startup += App_Startup;
                this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "App");
                App.Current.Shutdown();
            }
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                if (IsRunning())
                {
                    Application.Current.Shutdown();
                    return;
                }

                AppCache.Instance.LoadAppConfigs();
                //TextCache.Text.SetLanguage("En");
                try
                {
                    if (!string.IsNullOrEmpty(AppCache.Instance.Configs.Language))
                        TextCache.SetLanguage(AppCache.Instance.Configs.Language);

                    if (!string.IsNullOrEmpty(AppCache.Instance.Configs.Theme))
                        ThemeManager.ApplyTheme(AppCache.Instance.Configs.Theme);

                }
                catch { }

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
                Application.Current.Shutdown();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is System.Exception)
                {
                    HandleException((System.Exception)e.ExceptionObject);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void HandleException(Exception ex)
        {
            MessageBox.Show(ex.Message, "全局异常");
        }

        private bool IsRunning()
        {
            Process process = Process.GetCurrentProcess();
            foreach (Process p in Process.GetProcessesByName(process.ProcessName))
            {
                if (p.Id != process.Id)
                {
                    Win32Helper.ShowWindow(p.MainWindowHandle, 1);
                    Win32Helper.SetForegroundWindow(p.MainWindowHandle);
                    return true;
                }
            }
            return false;
        }

    }



}
