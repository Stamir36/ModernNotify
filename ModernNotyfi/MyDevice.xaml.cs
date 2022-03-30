using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using WPFUI.Common;
using WPFUI;

namespace ModernNotyfi
{
    /// <summary>
    /// https://wpfui.lepo.co/documentation/
    /// </summary>
    public partial class MyDevice : Window
    {
        public DispatcherTimer timer = new DispatcherTimer();

        public MyDevice()
        {
            WPFUI.Appearance.Background.Apply(
              this,                                // Window class
              WPFUI.Appearance.BackgroundType.Acrylic // Background type
            );

            InitializeComponent();
            ComputerID.Text = GetMotherBoardID();

            SelectUpload.Visibility = Visibility.Visible; GoUpload.Visibility = Visibility.Hidden;

            if (Properties.Settings.Default.ConnectMobile != "null")
            {
                TabConnect.SelectedIndex = 4;
            }

            GoResponseAsync();

            timer.Interval = new TimeSpan(0, 0, 0, 2, 500);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                GoResponseAsync();
            };
            timer.Start();
        }

        public async void GoResponseAsync()
        {
            if (Properties.Settings.Default.ConnectMobile != "null")
            {
                string responseString = string.Empty;

                try
                {
                    await Task.Run(() =>
                    {
                        using (var webClient = new WebClient())
                        {
                            responseString = webClient.DownloadString("http://api.unesell.com/connect/check_connect.php?id=" + GetMotherBoardID());
                        }
                    });
                }
                catch
                {
                    TabConnect.SelectedIndex = 3;
                }

                Task.WaitAll();

                if (responseString != string.Empty)
                {
                    CheckConnectUpdate(responseString);
                }
                else
                {
                    TabConnect.SelectedIndex = 3;
                }
            }
            else
            {
                TabConnect.SelectedIndex = 0;
            }
        }


        public void CheckConnectUpdate(string responseString)
        {
            //Проверка подключения
            if (Properties.Settings.Default.ConnectMobile != "null")
            {
                if(TabConnect.SelectedIndex == 0)
                {
                    TabConnect.SelectedIndex = 1;
                }

                try
                {
                    if (responseString == "null")
                    {
                        Properties.Settings.Default.ConnectMobile = "null";
                        Properties.Settings.Default.Save();
                        return;
                    }

                    string BATTETY = Convert.ToString(JObject.Parse(responseString).SelectToken("BATTETY"));
                    string MOBILE = Convert.ToString(JObject.Parse(responseString).SelectToken("MOBILE"));
                    string MEM1_s = Convert.ToString(JObject.Parse(responseString).SelectToken("MEM1"));
                    string MEM2_s = Convert.ToString(JObject.Parse(responseString).SelectToken("MEM2"));

                    double MEM1 = Convert.ToDouble(MEM1_s); // Всего
                    double MEM2 = Convert.ToDouble(MEM2_s); // Доступно
                    double free = MEM1 - MEM2;

                    DeviceName.Subtitle = "Связь с " + MOBILE;
                    BatteeyLevel.Content = BATTETY + "%";
                    BatteryBarr.Width = Convert.ToInt16(BATTETY) * 2;

                    MemoryLevel.Content = MEM2 + "Gb";
                    MemoryBarr.Width = Convert.ToInt16(free * 100 / MEM1) * 2;

                    var bc = new BrushConverter();
                    if (Convert.ToInt16(BATTETY) > 20)
                    {
                        BatteryBarr.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#7F08FF00");
                    }
                    else
                    {
                        BatteryBarr.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#7FFF0000");
                    }

                    if (TabConnect.SelectedIndex == 4)
                    {
                        TabConnect.SelectedIndex = 1;
                    }
                }
                catch(Exception e)
                {
                    TabConnect.SelectedIndex = 3;
                }
            }
            else
            {
                TabConnect.SelectedIndex = 0;
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

            if (mbInfo == "None")
            {
                mbInfo = "virtualMachine";
            }

            return mbInfo;
        }

        private void ConnectWindows(object sender, RoutedEventArgs e)
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
            try{
                using (var webClient = new WebClient())
                {
                    var response = webClient.DownloadString("http://api.unesell.com/connect/disconnect.php?id=" + GetMotherBoardID());
                }

                Properties.Settings.Default.ConnectMobile = "null";
                Properties.Settings.Default.Save();
                GoResponseAsync();
                timer.Stop();
            }
            catch{
                DisplayDialog("Отключение не выполнено", "Мы не смогли подключиться к серверу, чтобы отвязать устройство. Пожалуйста, проверьте соединение с сетью.");
            }
        }

