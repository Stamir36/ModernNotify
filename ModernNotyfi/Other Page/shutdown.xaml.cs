using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Woof.SystemEx;
using WPFUI.Controls;
using static ModernNotyfi.PInvoke.ParameterTypes;
using static ModernNotyfi.PInvoke.Methods;
using System.Windows.Interop;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для settings.xaml
    /// </summary>
    public partial class shutdown : Window
    {

        public shutdown()
        {
            InitializeComponent();
            var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
            AccauntImg.ImageSource = userBitmapSmall;

            if (Properties.Settings.Default.User_Name != "None")
            {
                User_Name.Content = Properties.Settings.Default.User_Name;
            }
            else
            {
                User_Name.Content = "Пользователь";
            }

            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
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
                    this.Background = Brushes.Transparent;
                    Dark_Theme();
                    this.WindowStyle = WindowStyle.None;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 2);
                    break;

                case 1:
                    this.Background = Brushes.Transparent;
                    Dark_Theme();
                    this.WindowStyle = WindowStyle.None;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 3);
                    break;

                case 2:
                    this.Background = Brushes.Transparent;
                    Dark_Theme();
                    this.WindowStyle = WindowStyle.None;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 4);
                    break;
            }
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            Dark_Theme();
            User_Name.Foreground = Brushes.White;
            Text1.Foreground = Brushes.White;
            Text2.Foreground = Brushes.White;
            Text3.Foreground = Brushes.White;
            close.Foreground = Brushes.White;
            acctype.Foreground = Brushes.White;
            ApplyBackgroundEffect(1);
            //Настройки
            /*
            if (Properties.Settings.Default.theme == "light")
            {
                Light_Theme();
                User_Name.Foreground = Brushes.Black;
                Text1.Foreground = Brushes.Black;
                Text2.Foreground = Brushes.Black;
                Text3.Foreground = Brushes.Black;
                close.Foreground = Brushes.Black;
                acctype.Foreground = Brushes.Black;
            }
            else
            {
                Dark_Theme();
                User_Name.Foreground = Brushes.White;
                Text1.Foreground = Brushes.White;
                Text2.Foreground = Brushes.White;
                Text3.Foreground = Brushes.White;
                close.Foreground = Brushes.White;
                acctype.Foreground = Brushes.White;
            }
            if (Properties.Settings.Default.Show_Exit == "False")
            {
                Exit_App.Visibility = Visibility.Hidden;
            }
            */
        }

        // ModernWpf.ThemeManager.GetActualTheme(this).ToString(); <- Тема установленная в системе
        public void Light_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F2FFFFFF"); // Белый фон
            //ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
        }
        public void Dark_Theme()
        {
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);
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
    }
}