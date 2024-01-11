using ModernWpf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Woof.SystemEx;
using static ModernNotyfi.PInvoke.ParameterTypes;
using static ModernNotyfi.PInvoke.Methods;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Diagnostics.Tracing.Session;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using ModernNotyfi.Other_Page;

namespace ModernNotyfi
{
    public partial class gameBar : Window
    {
        private WebViewManager webViewManager;
        // ------------------------------ FPS CODE ----------------------------------

        //коды событий (https://github.com/GameTechDev/PresentMon/blob/40ee99f437bc1061a27a2fc16a8993ee8ce4ebb5/PresentData/PresentMonTraceConsumer.cpp)
        public const int EventID_D3D9PresentStart = 1;
        public const int EventID_DxgiPresentStart = 42;

        //коды провайдеров ETW
        public static readonly Guid DXGI_provider = Guid.Parse("{CA11C036-0102-4A2D-A6AD-F03CFED5D3C9}");
        public static readonly Guid D3D9_provider = Guid.Parse("{783ACA0A-790E-4D7F-8451-AA850511C6B9}");

        static TraceEventSession m_EtwSession;
        static Dictionary<int, TimestampCollection> frames = new Dictionary<int, TimestampCollection>();
        static Stopwatch watch = null;
        static object sync = new object();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        public static double FPS_APP_ACTIVE = 0;
        public static string NAME_ACTIVE_APP = "";
        public static string PATH_ACTIVE_APP = "";

        static void EtwThreadProc()
        {
            //запускает отслеживание событий
            m_EtwSession.Source.Process();
        }

        static void OutputThreadProc()
        {
            //цикл вывода результатов в консоль
            while (true)
            {
                long t1, t2;
                long dt = 2000;

                // Получение активного окна
                IntPtr h = GetForegroundWindow();
                int pid = 0;
                GetWindowThreadProcessId(h, ref pid);
                Process p = Process.GetProcessById(pid);

                lock (sync)
                {
                    t2 = watch.ElapsedMilliseconds;
                    t1 = t2 - dt;

                    foreach (var x in frames.Values)
                    {
                        if (x.pid == pid)
                        {
                            //получаем количество кадров в интервале времени
                            int count = x.QueryCount(t1, t2);

                            //вычисляем FPS
                            FPS_APP_ACTIVE = (double)count / dt * 1000.0;
                        }
                    }
                }

                try
                {
                    if (p.MainWindowTitle.Length > 0)
                    {
                        NAME_ACTIVE_APP = p.MainWindowTitle;
                        PATH_ACTIVE_APP = p.MainModule.FileName;
                    }
                    else
                    {
                        NAME_ACTIVE_APP = "Переключитесь на игру";
                    }
                }
                catch
                {
                    // Skip
                }

                Thread.Sleep(1000);
            }
        }


        // -----------------------  MAIN CODE ----------------------------------
        public SeriesCollection SeriesCollection { get; set; }

        public DispatcherTimer timer = new DispatcherTimer();

        public gameBar()
        {
            InitializeComponent();


            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservableValue>{},
                    PointGeometrySize = 0,
                    StrokeThickness = 4,
                    Fill = Brushes.Transparent
                }
            };


            DataContext = this;

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

