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
using ModernNotyfi.Other_Page;

namespace ModernNotyfi
{
    public partial class gamePanel : Window
    {

        // ------------------------------ FPS CODE ----------------------------------
        private WebViewManager webViewManager;
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

        public gamePanel()
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

            TimeClock.Text = DateTime.Now.ToString("HH:mm");

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Top;

            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

            if (Properties.Settings.Default.User_Name != "None")
            {
                User_Name.Content = Properties.Settings.Default.User_Name;
            }
            else
            {
                User_Name.Content = "Пользователь";
            }


            if (Properties.Settings.Default.Unesell_Login == "Yes")
            {
                acctype.Content = "Учётная запись Unesell Studio";
                try
                {
                    var userBitmapSmall = new BitmapImage(new Uri("https://unesell.com/data/users/avatar/" + Properties.Settings.Default.Unesell_Avatar));
                    AccauntImg.ImageSource = userBitmapSmall;
                }
                catch
                {
                    var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
                    AccauntImg.ImageSource = userBitmapSmall;
                }
                WebContent.Visibility = Visibility.Visible;
                NoAccount.Visibility = Visibility.Collapsed;
            }
            else
            {
                WebLoader.Visibility = Visibility.Collapsed;
                WebContent.Visibility = Visibility.Collapsed;
                NoAccount.Visibility = Visibility.Visible;
            }

            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);


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
                TimeClock.Text = DateTime.Now.ToString("HH:mm");
                ManagementClass wmi = new ManagementClass("Win32_Battery");
                ManagementObjectCollection allBatteries = wmi.GetInstances();

                foreach (var battery in allBatteries)
                {
                    string doubleBattery = Convert.ToString(battery["EstimatedChargeRemaining"]);
                    int IconID = int.Parse(doubleBattery[0].ToString());
                    if (Convert.ToDouble(battery["EstimatedChargeRemaining"]) < 10) batteryIcon.Glyph = WPFUI.Common.Icon.Battery020;
                    if (IconID == 1) batteryIcon.Glyph = WPFUI.Common.Icon.Battery120;
                    if (IconID == 2) batteryIcon.Glyph = WPFUI.Common.Icon.Battery220;
                    if (IconID == 3) batteryIcon.Glyph = WPFUI.Common.Icon.Battery320;
                    if (IconID == 4) batteryIcon.Glyph = WPFUI.Common.Icon.Battery420;
                    if (IconID == 5) batteryIcon.Glyph = WPFUI.Common.Icon.Battery520;
                    if (IconID == 6) batteryIcon.Glyph = WPFUI.Common.Icon.Battery620;
                    if (IconID == 7) batteryIcon.Glyph = WPFUI.Common.Icon.Battery720;
                    if (IconID == 8) batteryIcon.Glyph = WPFUI.Common.Icon.Battery820;
                    if (IconID == 9) batteryIcon.Glyph = WPFUI.Common.Icon.Battery920;

                    BatteryProcent.Text = doubleBattery + "%";
                }

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