        private void Help_me_Click(object sender, RoutedEventArgs e)
        {
            ProblemDialog.Show = true;
            ProblemDialog.Visibility = Visibility.Visible;
        }

        private void CloseSupportDialog(object sender, RoutedEventArgs e)
        {
            ProblemDialog.Show = false;
            ProblemDialog.Visibility = Visibility.Hidden;
        }

        private void SupportSiteOpen(object sender, RoutedEventArgs e)
        {
            Process.Start("https://unesell.com/service/support/");
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

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            settings settings = new settings();
            settings.Show();
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://play.google.com/store/apps/details?id=com.unesell.mnc");
        }

        private void Files_Page_open(object sender, RoutedEventArgs e)
        {
            TabConnect.SelectedIndex = 2;
        }

        public bool SendBool = true;

        private void OpenFileSelector(object sender, RoutedEventArgs e)
        {
            // Выбор файла для загрузки
            string FileName;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true && SendBool == true)
            {
                FileName = openFileDialog.FileName;
                FileNameUpload.Content = "Подготовка к отправке файла...";
                SendFileDevice(FileName, openFileDialog.SafeFileName);
            }
        }

        private void Open_MyDevice(object sender, RoutedEventArgs e)
        {
            TabConnect.SelectedIndex = 1;
        }
        
        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && SendBool == true)
            {
                // можно же перетянуть много файлов, так что....
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                FileNameUpload.Content = "Подготовка к отправке файла...";
                SendFileDevice(files[0], System.IO.Path.GetFileName(files[0]));
            }
        }

        public void SendFileDevice(string path, string filename)
        {
            // Загрузка на сервер
            using (var client = new System.Net.WebClient())
            {
                SendBool = false; SelectUpload.Visibility = Visibility.Hidden; GoUpload.Visibility = Visibility.Visible;
                new ToastContentBuilder().AddArgument("action", "viewConversation").AddArgument("conversationId", 9813).AddText("Передача файла...").AddText("Отправка: " + filename).Show();
                Uri uri_upload = new Uri("http://api.unesell.com/connect/filesend.php?id=" + GetMotherBoardID());

                FileNameUpload.Content = filename;

                client.UploadProgressChanged += new UploadProgressChangedEventHandler(UploadProgressCallback);
                client.UploadFileCompleted += new UploadFileCompletedEventHandler(UploadFileCompleted);
                
                // Загрузка на сервер
                client.UploadFileAsync(uri_upload, path);
            }
        }

        private void UploadProgressCallback(object sender, UploadProgressChangedEventArgs e)
        {
            TextProgressUpload.Content = e.ProgressPercentage + "%";
            ProgressUploadBar.Value = e.ProgressPercentage;
        }

        private void UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            SendBool = true; SelectUpload.Visibility = Visibility.Visible; GoUpload.Visibility = Visibility.Hidden;
            new ToastContentBuilder().AddArgument("action", "viewConversation").AddArgument("conversationId", 9813).AddText("Файл загружен!").AddText("MN Connect готов скачивать файл.").Show();
        }

        private void Account_Open_Site(object sender, RoutedEventArgs e)
        {
            Process.Start("https://unesell.com/account/");
        }
    }
}
