using ModernWpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using static ModernNotyfi.PInvoke.ParameterTypes;
using static ModernNotyfi.PInvoke.Methods;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для modal_info.xaml
    /// </summary>
    public partial class modal_info
    {

        public DispatcherTimer timer = new DispatcherTimer();


        public modal_info(string command)
        {
            InitializeComponent();
            Loaded += OnLoaded;

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - 50;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                double value = 0;
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (command == "volume")
                {
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                    timer.IsEnabled = true;
                    timer.Tick += (o, t) =>
                    {
                        my.modal_show = true;
                        value = my.SoundSlider.Value;
                        SoundValues.Value = value;
                        LabelValueSound.Content = value;
                        my.close_time = my.close_time - 1;

                        if (my.close_time <= 0)
                        {
                            this.Close();
                        }
                    };
                }                
            }));
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshFrame();
            RefreshDarkMode();

            SetWindowAttribute(new WindowInteropHelper(this).Handle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 3);
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

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.modal_show = false;
                timer.Stop();
            }));
        }
    }
}