                // CPU and RAM
                RAM.Text = Convert.ToString(Convert.ToInt16(100 - Math.Round(SysInfo.SystemMemoryFree, 2) * 100 / Math.Round(SysInfo.SystemMemoryTotal, 2)));
                RAMBar.Progress = Convert.ToInt16(100 - Math.Round(SysInfo.SystemMemoryFree, 2) * 100 / Math.Round(SysInfo.SystemMemoryTotal, 2));
            };

            //RunCPU();
        }

        public async void RunCPU()
        {
            bool done = false;
            float t = 0;
            await Task.Run(() =>
            {
                PerformanceCounter total_cpu = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
                while (!done)
                {
                    t = total_cpu.NextValue();
                    System.Threading.Thread.Sleep(1000);
                    t = total_cpu.NextValue();
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        CPU.Text = Convert.ToString(Convert.ToInt32(t));
                        CPUBar.Progress = Convert.ToInt32(t);
                    })); 
                }
            });
        }

        public void Light_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#E5E1E1E1");
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Light);
        }

        public void Dark_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F21B1B1B");
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);
        }

        private void RefreshFrame()
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

            System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
            float DesktopDpiX = desktop.DpiX;

            MARGINS margins = new MARGINS();
            margins.cxLeftWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
            margins.cxRightWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
            margins.cyTopHeight = Convert.ToInt32(((int)ActualHeight + 5) * (DesktopDpiX / 96));
            margins.cyBottomHeight = Convert.ToInt32(5 * (DesktopDpiX / 96));

            ExtendFrame(mainWindowSrc.Handle, margins);
        }

        private void RefreshDarkMode()
        {
            var isDark = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark;
            int flag = isDark ? 1 : 0;
            SetWindowAttribute(
                new WindowInteropHelper(this).Handle,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                flag);
        }

        // WIN 11 (22581+) STYLE 
        private void ApplyBackgroundEffect(int index)
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;

            WPFUI.Appearance.Background.Remove(windowHandle);

            if (Properties.Settings.Default.theme == "dark")
            {
                WPFUI.Appearance.Background.ApplyDarkMode(windowHandle);
            }
            else
            {
                WPFUI.Appearance.Background.RemoveDarkMode(windowHandle);
            }

            switch (index)
            {
                case -1:
                    this.Background = Brushes.Transparent;
                    WPFUI.Appearance.Background.Apply(windowHandle, WPFUI.Appearance.BackgroundType.Auto);
                    break;

                case 0:
                    Dark_Theme();
                    this.Background = Brushes.Transparent;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 2);
                    break;

                case 1:
                    Dark_Theme();
                    this.Background = Brushes.Transparent;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 3);
                    break;

                case 2:
                    Dark_Theme();
                    this.Background = Brushes.Transparent;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 4);
                    break;
            }
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {         
            if (Properties.Settings.Default.theme == "light")
            {
                Light_Theme();
                User_Name.Foreground = Brushes.Black;
                close.Foreground = Brushes.Black;
                acctype.Foreground = Brushes.Black;
            }
            else
            {
                Dark_Theme();
                User_Name.Foreground = Brushes.White;
                close.Foreground = Brushes.White;
                acctype.Foreground = Brushes.White;
            }

            if (Properties.Settings.Default.WinStyle == "Mica")
            {
                if (Properties.Settings.Default.MicaBool == true)
                {
                    ApplyBackgroundEffect(0);
                }
            }
            if (Properties.Settings.Default.WinStyle == "Acrylic")
            {
                if (Properties.Settings.Default.MicaBool == true)
                {
                    ApplyBackgroundEffect(1);
                }
            }
            if (Properties.Settings.Default.WinStyle == "Tabbed")
            {
                if (Properties.Settings.Default.MicaBool == true)
                {
                    ApplyBackgroundEffect(2);
                }
            }

            webViewManager = new WebViewManager(webView);
            webView.NavigationStarting += EnsureHttps;
            webView.NavigationCompleted += NavigationCompleted;
        }

        void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('headers').style = 'background-blend-mode: unset;background: transparent;';");
            webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('textimput').style = 'padding: 10px; background: transparent; position: fixed; align-items: normal; border-top: 0px;';");
            webView.CoreWebView2.ExecuteScriptAsync("document.querySelector('.chat-box').style = 'padding-bottom: 50px !important; padding-bottom: 50px; padding: 15px; zoom: 90%;';");

            String uri = args.Uri;
            if (uri == "https://unesell.com/app/cursus/")
            {
                webView.CoreWebView2.ExecuteScriptAsync("location.href = 'https://unesell.com/app/cursus/users.php?login_id=" + Properties.Settings.Default.Unesell_id + "';");
            }
        }

        private void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            WebLoader.Visibility = Visibility.Collapsed;
            webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('headers').style = 'background-blend-mode: unset;background: transparent;';");
            webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('textimput').style = 'padding: 10px; background: transparent; position: fixed; align-items: normal; border-top: 0px;';");
            webView.CoreWebView2.ExecuteScriptAsync("document.querySelector('.chat-box').style = 'padding-bottom: 50px !important; padding-bottom: 50px; padding: 15px; zoom: 90%;';");

            if (Properties.Settings.Default.theme != "light"){ webView.CoreWebView2.ExecuteScriptAsync("document.body.classList.toggle('dark-mode');"); }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Exit_App_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        ProcessStartInfo commands = new ProcessStartInfo();

        private void ShutdownWindows(object sender, RoutedEventArgs e)
        {
            //Выключение компьютера
            commands.FileName = "cmd.exe";
            commands.Arguments = "/c shutdown -s -f -t 00";
            Process.Start(commands);
        }

        private void RestartWindows(object sender, RoutedEventArgs e)
        {
            //Перезагрузка компьютера
            commands.FileName = "cmd.exe";
            commands.Arguments = "/c shutdown -r -f -t 00";
            Process.Start(commands);
        }

        private void Run_TaskMenedger_Go(object sender, RoutedEventArgs e)
        {
            Process.Start("taskmgr.exe");
        }

        private void Settings_Closed(object sender, EventArgs e)
        {
            webView.NavigationStarting -= EnsureHttps;
            webView.NavigationCompleted -= NavigationCompleted;
            webViewManager.Dispose();

            m_EtwSession.Dispose();
            timer.Stop();
            timer.IsEnabled = false;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.gameBar_show = false;
            }));

            // Вызываем сборщик мусора
            GC.Collect();
        }

        private void Minimization(object sender, RoutedEventArgs e)
        {
            gameBar gameBar = new gameBar();
            gameBar.Show();
            Close();
        }

        private void webView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (Properties.Settings.Default.Unesell_Login == "Yes")
            {
                webView.CoreWebView2.Navigate("https://unesell.com/app/cursus/users.php?login_id=" + Properties.Settings.Default.Unesell_id);
            }
            else
            {
                webView.NavigationStarting -= EnsureHttps;
                webView.NavigationCompleted -= NavigationCompleted;
                webViewManager.Dispose();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UiLoginUnesell uiLoginUnesell = new UiLoginUnesell();
            uiLoginUnesell.Show();
            Close();
        }
    }

    //вспомогательный класс для хранения моментов времени отрисовки кадров
    public class TimestampCollection
    {
        const int MAXNUM = 1000;

        public string Name { get; set; }
        public int pid { get; set; }

        List<long> timestamps = new List<long>(MAXNUM + 1);
        object sync = new object();

        //добавление значения в коллекцию
        public void Add(long timestamp)
        {
            lock (sync)
            {
                timestamps.Add(timestamp);
                if (timestamps.Count > MAXNUM) timestamps.RemoveAt(0);
            }
        }

        //получение числа значений в определенном интервале времени
        public int QueryCount(long from, long to)
        {
            int c = 0;

            lock (sync)
            {
                foreach (var ts in timestamps)
                {
                    if (ts >= from && ts <= to) c++;
                }
            }
            return c;
        }
    }
}