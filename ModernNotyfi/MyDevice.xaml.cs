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
using static ModernNotyfi.PInvoke.ParameterTypes;
using static ModernNotyfi.PInvoke.Methods;
using System.Windows.Interop;
using Woof.SystemEx;

namespace ModernNotyfi
{
    /// <summary>
    /// https://wpfui.lepo.co/documentation/
    /// </summary>
    public partial class MyDevice : Window
    {
        public string api = "http://api.unesell.com/";
        //public string api = "http://localhost/api/";

        public DispatcherTimer timer = new DispatcherTimer();

        public MyDevice()
        {
            InitializeComponent();
            info.Content = "Ожидаем подключение...";
            if (Properties.Settings.Default.Language == "English")
            {
                info.Content = "Waiting for connection...";
            }

            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);

            if (Properties.Settings.Default.WinStyle == "Mica")
            {
                WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
            }
            else
            {
                WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
            }

            if (Properties.Settings.Default.theme == "system")
            {
                Loaded += (sender, args) =>
                {
                    if (Properties.Settings.Default.WinStyle == "Mica")
                    {
                        WPFUI.Appearance.Watcher.Watch(this, WPFUI.Appearance.BackgroundType.Mica, true);
                    }
                    else
                    {
                        WPFUI.Appearance.Watcher.Watch(this, WPFUI.Appearance.BackgroundType.Auto, true);
                    }
                };
            }

            if (Properties.Settings.Default.Language == "English")
            {
                EnglishInterfase_Settings();
            }

            if (Properties.Settings.Default.Startup == "Connect")
            {
                SettingsOpen.Visibility = Visibility.Visible;
            }
            else
            {
                SettingsOpen.Visibility = Visibility.Collapsed;
            }

            ComputerID.Text = GetMotherBoardID();

            SelectUpload.Visibility = Visibility.Visible; GoUpload.Visibility = Visibility.Hidden;

            if (Properties.Settings.Default.ConnectMobile != "null")
            {
                TabConnect.SelectedIndex = 4;
            }

            GoResponseAsync();

            DriveInfo di = new DriveInfo(@"C:\");
            double Ffree = (di.AvailableFreeSpace / 1024) / 1024 / 1024;
            DiskSpaceBar.Value = 100 - (di.AvailableFreeSpace * 100 / di.TotalSize);
            DiskSpace.Content = Ffree.ToString("#,##") + " Gb";


            timer.Interval = new TimeSpan(0, 0, 0, 2, 500);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                GoResponseAsync();
                Ffree = (di.AvailableFreeSpace / 1024) / 1024 / 1024;
                DiskSpaceBar.Value = 100 - (di.AvailableFreeSpace * 100 / di.TotalSize);
                DiskSpace.Content = Ffree.ToString("#,##") + " Gb";
            };
            timer.Start();
        }

        public void updateaccount()
        {
            if (Properties.Settings.Default.Unesell_Login == "Yes")
            {
                NameProfile.Text = Properties.Settings.Default.User_Name;
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
            }
            else
            {
                NameProfile.Text = "Вход не выполнен";
            }
        }

        private void EnglishInterfase_Settings()
        {
            beta_banner.Visibility = Visibility.Hidden;
            Title.Title = "My Device";
            LinkDownload.Content = "MN Connect - Connect with Android";
            Help_me.Content = "Troubleshooting and Support";
            IDDeviceText.Text = "ID Device:";
            ConnectingText.Text = "Server connection";
            NoInternetTitle.Content = "Oops, something's wrong... Check the internet!";
            NoInternetSubtext.Content = "Server connection error, please check your internet connection.";
            FileSendTitle.Content = "File sharing";
            SendFileTextSubtitle.Content = "Select a file or drag and drop to the window you want to send to the device";
            Open_Upload_File.Content = "Select file";
            back.Content = "Back";
            NoDeviceCard.Title = "No connection";
            NoDeviceCard.Subtitle = "Link your computer to your phone using MN Connect";
            Connect.Content = "Connect Android device";
            MainTextBanner.Content = "Connect your Android device to be able to:\n● Receive notifications from phone to computer\n● View battery level\n● Control playback on PC.\nAnd much more!\n\n\n\nTo connect, use the Android application MN Connect";
            Text1.Text = "Manage anywhere";
            Text2.Text = "All in the cloud, no LANs";
            Text3.Text = "Exact data";
            Text4.Text = "Delay just a couple of seconds";
            DisConnect.Content = "Unlink device.";
            DeviceName.Title = "Connected!";
            //DeviceName.Subtitle = "";
            btext2.Content = "Battery:";
            btext2_Copy.Content = "Free memory:";
            textcon2.Content = "Phone options:";
            mainsett_text.Text = "Sending files";
            sendfileText2.Text = "Send file to device";
            AcountText1.Text = "My Unesell Account";
            AcountText2.Text = "The site will open in a browser";
            SettingsOpen.Content = "Settings";
        }

