using Microsoft.Win32;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
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
using System.Windows.Shapes;
using Woof.SystemEx;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для welcome.xaml
    /// </summary>
    public partial class welcome : Window
    {
        public welcome()
        {
            InitializeComponent();

            try
            {
                GifImage_Step_2.Visibility = Visibility.Hidden;
                ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Step_1_Click(object sender, RoutedEventArgs e)
        {
            WelcomTabs.SelectedIndex = 1;
            Step_Text.Content = "О себе";
            try
            {
                User_Name.Text = SysInfo.LogonUser.FullName;
            }
            catch
            {
                User_Name.Text = "Пользователь";
            }

            languege_combo.SelectedIndex = 0;

            Step_Progress.Value = 25;
        }

        private void Start_App_Click(object sender, RoutedEventArgs e)
        {
            //OOBE Close
            Properties.Settings.Default.First_Settings = false;
            Properties.Settings.Default.Save();

            MainWindow main = new MainWindow();
            main.Show();
            Close();
        }

        private void Step_2_Click(object sender, RoutedEventArgs e)
        {
            if (languege_combo.SelectedIndex == 1)
            {
                Properties.Settings.Default.Language = "English";
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Language = "Russian";
                Properties.Settings.Default.Save();
            }

            LOG.Content = "";
            string name = User_Name.Text;
            if (name.Length >= 4)
            {
                GifImage_Step_1.Visibility = Visibility.Hidden;
                GifImage_Step_2.Visibility = Visibility.Visible;
                WelcomTabs.SelectedIndex = 2;

                Properties.Settings.Default.User_Name = User_Name.Text;
                Properties.Settings.Default.Save();

                Step_Text.Content = "Настроим под себя";
                Step_Progress.Value = 50;
            }
            else
            {
                LOG.Content = "Пожалуйста, введите ваше имя (Больше 4 символов, у вас: " + name.Length + ").";
            }
        }

        private void Steap_4(object sender, RoutedEventArgs e)
        {
            Step_Text.Content = "Базовая настройка";
            Step_Progress.Value = 75;
            WelcomTabs.SelectedIndex = 3;
            theme_combo.SelectedIndex = 0;
            pos_combo.SelectedIndex = 0;
        }

        private void Save_Settings_Click(object sender, RoutedEventArgs e)
        {
            if (theme_combo.SelectedIndex == 0) {
                Properties.Settings.Default.theme = "light";
                Properties.Settings.Default.color_panel = "#fff";
            }
            else {
                Properties.Settings.Default.theme = "black";
                Properties.Settings.Default.color_panel = "#404040";
            }

            if (pos_combo.SelectedIndex == 0) { Properties.Settings.Default.posicion = "rigth"; }
            else { Properties.Settings.Default.posicion = "left"; }

            string ExePath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\ModernNotify.exe";

            string name = "ModernNotify";
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");

            if ((bool)StartInWindows.IsChecked)
            {
                
                try
                {
                    reg.SetValue(name, ExePath);
                    reg.Close();
                }
                catch
                {
                    StartInWindows.IsChecked = false;
                }
            }
            else
            {
                try
                {
                    reg.DeleteValue(name);
                    reg.Close();
                }
                catch
                {
                    StartInWindows.IsChecked = false;
                }
            }

            //OOBE Close
            Properties.Settings.Default.First_Settings = false;
            Properties.Settings.Default.Save();
            
            MainWindow main = new MainWindow();
            main.Show();
            Close();
        }
    }
}
