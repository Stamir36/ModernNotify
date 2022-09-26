using NAudio.CoreAudioApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
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
using Woof.SystemEx;
using ModernNotyfi;
using System.Collections.ObjectModel;

namespace ModernNotyfi
{
    public partial class ServiceMyDeviceNet : Window
    {
        public string api = "http://api.unesell.com/";
        //public string api = "http://localhost/api/";

        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        ProcessStartInfo commands = new ProcessStartInfo();
        IEnumerable<MMDevice> speakDevices;
        public int soundDevice = 1; //Активное устройство воспроизведения
        public DispatcherTimer timer = new DispatcherTimer();
        ManagementClass wmi = new ManagementClass("Win32_Battery");

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        public const int KEYEVENTF_EXTENTEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 0;
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;

        public class ItemModel
        {
            public ItemModel(string name, ImageSource image, string sourse)
            {
                Name = name;
                Image = image;
                Sourse = sourse;
            }

            public string Name { get; set; }
            public string Sourse { get; set; }
            public ImageSource Image { get; set; }
        }

        public ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();

        public ServiceMyDeviceNet()
        {
            InitializeComponent();

            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);

            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#FF2B2B2B");
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);

            LOGConsole.Text = LOGConsole.Text + "Консоль запущена...";
            LOGConsole.Text = LOGConsole.Text + "\n" + "Идентификатор устройства: " + GetMotherBoardID();
            
            speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();

            if (Properties.Settings.Default.RunServiceBackground == false)
            { 
                RunBackgroundBool.IsChecked = false;
            }
            else
            {
                RunBackgroundBool.IsChecked = true;
            }