        public void Light_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#E5E1E1E1");
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Light);
            //ApplyBackgroundEffect(stylewin_combo.SelectedIndex);
        }

        public void Dark_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F21B1B1B");
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);
            // ApplyBackgroundEffect(stylewin_combo.SelectedIndex);
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
                            responseString = webClient.DownloadString(api + "connect/check_connect.php?id=" + GetMotherBoardID());
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

                    // ---------- Получение информации (основной источник проблем) ------------


                    string BATTETY = Convert.ToString(JObject.Parse(responseString).SelectToken("BATTETY"));
                    string MOBILE = Convert.ToString(JObject.Parse(responseString).SelectToken("MOBILE"));
                    string MEM1_s = Convert.ToString(JObject.Parse(responseString).SelectToken("MEM1"));
                    string MEM2_s = Convert.ToString(JObject.Parse(responseString).SelectToken("MEM2"));
                    string OS = Convert.ToString(JObject.Parse(responseString).SelectToken("System"));

                    MEM1_s = MEM1_s.Replace(".", ","); MEM2_s = MEM2_s.Replace(".", ",");

                    double MEM1 = Convert.ToDouble(MEM1_s); // Всего
                    double MEM2 = Convert.ToDouble(MEM2_s); // Доступно
                    double free = MEM1 - MEM2;
                    MemoryLevel.Content = MEM2 + "Gb";
                    MemoryBarr.Progress = Convert.ToInt16(free * 100 / MEM1);

                    DeviceName.Subtitle = "Связь с " + MOBILE;
                    BatteeyLevel.Text = BATTETY;
                    BatteryBarr.Progress = Convert.ToInt16(BATTETY);
                    OSVersionAndroid.Content = OS;


                    var bc = new BrushConverter();
                    if (Convert.ToInt16(BATTETY) > 20)
                    {
                        BatteryBarr.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#7F08FF00");
                    }
                    else
                    {
                        BatteryBarr.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#FFD64545");
                    }

                    // ------------------

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
            
            if (Properties.Settings.Default.RunServiceBackground == false && Properties.Settings.Default.Startup == "Connect")
            {
                ServiceMyDeviceNet service = Application.Current.Windows.OfType<ServiceMyDeviceNet>().FirstOrDefault();
                service.Close();
            }
        }

        private void DisConnect_Click(object sender, RoutedEventArgs e)
        {
            try{
                using (var webClient = new WebClient())
                {
                    var response = webClient.DownloadString(api + "connect/disconnect.php?id=" + GetMotherBoardID());
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
                Uri uri_upload = new Uri(api + "connect/filesend.php?id=" + GetMotherBoardID());

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
            Process.Start("https://unesell.com/login/?go=mydevice");
        }

        private void MyDevice1_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.theme == "light")
            {
                Light_Theme();
            }
            else if (Properties.Settings.Default.theme == "dark")
            {
                Dark_Theme();
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

            var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
            AccauntImg.ImageSource = userBitmapSmall;
            updateaccount();
        }

        private void SettingsAllOpen(object sender, RoutedEventArgs e)
        {
            settings settings = new settings();
            settings.Show();

            this.Close();
        }

        private void ShowMyDeviceConsole(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceMyDeviceNet service = Application.Current.Windows.OfType<ServiceMyDeviceNet>().FirstOrDefault();
                service.ShowInTaskbar = true;
                service.Visibility = Visibility.Visible;
            }
            catch
            {
                ServiceMyDeviceNet service = new ServiceMyDeviceNet();
                service.Show();
            }
        }
    }
}
