using ModernWpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
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
using System.Windows.Threading;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для MyDevice.xaml
    /// </summary>
    public partial class MyDevice : Window
    {
        public DispatcherTimer timer = new DispatcherTimer();

        public MyDevice()
        {
            InitializeComponent();
            ComputerID.Content = "ID компьютера: " + GetMotherBoardID();
            if (Properties.Settings.Default.AllowsTransparency == true)
            { this.AllowsTransparency = true; }
            else
            { this.AllowsTransparency = false; }
            if (Properties.Settings.Default.theme == "light")
            {Light_Theme();}
            else
            {Dark_Theme();}

            CheckConnectUpdate();

            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                CheckConnectUpdate();
            };
            timer.Start();
        }

        

        public void CheckConnectUpdate()
        {
            //Проверка подключения
            if (Properties.Settings.Default.ConnectMobile != "null")
            {
                TabConnect.SelectedIndex = 1;
                DisConnect.Visibility = Visibility.Visible;
                Connect.Visibility = Visibility.Hidden;
                try
                {
                    string responseString = string.Empty;
                    using (var webClient = new WebClient())
                    {
                        responseString = webClient.DownloadString("https://unesell.000webhostapp.com/check_connect.php?id=" + GetMotherBoardID());
                    }

                    if (responseString == "null")
                    {
                        Properties.Settings.Default.ConnectMobile = "null";
                        Properties.Settings.Default.Save();
                        return;
                    }

                    string BATTETY = Convert.ToString(JObject.Parse(responseString).SelectToken("BATTETY"));
                    string MOBILE = Convert.ToString(JObject.Parse(responseString).SelectToken("MOBILE"));


                    DeviceName.Content = "Подключено к: " + MOBILE;
                    BatteeyLevel.Content = "Батарея: " + BATTETY + "%";
                }
                catch
                {
                    MessageBox.Show("Не удалось соединиться с сервером. Проверьте настройки подключения.", "Нет соединения.");
                }
            }
            else
            {
                TabConnect.SelectedIndex = 0;
                DisConnect.Visibility = Visibility.Hidden;
                Connect.Visibility = Visibility.Visible;
            }
        }


        public static string GetMotherBoardID()
        {
            string mbInfo = String.Empty;
            ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            scope.Connect();
            ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

            foreach (PropertyData propData in wmiClass.Properties)
            {
                if (propData.Name == "SerialNumber")
                    mbInfo = Convert.ToString(propData.Value);
            }

            return mbInfo;
        }

        public void Light_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F2FFFFFF"); // Белый фон
            ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
        }
        public void Dark_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F2343434"); // Чёрный фон
            ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);
        }

        private void ConnectWindows(object sender, RoutedEventArgs e)
        {
            ConnectQR connectQR = new ConnectQR();
            connectQR.Show();
            Close();
        }

        private void ConnectMain2(object sender, RoutedEventArgs e)
        {
            ConnectQR connectQR = new ConnectQR();
            connectQR.Show();
            Close();
        }

        private void MyDevice1_Closed(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void DisConnect_Click(object sender, RoutedEventArgs e)
        {
            using (var webClient = new WebClient())
            {
                var response = webClient.DownloadString("https://unesell.000webhostapp.com/disconnect.php?id=" + GetMotherBoardID());
            }

            Properties.Settings.Default.ConnectMobile = "null";
            Properties.Settings.Default.Save();
            CheckConnectUpdate();
            timer.Stop();
        }
    }
}
