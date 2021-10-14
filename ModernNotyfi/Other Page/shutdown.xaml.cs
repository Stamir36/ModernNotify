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
           

        }


        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            //Настройки
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
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F2343434"); // Чёрный фон
            //ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);
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
    }
}