            if (Properties.Settings.Default.Startup == "Panel")
            {
                SwitchRunBackgraundText.Visibility = Visibility.Collapsed;
                RunBackgroundBool.Visibility = Visibility.Collapsed;
            }
        }

        public async void GetServerInfo()
        {
            try
            {
                string m1 = Convert.ToString(NowPlayning.Content);
                m1 = m1.Replace(' ', '_');
                string m2 = Convert.ToString(NowPlayning_Autor.Content);
                m2 = m2.Replace(' ', '_');

                int bb = 0;
                int volume = 0;

                ManagementObjectCollection allBatteries = wmi.GetInstances();


                foreach (var battery in allBatteries)
                {
                    bb = Convert.ToInt16(battery["EstimatedChargeRemaining"]); // БАТАРЕЯ
                }


                MMDevice mMDevice = speakDevices.ToList()[0];
                volume = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100); // ГРОМКОСТЬ

                Battery.Content = bb + "%";
                Volume.Content = volume + "%";

                await Task.Run(() => {

                    string url = api + "connect/pc_add_info.php?ID_PC=" + GetMotherBoardID() + "&BATTETY=" + bb + "&M1=" + m1 + "&M2=" + m2 + "&VOLUME=" + volume + "&UnesellID=" + Properties.Settings.Default.Unesell_id + "&SystemInfo=" + Properties.Settings.Default.System + "&SysMemoryTotal=" + Math.Round(SysInfo.SystemMemoryTotal, 2) + "&SysMemoryFree=" + Math.Round(SysInfo.SystemMemoryFree, 2);

                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                        var response = webClient.DownloadString(url);

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            //LOGConsole.Text = LOGConsole.Text + "\n" + "Данные отправлены: " + "ID_PC=" + GetMotherBoardID() + "&BATTETY=" + bb + "&M1=" + m1 + "&M2=" + m2 + "&VOLUME=" + volume + "&UnesellID=" + Properties.Settings.Default.Unesell_id + "&SystemInfo=" + Properties.Settings.Default.System + "&SysMemoryTotal=" + Math.Round(SysInfo.SystemMemoryTotal, 2) + "&SysMemoryFree=" + Math.Round(SysInfo.SystemMemoryFree, 2);
                        }));
                    }
                });

            }
            catch (Exception ex)
            {
                LOGConsole.Text = LOGConsole.Text + "\n" + "Ошибка отправки данных на сервер.";
            }
        }

        public async void GetMobileInfo()
        {
            try
            {
                string responseString = string.Empty;

                await Task.Run(() => {
                    using (var webClient = new WebClient())
                    {
                        responseString = webClient.DownloadString(api + "connect/check_connect.php?id=" + GetMotherBoardID());
                    }
                });
                if (responseString != "null")
                {
                    string command = Convert.ToString(JObject.Parse(responseString).SelectToken("command"));
                    if (command == "volume_up")
                    {
                        MMDevice mMDevice = speakDevices.ToList()[0];
                        mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = Convert.ToInt32(Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100) + 10) / 100.0f;
                    }
                    if (command == "volume_down")
                    {
                        MMDevice mMDevice = speakDevices.ToList()[0];
                        mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = Convert.ToInt32(Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100) - 10) / 100.0f;
                    }
                    if (command == "back_track")
                    {
                        keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
                    }
                    if (command == "next_track")
                    {
                        keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
                    }
                    if (command == "play_track")
                    {
                        keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
                    }
                    if (command == "shutdown")
                    {
                        commands.FileName = "cmd.exe";
                        commands.Arguments = "/c shutdown -s -f -t 00";
                        Process.Start(commands);
                    }
                    if (command.Contains("shutdown-s:"))
                    {
                        int sec = Convert.ToInt32(command.Replace("shutdown-s:", ""));
                        commands.FileName = "cmd.exe";
                        commands.Arguments = "/c shutdown -s -t " + sec;
                        Process.Start(commands);
                    }
                    if (command == "copy_bufer")
                    {
                        // Отправить буфер обмена
                        using (var webClient = new WebClient())
                        {
                            webClient.DownloadString(api + "connect/command_mobile.php?id=" + GetMotherBoardID() + "&command=copy:" + Clipboard.GetText());
                        }
                    }
                    if (command == "copy_bufer_web")
                    {
                        // Отправить буфер обмена самому себе для интерфейса MyDevice
                        using (var webClient = new WebClient())
                        {
                            webClient.DownloadString(api + "connect/command_mobile.php?id=" + GetMotherBoardID() + "&command=copyWEB:" + Clipboard.GetText());
                        }
                    }
                    if (command == "commands.apps.list")
                    {
                        string appList = "";

                        for (int i = 1; i <= Items.Count() / 14; i++)
                        {
                            appList = "";
                            for (int y = i * 14 - 14; y < i * 14; y++)
                            {
                                appList = appList + Items.ElementAt(y).Name + "|";
                            }

                            await Task.Run(() => {
                                appList = appList.Replace(" ", "_");
                                using (var webClient = new WebClient())
                                {
                                    webClient.DownloadString(api + "connect/command_mobile.php?id=" + GetMotherBoardID() + "&command=AppList:" + appList);
                                }
                            });
                            Task.Delay(1000).Wait();
                        }
                    }
                    if (command.Contains("appStart:"))
                    {
                        int indexApp = Convert.ToInt32(command.Replace("appStart:", ""));
                        Process.Start(Items.ElementAt(indexApp).Sourse);
                    }
                    GetServerInfo();
                }
            }
            catch (Exception ex)
            {
                // Ignore Error Send Server
                //MessageBox.Show("Error Send:\n" + ex);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            this.Visibility = Visibility.Collapsed;
            this.ShowInTaskbar = false;


            MediaManager.Start();
            AppListItit();


            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Left + 20;
            this.Top = desktopWorkingArea.Bottom - this.Height - 20;


            // ПОЛУ-СЕКУНДНЫЙ ТАЙМЕР
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += async (o, t) =>
            {

                // Обновление данных плеера. (Более рабочий вариант с таймером)
                try
                {
                    await Task.Run(async () => {
                        var gsmtcsm = await ModernNotyfi.MainWindow.GetSystemMediaTransportControlsSessionManager();
                        var mediaProperties = await ModernNotyfi.MainWindow.GetMediaProperties(gsmtcsm.GetCurrentSession());
                        if (mediaProperties.Title.Length > 0)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                NowPlayning.Content = mediaProperties.Title;
                                NowPlayning_Autor.Content = mediaProperties.Artist;

                                if (mediaProperties.Artist == "" || mediaProperties.Artist == null)
                                {
                                    NowPlayning_Autor.Content = "Нет автора";
                                }
                            }));
                        }
                    });
                }
                catch
                {
                    NowPlayning.Content = "Нет музыки";
                    NowPlayning_Autor.Content = "Давайте что-нибудь послушаем";
                }

                GetServerInfo();
                GetMobileInfo();
            };
        }

        public async void AppListItit()
        {
            string name_user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var appUser = Directory.GetFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", "*.lnk");
            var appWindows = Directory.GetFiles(name_user + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs", "*.lnk");
            var files = new string[appUser.Length + appWindows.Length];
            appUser.CopyTo(files, 0);
            appWindows.CopyTo(files, appUser.Length);

            files = await SortFilesPath(files);

            foreach (var file in files)
            {
                ImageSource imageSource = null;

                FileInfo fileInfo = new FileInfo(file);
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileInfo.FullName);

                if (icon != null)
                {
                    using (var bmp = icon.ToBitmap())
                    {
                        var stream = new MemoryStream();
                        bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        imageSource = BitmapFrame.Create(stream);
                    }
                }

                Items.Add(new ItemModel(fileInfo.Name.Replace(".lnk", ""), imageSource, fileInfo.FullName));
            }
        }

        private async Task<string[]> SortFilesPath(string[] files) => await Task.Run(() =>
        {
            int valuesort = 0;

            foreach (var sorts in files)
            {
                string SubAppUser = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\";
                string SubAppWindows = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\";

                if (sorts.Contains(SubAppUser))
                {
                    files[valuesort] = sorts.Replace(SubAppUser, "") + "||SubAppUser";
                }
                else
                {
                    files[valuesort] = sorts.Replace(SubAppWindows, "") + "||SubAppWindows";
                }

                valuesort++;
            }

            Array.Sort(files);
            valuesort = 0;

            foreach (var sorts in files)
            {
                string SubAppUser = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\";
                string SubAppWindows = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\";

                if (sorts.Contains("||SubAppUser"))
                {
                    files[valuesort] = SubAppUser + sorts.Replace("||SubAppUser", "");
                }
                else
                {
                    files[valuesort] = SubAppWindows + sorts.Replace("||SubAppWindows", "");
                }

                valuesort++;
            }

            return files;
        });

        private void InvisibleConsole(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;
        }

        public void ShowConsole()
        {
            this.Visibility = Visibility.Visible;
            this.ShowInTaskbar = true;
        }

        private void RunBackgroundBoolCheked(object sender, RoutedEventArgs e)
        {
            if (RunBackgroundBool.IsChecked == true)
            {
                Properties.Settings.Default.RunServiceBackground = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.RunServiceBackground = false;
                Properties.Settings.Default.Save();
            }
            Properties.Settings.Default.Save();
        }
    }
}