            this.Left = desktopWorkingArea.Left;
            this.Top = desktopWorkingArea.Top;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.gameBar_show = true;
            }));

            // FPS / FPSChart

            //создаем сессию ETW и регистрируем провайдеры
            m_EtwSession = new TraceEventSession("mysess");
            m_EtwSession.StopOnDispose = true;
            m_EtwSession.EnableProvider("Microsoft-Windows-D3D9");
            m_EtwSession.EnableProvider("Microsoft-Windows-DXGI");

            //подписываемся на событие
            m_EtwSession.Source.AllEvents += data =>
            {
                //интересуют только события рендеринга кадров
                if (((int)data.ID == EventID_D3D9PresentStart && data.ProviderGuid == D3D9_provider) ||
                ((int)data.ID == EventID_DxgiPresentStart && data.ProviderGuid == DXGI_provider))
                {
                    int pid = data.ProcessID;
                    long t;

                    lock (sync)
                    {
                        t = watch.ElapsedMilliseconds;

                        //если найден новый процесс, добавляем в Dictionary
                        if (!frames.ContainsKey(pid))
                        {
                            frames[pid] = new TimestampCollection();

                            string name = "";
                            var proc = Process.GetProcessById(pid);
                            if (proc != null)
                            {
                                using (proc)
                                {
                                    name = proc.ProcessName;
                                }
                            }
                            else name = pid.ToString();

                            frames[pid].Name = name;
                            frames[pid].pid = pid;
                        }

                        //добавляем кадр в коллекцию
                        frames[pid].Add(t);
                    }
                }
            };

            watch = new Stopwatch();
            watch.Start();

            Thread thETW = new Thread(EtwThreadProc);
            thETW.IsBackground = true;
            thETW.Start();

            Thread thOutput = new Thread(OutputThreadProc);
            thOutput.IsBackground = true;
            thOutput.Start();

            //TimeClock

            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                ManagementClass wmi = new ManagementClass("Win32_Battery");
                ManagementObjectCollection allBatteries = wmi.GetInstances();

                //FPS.Content = FPS_APP_ACTIVE + " кадров / сек";
                Game.Content = NAME_ACTIVE_APP;
                FPS_Copy.Content = Convert.ToInt32(FPS_APP_ACTIVE);
                SeriesCollection[0].Values.Add(new ObservableValue(Convert.ToInt32(FPS_APP_ACTIVE)));

                if (SeriesCollection[0].Values.Count > 30)
                {
                    SeriesCollection[0].Values.Remove(SeriesCollection[0].Values[0]);
                }

                try
                {
                    System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(PATH_ACTIVE_APP);
                    if (icon != null)
                    {
                        using (var bmp = icon.ToBitmap())
                        {
                            var stream = new MemoryStream();
                            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            IconApp.Source = BitmapFrame.Create(stream);
                        }
                    }
                }
                catch
                {
                    IconApp.Source = null;
                }

                if (NAME_ACTIVE_APP == "Переключитесь на игру")
                {
                    IconApp.Source = null;
                }
                
            };

            webView.NavigationStarting += EnsureHttps;
            Loaded += GameBar_Loaded;
        }

        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = -20;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        private void GameBar_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            IntPtr hwnd = helper.Handle;

            int extendedStyle = WS_EX_TRANSPARENT;
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle);

            webViewManager = new WebViewManager(webView);
            webView.NavigationStarting += EnsureHttps;
            webView.NavigationCompleted += NavigationCompleted;
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {

            };
        }

        void CheckWebViewSource()
        {
            // Проверка, инициализирован ли CoreWebView2
            if (webView.CoreWebView2 != null)
            {
                // Получение URL текущей страницы
                string currentUrl = webView.CoreWebView2.Source.ToString();

                // Вывод URL в консоль
                Console.WriteLine(currentUrl);
            }
        }


        void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('headers').style.display = 'none';");
            webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('textimput').style = 'padding: 10px; background: transparent; position: fixed; align-items: normal; border-top: 0px;';");
            webView.CoreWebView2.ExecuteScriptAsync("document.querySelector('.chat-box').style = 'padding-bottom: 50px !important; padding-bottom: 50px; padding: 15px; zoom: 90%;';");
            webView.CoreWebView2.ExecuteScriptAsync("document.body.classList.toggle('dark-mode');");

            String uri = args.Uri;
            if (uri == "https://unesell.com/app/cursus/")
            {
                webView.CoreWebView2.ExecuteScriptAsync("location.href = 'https://unesell.com/app/cursus/users.php?login_id=" + Properties.Settings.Default.Unesell_id + "';");
            }
        }

        private void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            WebLoader.Visibility = Visibility.Collapsed;
            webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('headers').style.display = 'none';");
            webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('textimput').style = 'padding: 10px; background: transparent; position: fixed; align-items: normal; border-top: 0px;';");
            webView.CoreWebView2.ExecuteScriptAsync("document.querySelector('.chat-box').style = 'padding-bottom: 50px !important; padding-bottom: 50px; padding: 15px; zoom: 90%;';");
            webView.CoreWebView2.ExecuteScriptAsync("document.body.classList.toggle('dark-mode');");
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Settings_Closed(object sender, EventArgs e)
        {
            m_EtwSession.Dispose();
            timer.Stop();
            timer.IsEnabled = false;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.gameBar_show = false;
            }));
        }

        private void GameBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void Fullscrean(object sender, RoutedEventArgs e)
        {
            try
            {
                gamePanel gamePanel = new gamePanel();
                gamePanel.Show();
                Close();
            }
            catch
            {

            }
            
        }

        private void openChatBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Perfomance.Visibility == Visibility.Visible)
            {
                Perfomance.Visibility = Visibility.Collapsed;

                if (Properties.Settings.Default.Unesell_Login == "Yes")
                {
                    Web.Visibility = Visibility.Visible;
                }
                else
                {
                    NoAccount.Visibility = Visibility.Visible;
                    WebLoader.Visibility = Visibility.Collapsed;
                }

                openChatBtn.Content = "Назад";

                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                this.Left = desktopWorkingArea.Left + 5;
                this.Top = desktopWorkingArea.Bottom - this.Height;
            }
            else
            {
                Perfomance.Visibility = Visibility.Visible;
                Web.Visibility = Visibility.Collapsed;
                NoAccount.Visibility = Visibility.Collapsed;
                openChatBtn.Content = "Открыть чат";

                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                this.Left = desktopWorkingArea.Left;
                this.Top = desktopWorkingArea.Top;
            }
        }

        private void GameBar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            webView.NavigationStarting -= EnsureHttps;
            webView.NavigationCompleted -= NavigationCompleted;
            webViewManager.Dispose();
            GC.Collect();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UiLoginUnesell uiLoginUnesell = new UiLoginUnesell();
            uiLoginUnesell.Show();
            Close();
        }
    }

    public class WebViewManager : IDisposable
    {
        private WebView2 webView;

        public WebViewManager(WebView2 webView)
        {
            this.webView = webView;
        }

        public void Dispose()
        {
            try
            {
                if (Properties.Settings.Default.Unesell_Login == "Yes"){ webView.CoreWebView2.Stop(); }
                webView.Dispose();
            }
            catch{}
        }
    }
}