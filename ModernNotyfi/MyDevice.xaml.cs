using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                        responseString = webClient.DownloadString("https://beesportal.online/connect/check_connect.php?id=" + GetMotherBoardID());
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
                    BatteeyLevel.Content = BATTETY + "%";
                    BatteryBarr.Width = Convert.ToInt16(BATTETY) * 2;

                    var bc = new BrushConverter();
                    if (Convert.ToInt16(BATTETY) > 20)
                    {
                        BatteryBarr.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#7F08FF00");
                    }
                    else
                    {
                        BatteryBarr.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#7FFF0000");
                    }
                }
                catch
                {
                    DisplayDialog("Нет соединения.", "Не удалось соединиться с сервером, возможно, сервер недоступен или пропал интернет.");
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
                var response = webClient.DownloadString("https://beesportal.online/connect/disconnect.php?id=" + GetMotherBoardID());
            }

            Properties.Settings.Default.ConnectMobile = "null";
            Properties.Settings.Default.Save();
            CheckConnectUpdate();
            timer.Stop();
        }

        private void Help_me_Click(object sender, RoutedEventArgs e)
        {
            DisplayDialog("Проблемы с подключением.", "● Сервер недоступен | Не обновляются данные. \n" +
                "MN Connect использует сервер для связи устройств, из-за ограниченой мощности устройства могут прекратить на некоторое время передавать друг-другу данные, подождите не много, пока нагрузка уменьшится.\n" +
                "\n● Данные не верные | Телефон отключён.\n" +
                "Проверьте обновления программ. Старые версии не могут работать с сервером, который требует новый код и функционал. Если новых обновлений нет, попробуйте переподключить устройство.\n" +
                "\n● Как я могу помочь проекту?\n" +
                "Чтобы повысить качество работы, нужны ресурсы для сервера. Вы можете поддержать разработчика финансово, связавщись с ним напрямую.");
        }

        private async void DisplayDialog(string Title, string Content)
        {
            ContentDialog DisplayDialog = new ContentDialog
            {
                Title = Title,
                Content = Content,
                CloseButtonText = "Закрыть"
            };

            ContentDialogResult result = await DisplayDialog.ShowAsync();
        }

        bool commandtabopen = false;

        private void Settings_Open_Click(object sender, RoutedEventArgs e)
        {
            if (commandtabopen == false)
            {
                commands.Visibility = Visibility.Visible;
                commandtabopen = true;
            }
            else
            {
                commands.Visibility = Visibility.Hidden;
                commandtabopen = false;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            settings settings = new settings();
            settings.Show();
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Stamir36/ModernNotyfi/raw/main/mnconnect.apk");
        }
    }
}
