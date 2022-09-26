﻿using System;
using System.Collections.Generic;
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
using WPFUI.Common;
using WPFUI;
using static ModernNotyfi.PInvoke.ParameterTypes;
using static ModernNotyfi.PInvoke.Methods;
using System.Windows.Interop;
using System.Management;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для Startup.xaml
    /// </summary>
    public partial class Startup : Window
    {
        public Startup()
        {
            InitializeComponent();
            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F21B1B1B");
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);
        }

        private void StartProgram(object sender, RoutedEventArgs e)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get()) { Properties.Settings.Default.System = os["Caption"].ToString(); break; }

            Properties.Settings.Default.System = Properties.Settings.Default.System.Replace("Майкрософт ", "");

            Properties.Settings.Default.Save();

            string UserData = "https://unesell.com/api/account.info.id.php?id=" + Properties.Settings.Default.Unesell_id;

            if (Properties.Settings.Default.Unesell_id != "")
            {
                string responseString = string.Empty;
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    responseString = webClient.DownloadString(UserData);
                }

                if (responseString != "null")
                {
                    string name = Convert.ToString(JObject.Parse(responseString).SelectToken("name"));
                    Properties.Settings.Default.User_Name = name;
                    Properties.Settings.Default.Save();
                }
            }

            if (Properties.Settings.Default.Startup == "Panel")
            {
                if (Properties.Settings.Default.PanelStyle == 1)
                {
                    MainWindow panel = new MainWindow();
                    panel.Show();
                }
                else if(Properties.Settings.Default.PanelStyle == 2)
                {
                    ModernUI panel = new ModernUI();
                    panel.Show();
                }
                else
                {
                    MainWindow panel = new MainWindow();
                    panel.Show();
                }
            }
            else if (Properties.Settings.Default.Startup == "Connect")
            {
                MyDevice connect = new MyDevice();
                connect.Show();
            }
            else
            {
                MessageBox.Show("Error", "Конфигурация установлена не правильно. Очистите настройки или поставте параметр 'Startup' на значение ''Panel''.");
            }

            // Запуск сервесов.
            ServiceMyDeviceNet serviceMyDeviceNet = new ServiceMyDeviceNet();
            serviceMyDeviceNet.Show();

            this.Close();
        }
    }
}