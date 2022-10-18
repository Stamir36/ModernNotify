using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using ModernWpf;
using LiveCharts;
using Windows.Media.Control;
using System.Media;
using System.Runtime.InteropServices;
using System.Management;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Interop;
using Windows.UI.Notifications.Management;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Net;
using System.Reflection;
using Windows.Devices.Radios;
using Windows.Devices.Bluetooth;
using Windows.Devices.WiFi;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using WPFUI;
using WPFUI.Controls;
using Woof.SystemEx;

namespace ModernNotyfi
{
    /***
     * 
     * 
     *  АВТОРСКОЕ ПРАВО НА ЭТО ПРОГРАМНОЕ ОБЕСПЕЧЕНИЕ ПРЕНАДЛЕЖИТ:
     *  СТАНИСТАВУ МИРОШНИЧЕНКО. 2022 год. Unesell Studio.
     *  
     *  Любое изменение и использование невозможно без согласия автора.
     *  
     * 
     */

    public partial class MainWindow : Window
    {
        public string api = "http://api.unesell.com/";
        //public string api = "http://localhost/api/";

        // БАЗОВЫЕ НАСТРОЙКИ И ПАРАМЕТРЫ ---------------------------------------------------------
        public int soundDevice = 1; //Активное устройство воспроизведения
        public int SoundDeviceOpen = 0;
        public int AudioOnOff = 0; //Вкл \ Выкл звук.
        public int TempAudioOnOff = 0;

        public DispatcherTimer timer_minute = new DispatcherTimer();
        public DispatcherTimer timer = new DispatcherTimer();

        public int batt_status = 0; // 0 разряд / 1 заряд.
        public int batt_start = 0;
        public int batt_min = 0;

        int music_min = 0;
        int music_sec = 0;
        int media_all_sec = 0;

        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        ProcessStartInfo commands = new ProcessStartInfo();
        IEnumerable<MMDevice> speakDevices;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        public const int KEYEVENTF_EXTENTEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 0;
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;

        // РЕГИСТРАЦИЯ ХОТКЕЯ ----------------------------------------------------------------------
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
        [In] IntPtr hWnd,
        [In] int id,
        [In] uint fsModifiers,
        [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID_Panel = 9000;
        private const int HOTKEY_ID_Bar = 9001;


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);

            // Хоткей на открытие панели
            const uint SPASE = 0x20;
            const uint MOD_CTRL = 0x0002;
            RegisterHotKey(helper.Handle, HOTKEY_ID_Panel, MOD_CTRL, SPASE);

            // Хоткей на открытии игрового оверлея
            const uint ALT = 0xA4;
            const uint G = 0x47;
            RegisterHotKey(helper.Handle, HOTKEY_ID_Bar, MOD_CTRL, G);
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _source.RemoveHook(HwndHook);
                _source = null;
                UnregisterHotKey();
            }
            catch (Exception ex)
            {

            }
            base.OnClosed(e);
        }
        /*
        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);

            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, SPASE))
            {
                // handle error
            }
        }
        */
        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID_Panel);
            UnregisterHotKey(helper.Handle, HOTKEY_ID_Bar);
        }

        public bool gameBar_show = false;

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID_Panel:
                            OnHotKeyPressed();
                            handled = true;
                            break;

                        case HOTKEY_ID_Bar:

                            if (!gameBar_show && ModernNotyfi.WindowState == WindowState.Minimized)
                            {
                                gamePanel gamePanel = new gamePanel();
                                gamePanel.Show();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            ModernNotyfi.WindowState = WindowState.Normal;
            ThicknessAnimation Border_MainPanelGrid = new ThicknessAnimation();
            Border_MainPanelGrid.From = new Thickness(355, 0, -666, 0);
            Border_MainPanelGrid.To = new Thickness(0, 0, -312, 0);
            Border_MainPanelGrid.Duration = TimeSpan.FromSeconds(0.1);
            MainPanelGrid.BeginAnimation(Border.MarginProperty, Border_MainPanelGrid);
        }

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

        public class NotifyModel
        {
            public NotifyModel(string mainTextN, string subTextN, ImageSource image)
            {
                MainTextN = mainTextN;
                SubTextN = subTextN;
                Image = image;
            }

            public string MainTextN { get; set; }
            public string SubTextN { get; set; }
            public ImageSource Image { get; set; }
        }

        public ObservableCollection<NotifyModel> Notify { get; set; } = new ObservableCollection<NotifyModel>();

        private async void Search_App_TextChanged(object sender, TextChangedEventArgs e)
        {
            Items.Clear();
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


                if (Search_App.Text.Length == 0)
                {
                    Items.Add(new ItemModel(fileInfo.Name.Replace(".lnk", ""), imageSource, fileInfo.FullName));
                }
                else
                {
                    if (fileInfo.Name.Contains(Search_App.Text))
                    {
                        Items.Add(new ItemModel(fileInfo.Name.Replace(".lnk", ""), imageSource, fileInfo.FullName));
                    }
                }
            }

            if (Search_App.Text == "OpenDebugWindow")
            {
                Search_App.Text = "";
                Other_Page.DebugWindow debugWindow = new Other_Page.DebugWindow();
                debugWindow.Show();
            }
        }

        public void SubmitNotification(string Main, string Sub, string UriIcon) // Уведомление.
        {
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri(UriIcon, UriKind.Relative); bi3.EndInit();
            Notify.Add(new NotifyModel(Main, Sub, bi3));
            SystemSounds.Asterisk.Play();
            new ToastContentBuilder().AddArgument("action", "viewConversation").AddArgument("conversationId", 9813)
                .AddText(Main).AddText(Sub).Show();
        }

        // Инициализация
        public MainWindow()
        {
            // Сохранение настроек после обновления
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            BatteryChar = new ChartValues<int> { };
            try
            {
                if (Properties.Settings.Default.theme == "light")
                {
                    ThemeManager.SetRequestedTheme(this, ElementTheme.Light);   
                }
                else
                {
                    ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);
                }

                commands.CreateNoWindow = false;
                commands.UseShellExecute = false;

                WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
                InitializeComponent();
                this.Background = null;

                // Положение: правый-нижний угол.
                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                if (Properties.Settings.Default.posicion == "rigth")
                {
                    this.Left = desktopWorkingArea.Right - this.Width;
                    this.Top = desktopWorkingArea.Bottom - this.Height;
                }
                else
                {
                    this.Left = desktopWorkingArea.Left;
                    this.Top = desktopWorkingArea.Bottom - this.Height;
                }

                Loaded += (sender, args) =>
                {
                    WPFUI.Appearance.Watcher.Watch(this, WPFUI.Appearance.BackgroundType.Mica, true);
                };

                if (Properties.Settings.Default.theme != "light")
                {
                    this.Resources["Button.Static.Background"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                }
                else
                {
                    this.Resources["Button.Static.Background"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                }

                if (Properties.Settings.Default.Language == "English")
                {
                    EnglishInterfase_Settings();
                }

                DataContext = this;

                // Применение настроек.
                var bc = new BrushConverter();
                Border_Time.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Panel.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                SoundBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Shutdown.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Music.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                MainPanel.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Notify.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                MusicCard.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);

                Border_Time.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
                Border_Panel.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
                SoundBorder.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
                Border_Shutdown.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
                Border_Music.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
                MainPanel.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
                Border_Notify.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
                NWrite.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
                NMWrite.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);

                Border_Time.Background.Opacity = Properties.Settings.Default.opacity_panel;
                Border_Panel.Background.Opacity = Properties.Settings.Default.opacity_panel;
                SoundBorder.Background.Opacity = Properties.Settings.Default.opacity_panel;
                Border_Music.Background.Opacity = Properties.Settings.Default.opacity_panel;
                MainPanel.Background.Opacity = Properties.Settings.Default.opacity_panel;
                Border_Notify.Opacity = Properties.Settings.Default.opacity_panel;
                MusicCard.Opacity = Properties.Settings.Default.opacity_panel;
                NowPlayning.Foreground = SoundText.Foreground;

                if (Properties.Settings.Default.Show_Exit == "False")
                {
                    Border_Shutdown.Height = 87;
                    Border_Shutdown.Margin = new Thickness(141, 361, 0, 0);
                    Border_Shutdown.UpdateLayout();
                }

                if (Properties.Settings.Default.progressbarstyle == "new")
                {
                    ValueVolumeBar.Visibility = Visibility.Visible;
                }
                else
                {
                    SoundSlider.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#7FC5C5C5");
                    SoundSlider.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#FF037BFF");
                    ValueVolumeBar.Visibility = Visibility.Hidden;
                }

                // ---------------------------------------------------------------------
                speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
                if (speakDevices.Count() > 0)
                {
                    AudioDevice.Items.Clear();
                    for (int i = 0; i < speakDevices.Count(); i++)
                    {
                        ListBoxItem itm = new ListBoxItem();
                        itm.Content = speakDevices.ToList()[i];
                        AudioDevice.Items.Add(itm);
                        if (Convert.ToString(speakDevices.ToList()[i]) == Convert.ToString(enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)))
                        {
                            soundDevice = i;
                            AudioDevice.SelectedIndex = i;
                        }
                    }

                    MMDevice mMDevice = speakDevices.ToList()[soundDevice];
                    SoundSlider.Value = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    ValueVolumeBar.Value = SoundSlider.Value;
                    SoundDevice.Content = speakDevices.ToList()[soundDevice];
                }

                InitStart();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Во время загрузки произошла ошибка. Мы не смогли запустить приложение, в следующем окне будет показана детальная информация.", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.MessageBox.Show("Информация об ошибке:\n" + e + "\n\nПриложение будет закрыто, для избежания перегрузки памяти.", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        public async void InitStart()
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


            //Синхронизация уведомлений с Unesell Account
            /*
            string id = Properties.Settings.Default.Unesell_id;
            if (!string.IsNullOrEmpty(id))
            {
                // api + "applogin.php?email=" + "&password="
                string responseString = string.Empty;
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    responseString = webClient.DownloadString("http://localhost/api/" + "appNotify.php?id=" + id);
                }

                if (responseString == "null")
                {
                    return;
                }
                else
                {
                    string text = Convert.ToString(JObject.Parse(responseString).SelectToken("text"));
                    System.Windows.MessageBox.Show(text);
                    try
                    {
                        Task.Run(() =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                
                            });
                        });

                    }
                    catch
                    {

                    }
                }

            }
            */
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

        bool Search_Box = true;

        private void Search_Box_Open(object sender, RoutedEventArgs e)
        {
            Search_App.Text = "";
            if (Search_Box)
            {
                Search_Box = false; 
                Search_App.Visibility = Visibility.Visible; TextApp.Visibility = Visibility.Hidden;
                SearchIcons.Symbol = ModernWpf.Controls.Symbol.Cancel;

            }
            else
            {
                Search_Box = true;
                Search_App.Visibility = Visibility.Hidden; TextApp.Visibility = Visibility.Visible;
                SearchIcons.Symbol = ModernWpf.Controls.Symbol.Find;
            }
        }

        private void App_Start(object sender, SelectionChangedEventArgs e)
        {
            if (Apps_List.SelectedIndex != -1)
            {
                Process.Start(Items.ElementAt(Apps_List.SelectedIndex).Sourse);
                Full_Close_Panel(null, null);
            }
            Apps_List.SelectedIndex = -1;
        }

        // Потеря фокуса
        private void ModernNotyfi_Deactivated(object sender, EventArgs e)
        {
            ModernNotyfi.WindowState = WindowState.Minimized;
        }

        public ChartValues<int> BatteryChar { get; set; }

        public async void ModernNotyfi_Loaded(object sender, RoutedEventArgs e) // ИНИЦИАЛИЗАЦИЯ ПАРАМЕТРОВ
        {
            
            ModernNotyfi.WindowState = WindowState.Minimized;

            MediaManager.OnNewSource += MediaManager_OnNewSource;
            MediaManager.OnRemovedSource += MediaManager_OnRemovedSource;
            MediaManager.OnPlaybackStateChanged += MediaManager_OnPlaybackStateChanged;
            MediaManager.OnSongChanged += MediaManager_OnSongChanged;
            MediaManager.Start();

            MMDevice mMDevice = speakDevices.ToList()[soundDevice];
            await Task.Run(() =>
            {
                // Отправка уведомления.
                if (Properties.Settings.Default.show_start_notify && Properties.Settings.Default.First_Settings != true)
                {
                    if (Properties.Settings.Default.Language == "English")
                    {
                        new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)
                        .AddText("Open the panel with Ctrl + Space \n GameBar from with Ctrl + G")
                        .AddText("This notification can be turned off in the settings.")
                        .AddHeroImage(new Uri("https://unesell.com/assets/img/software/Shortcuts.low.jpg"))
                        .Show();
                    }
                    else
                    {
                        new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)
                        .AddText("Откройте панель с Ctrl + Пробел \nИгровой оверлей с Ctrl + G")
                        .AddText("Это уведомление можно отключить в настройках.")
                        .AddHeroImage(new Uri("https://unesell.com/assets/img/software/Shortcuts.low.jpg"))
                        .Show();
                    }
                }
                // ------------------------------------------------------------------------------
            });


            // ПОЛУ-СЕКУНДНЫЙ ТАЙМЕР
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.IsEnabled = true;
            timer.Tick += async (o, t) =>
            {
                if (stopwatch_work)
                {
                    Stopwatch_mnsec = Stopwatch_mnsec + 1;
                    if (Stopwatch_mnsec == 2)
                    {
                        Stopwatch_mnsec = 0;
                        Stopwatch_sec++;
                    }
                    if (Stopwatch_sec == 60)
                    {
                        Stopwatch_sec = 0;
                        Stopwatch_min++;
                    }
                    if (Stopwatch_min == 60)
                    {
                        Stopwatch_min = 0;
                        Stopwatch_hours++;
                    }

                    string Stopwatch_hours_s = "";
                    string Stopwatch_min_s = "";
                    string Stopwatch_sec_s = "";

                    if (Stopwatch_hours < 10)
                    {
                        Stopwatch_hours_s = "0" + Convert.ToString(Stopwatch_hours);
                    }
                    else
                    {
                        Stopwatch_hours_s = Convert.ToString(Stopwatch_hours);
                    }

                    if (Stopwatch_min < 10)
                    {
                        Stopwatch_min_s = "0" + Convert.ToString(Stopwatch_min);
                    }
                    else
                    {
                        Stopwatch_min_s = Convert.ToString(Stopwatch_min);
                    }

                    if (Stopwatch_sec < 10)
                    {
                        Stopwatch_sec_s = "0" + Convert.ToString(Stopwatch_sec);
                    }
                    else
                    {
                        Stopwatch_sec_s = Convert.ToString(Stopwatch_sec);
                    }

                    Stopwatch.Content = Stopwatch_hours_s + ":" + Stopwatch_min_s + ":" + Stopwatch_sec_s;
                }

                this.Background = null;
                try
                {
                    int value_prog_sound = (music_sec + (music_min * 60)) * 100 / media_all_sec;
                    NowSoundProgress.Value = value_prog_sound;
                }
                catch
                {
                    //Error
                }

                if (Notify.Count > 0)
                {
                    Not_Text.Visibility = Visibility.Hidden;
                    MyDevice.Visibility = Visibility.Hidden;
                }
                else
                {
                    Not_Text.Visibility = Visibility.Visible;
                    MyDevice.Visibility = Visibility.Visible;
                }

                // Время
                if (Properties.Settings.Default.Language == "English")
                {
                    SoundText.Content = "Sound settings";
                }
                else
                {
                    SoundText.Content = "Настройки звука";
                }
                DateTimeText.Text = DateTime.Now.ToString("HH:mm");
                DateTimeText1.Content = DateTime.Now.ToString("HH:mm");
                DateTimeText_sec.Text = ":" + DateTime.Now.ToString("ss");
                DateTimeText_Panel.Text = DateTime.Now.ToString("HH:mm");
                DateTimeText_sec_main.Text = ":" + DateTime.Now.ToString("ss");
                DataDate.Content = DateTime.Today.ToString("d");
                TimeSecBar.Progress = Convert.ToInt16(DateTime.Now.ToString("ss")) * 100 / 60;

                // Положение: правый-нижний угол.
                if (Properties.Settings.Default.posicion == "rigth")
                {
                    this.Left = desktopWorkingArea.Right - this.Width;
                    this.Top = desktopWorkingArea.Bottom - this.Height;
                }
                else
                {
                    this.Left = desktopWorkingArea.Left;
                    this.Top = desktopWorkingArea.Bottom - this.Height;
                }

                // ---------------------------------------------------------------------
                if (speakDevices.Count() > 0)
                {
                    SoundSlider.Value = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    ValueVolumeBar.Value = SoundSlider.Value;
                    if (SoundSlider.Value > 0)
                    {
                        AudioOnOff = 1;
                        Audio_settings_Color.IsChecked = true;
                    }
                    else
                    {
                        AudioOnOff = 0;
                        Audio_settings_Color.IsChecked = false;
                    }
                }

                // Обновление данных плеера. (Более рабочий вариант с таймером)
                try
                {
                    await Task.Run(async () => {
                        MediaManager.OnNewSource += MediaManager_OnNewSource;
                        MediaManager.OnRemovedSource += MediaManager_OnRemovedSource;
                        MediaManager.OnPlaybackStateChanged += MediaManager_OnPlaybackStateChanged;
                        MediaManager.OnSongChanged += MediaManager_OnSongChanged;
                        MediaManager.Start();

                        var gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                        var mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
                        if (mediaProperties.Title.Length > 0)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                NowPlayning.Content = mediaProperties.Title;
                                NowPlayning_Autor.Content = mediaProperties.Artist;
                                try
                                {
                                    MediaSession.Content = gsmtcsm.GetCurrentSession().SourceAppUserModelId.Replace(".exe", "");
                                    if(gsmtcsm.GetCurrentSession().SourceAppUserModelId.Replace(".exe", "").Contains("ZuneMusic"))
                                    {
                                        MediaSession.Content = "Zune Music";
                                    }
                                }
                                catch {
                                    MediaSession.Content = "Нет источника";
                                }
                                

                                if (mediaProperties.Artist == "" || mediaProperties.Artist == null)
                                {
                                    NowPlayning_Autor.Content = "Нет автора";
                                }

                                SoundTimeAll.Content = gsmtcsm.GetCurrentSession().GetTimelineProperties().EndTime.ToString(@"mm\:ss");
                                media_all_sec = Convert.ToInt32(gsmtcsm.GetCurrentSession().GetTimelineProperties().EndTime.ToString(@"ss")) + 60 * Convert.ToInt32(gsmtcsm.GetCurrentSession().GetTimelineProperties().EndTime.ToString(@"mm"));
                            }));
                        }
                    });
                }
                catch
                {
                    NowPlayning.Content = "Нет музыки";
                    NowPlayning_Autor.Content = "Давайте что-нибудь послушаем";
                }

                NowPlayning_MusicCard.Content = NowPlayning.Content;
                NowPlayning_Autor_MusicCard.Content = NowPlayning_Autor.Content;

                //GetMobileInfo(); // MN Connect 
                GetBatteryPercent(); //Батарея

                GetConnectionsPCIsEnabledAsync(); // Радио
            };

            if (Properties.Settings.Default.Language == "English")
            {
                Battery_time.Content = "Estimation...";
            }
            else
            {
                Battery_time.Content = "Вычисление...";
            }
                
            ManagementClass wmi = new ManagementClass("Win32_Battery");
            ManagementObjectCollection allBatteries = wmi.GetInstances();
            DataContext = this;

            await Task.Run(() =>
            {
                foreach (var battery in allBatteries)
                {
                    if (Convert.ToUInt16(battery["BatteryStatus"]) == 1)
                    {
                        batt_status = 1;
                    }
                    else
                    {
                        batt_status = 0;
                    }
                    BatteryChar.Add(Convert.ToInt32(battery["EstimatedChargeRemaining"]));
                }
            });

            // МИНУТНЫЙ ТАЙМЕР
            timer_minute.Interval = new TimeSpan(0, 1, 0);
            timer_minute.IsEnabled = true;
            timer_minute.Tick += (o, t) =>
            {
                GetUpdateBatteryStatus();
            };
            timer_minute.Start();

            if (Properties.Settings.Default.First_Settings == true)
            {
                try
                {
                    welcome welcome = new welcome();
                    welcome.Show();
                    this.Close();
                }
                catch
                {
                    // OOBE Error. Пропуск.
                }
            }

            await Task.Run(() => {
                try
                {
                    if (GetContent("https://unesell.com/api/version/modernnotify/version_dev.txt") != Assembly.GetExecutingAssembly().GetName().Version.ToString() && GetContent("http://version-modernnotify.ml/modernnotify/version_dev.txt") != "Error")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SubmitNotification("Доступно обновление!", "Перейдите в настройки для скачивания.", "icons/update.png");
                        });
                    }
                }
                catch
                {
                    // Error
                }

                return Task.CompletedTask;
            });


            //GetServerInfo();
        }

        public sealed class UserNotification
        {
            public Windows.ApplicationModel.AppInfo AppInfo { get; }
            public DateTimeOffset CreationTime { get; }
            public uint Id { get; }
            public Windows.UI.Notifications.Notification Notification { get; }
        }

        public async void NotificationAsync()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                UserNotificationListener listener = UserNotificationListener.Current;
                UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

                //IReadOnlyList<UserNotification> notifs = await listener.GetNotificationsAsync(NotificationKinds.Toast);
            }
            else
            {
                System.Windows.MessageBox.Show("На этом устройстве прослушивание недоступно.");
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
                        SoundSlider.Value = SoundSlider.Value + 10;
                    }
                    if (command == "volume_down")
                    {
                        SoundSlider.Value = SoundSlider.Value - 10;
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
                    //GetServerInfo();
                }
            }
            catch (Exception ex)
            {
                // Ignore Error Send Server
                //MessageBox.Show("Error Send:\n" + ex);
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
                ManagementClass wmi = new ManagementClass("Win32_Battery");
                ManagementObjectCollection allBatteries = wmi.GetInstances();
                
                int bb = Convert.ToInt16(Convert.ToString(BatteryState.Content).Replace('%', ' '));
                foreach (var battery in allBatteries)
                {
                    bb = Convert.ToInt16(battery["EstimatedChargeRemaining"]);
                }
                int volume = Convert.ToInt16(SoundSlider.Value);
                
                await Task.Run(() => {
                    string url = api + "connect/pc_add_info.php?ID_PC=" + GetMotherBoardID() + "&BATTETY=" + bb + "&M1=" + m1 + "&M2=" + m2 + "&VOLUME=" + volume + "&UnesellID=" + Properties.Settings.Default.Unesell_id + "&SystemInfo=" + Properties.Settings.Default.System + "&SysMemoryTotal=" + Math.Round(SysInfo.SystemMemoryTotal, 2) + "&SysMemoryFree=" + Math.Round(SysInfo.SystemMemoryFree, 2);

                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                        var response = webClient.DownloadString(url);
                        //MessageBox.Show(response, "Response");
                    }



                });

            }
            catch(Exception ex)
            {
                // Ignore Error Send Server
                //System.Windows.MessageBox.Show("Error Send:\n" + ex); Properties.Settings.Default.Unesell_id
            }
        }

        public void GetUpdateBatteryStatus()
        {
            ManagementClass wmi = new ManagementClass("Win32_Battery");
            ManagementObjectCollection allBatteries = wmi.GetInstances();
            foreach (var battery in allBatteries)
            {
                BatteryChar.Add(Convert.ToInt16(Convert.ToDouble(battery["EstimatedChargeRemaining"])));

                if (Convert.ToUInt16(battery["BatteryStatus"]) == 1 || batt_status == 0) //Разряжается
                {
                    batt_min++;

                    if (Properties.Settings.Default.Language == "English")
                    {
                        Batt_label_time.Content = "Battery life:";
                    }
                    else
                    {
                        Batt_label_time.Content = "Время работы:";
                    }
                    if (batt_min >= 1)
                    {
                        int bvar1 = batt_start - Convert.ToInt16(battery["EstimatedChargeRemaining"]);
                        int min = 0;
                        if (bvar1 != 0) { min = Convert.ToInt16(battery["EstimatedChargeRemaining"]) * batt_min / bvar1; } else { min = Convert.ToInt16(battery["EstimatedChargeRemaining"]) * batt_min; }
                        int chas = 0;
                        if (min > 60)
                        {
                            chas = min / 60;
                            min = min % 60;
                        }
                        Battery_time.Content = chas + "ч. " + min + "мин. ";
                    }
                    else
                    {
                        if (Properties.Settings.Default.Language == "English")
                        {
                            Battery_time.Content = "Estimation...";
                        }
                        else
                        {
                            Battery_time.Content = "Вычисление...";
                        }
                    }

                    if (Convert.ToInt32(battery["EstimatedChargeRemaining"]) == 25)
                    {
                        SubmitNotification("Батарея почти разряжена.", "Осталось " + Convert.ToInt32(battery["EstimatedChargeRemaining"]) + "%. Подключите зарядное устройство.", "icons/battery_Low.png");
                    }
                }
                else
                {
                    batt_min++;
                    Batt_label_time.Content = "До полного заряда:";
                    if (batt_min >= 1)
                    {
                        int bvar1 = Convert.ToInt16(battery["EstimatedChargeRemaining"]) - batt_start;
                        int min = 0;
                        if (bvar1 != 0) { min = 100 * batt_min / bvar1; } else { min = Convert.ToInt16(battery["EstimatedChargeRemaining"]) * batt_min; }
                        int chas = 0;
                        if (min > 60)
                        {
                            chas = min / 60;
                            min = min % 60;
                        }
                        Battery_time.Content = chas + "ч. " + min + "мин. ";
                    }
                    else
                    {
                        if (Properties.Settings.Default.Language == "English")
                        {
                            Battery_time.Content = "Estimation...";
                        }
                        else
                        {
                            Battery_time.Content = "Вычисление...";
                        }
                    }
                    if (Convert.ToInt32(battery["EstimatedChargeRemaining"]) == 80)
                    {
                        SubmitNotification("Можно отключить ЗУ.", "Устройство заряжено на " + Convert.ToInt32(battery["EstimatedChargeRemaining"]) + "%.", "icons/battery_full.png");
                    }
                }
            }
        }

        public void GetBatteryPercent()
        {
            ManagementClass wmi = new ManagementClass("Win32_Battery");
            ManagementObjectCollection allBatteries = wmi.GetInstances();

            foreach (var battery in allBatteries)
            {
                BatteryState.Content = Convert.ToDouble(battery["EstimatedChargeRemaining"]) + "%";
                Voltage_battery.Content = Math.Round(Convert.ToDouble(battery["DesignVoltage"]) / 1000, 1) + " kW";
                Battery_ProgressBar.Value = Convert.ToDouble(battery["EstimatedChargeRemaining"]);
                if (Convert.ToUInt16(battery["BatteryStatus"]) == 1)
                {

                    if (Properties.Settings.Default.Language == "English")
                    {
                        Battery_Status.Content = "Discharges";
                    }
                    else
                    {
                        Battery_Status.Content = "Разряжается";
                    }
                    
                    if (batt_status == 1)
                    {
                        batt_min = 0;
                        batt_start = Convert.ToInt16(battery["EstimatedChargeRemaining"]);
                        batt_status = 0;
                        if (Properties.Settings.Default.Language == "English")
                        {
                            Battery_time.Content = "Estimation...";
                        }
                        else
                        {
                            Battery_time.Content = "Вычисление...";
                        }
                    }
                    var bc = new BrushConverter();
                    Battery_ProgressBar.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#FF0078D7");
                    //Battery
                    BitmapImage bi3 = new BitmapImage(); bi3.BeginInit();
                    if (Convert.ToDouble(battery["EstimatedChargeRemaining"]) >= 80)
                    {
                        Battery.Glyph = WPFUI.Common.Icon.Battery1024;
                    }
                    else if (Convert.ToDouble(battery["EstimatedChargeRemaining"]) < 80 && Convert.ToDouble(battery["EstimatedChargeRemaining"])  > 40)
                    {
                        Battery.Glyph = WPFUI.Common.Icon.Battery524;
                    }
                    else
                    {
                        Battery.Glyph = WPFUI.Common.Icon.Battery224;
                    }
                }
                else
                {
                    if (Properties.Settings.Default.Language == "English")
                    {
                        Battery_Status.Content = "Is charging";
                    }
                    else
                    {
                        Battery_Status.Content = "Заряжается";
                    }
                    var bc = new BrushConverter();
                    Battery_ProgressBar.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#FF70BD13");
                    Battery.Glyph = WPFUI.Common.Icon.BatteryCharge24;
                    batt_min = 0;
                    if (batt_status == 0)
                    {
                        batt_status = 1;
                        batt_start = Convert.ToInt16(battery["EstimatedChargeRemaining"]);
                        batt_min = 0;
                        if (Properties.Settings.Default.Language == "English")
                        {
                            Battery_time.Content = "Estimation...";
                        }
                        else
                        {
                            Battery_time.Content = "Вычисление...";
                        }
                    }
                }
            }
        }

        private void On_OFF_Audio(object sender, RoutedEventArgs e)
        {
            MMDevice mMDevice = speakDevices.ToList()[soundDevice];
            if (AudioOnOff == 1) {
                AudioOnOff = 0;
                Audio_settings_Color.IsChecked = false;
                TempAudioOnOff = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 0 / 100.0f;
                SoundSlider.Value = 0;
                ValueVolumeBar.Value = 0;
            }
            else {
                AudioOnOff = 1;
                Audio_settings_Color.IsChecked = true;
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = TempAudioOnOff / 100.0f;
                SoundSlider.Value = TempAudioOnOff;
                ValueVolumeBar.Value = TempAudioOnOff;
            }
        }

        // Открытие настроек
        private void Settings_Open_Click(object sender, RoutedEventArgs e)
        {
            settings settings = new settings();
            settings.Show();
            timer_minute.Stop();
            timer.Stop();
            Close();
        }

        public bool modal_show = false;
        public int close_time = 10;

        private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (speakDevices.Count() > 0)
            {
                MMDevice mMDevice = speakDevices.ToList()[soundDevice];
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = Convert.ToInt32(SoundSlider.Value) / 100.0f;
                ValueVolumeBar.Value = SoundSlider.Value;
                if (Properties.Settings.Default.Language == "English")
                {
                    SoundText.Content = "Volume: " + (Convert.ToInt32(SoundSlider.Value) / 100.0f * 100) + "%";
                }
                else
                {
                    SoundText.Content = "Громкость: " + (Convert.ToInt32(SoundSlider.Value) / 100.0f * 100) + "%";
                }

                if (!modal_show && ModernNotyfi.WindowState == WindowState.Minimized)
                {
                    modal_info modal_Info = new modal_info("volume");
                    modal_Info.Show();
                }
                close_time = 10;
            }
        }

        private void SoundDevice_Click(object sender, RoutedEventArgs e)
        {
            if (SoundDeviceOpen == 0)
            {
                SoundDeviceOpen = 1; not_up = 0;
                DoubleAnimation SoundDeviceAnimation = new DoubleAnimation();
                SoundDeviceAnimation.From = SoundBorder.ActualHeight;
                SoundDeviceAnimation.To = 150;
                SoundDeviceAnimation.Duration = TimeSpan.FromSeconds(0.5);
                SoundBorder.BeginAnimation(Border.HeightProperty, SoundDeviceAnimation);

                DoubleAnimation NotifyAnimation = new DoubleAnimation();
                NotifyAnimation.From = Border_Notify.ActualHeight;
                NotifyAnimation.To = 52;
                NotifyAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Notify.BeginAnimation(Border.HeightProperty, NotifyAnimation);

                Open_N_Image.Glyph = WPFUI.Common.Icon.CaretUp24;

                ThicknessAnimation Border_NotifyAnimation = new ThicknessAnimation();
                Border_NotifyAnimation.From = new Thickness(10, 0, 0, 274);
                Border_NotifyAnimation.To = new Thickness(10, 0, 0, 358);
                Border_NotifyAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Notify.BeginAnimation(Border.MarginProperty, Border_NotifyAnimation);

                ThicknessAnimation Border_MusicAnimation = new ThicknessAnimation();
                Border_MusicAnimation.From = new Thickness(10, 230, 0, 0);
                Border_MusicAnimation.To = new Thickness(10, 150, 0, 0);
                Border_MusicAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Music.BeginAnimation(Border.MarginProperty, Border_MusicAnimation);
                //Margin="10,150,0,0"
            }
            else
            {
                SoundDeviceOpen = 0; not_up = 0;
                DoubleAnimation SoundDeviceAnimation = new DoubleAnimation();
                SoundDeviceAnimation.From = SoundBorder.ActualHeight;
                SoundDeviceAnimation.To = 70;
                SoundDeviceAnimation.Duration = TimeSpan.FromSeconds(0.5);
                SoundBorder.BeginAnimation(Border.HeightProperty, SoundDeviceAnimation);

                DoubleAnimation NotifyAnimation = new DoubleAnimation();
                NotifyAnimation.From = Border_Notify.ActualHeight;
                NotifyAnimation.To = 52;
                NotifyAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Notify.BeginAnimation(Border.HeightProperty, NotifyAnimation);

                Open_N_Image.Glyph = WPFUI.Common.Icon.CaretUp24;

                ThicknessAnimation Border_NotifyAnimation = new ThicknessAnimation();
                Border_NotifyAnimation.From = new Thickness(10, 0, 0, 358);
                Border_NotifyAnimation.To = new Thickness(10, 0, 0, 274);
                Border_NotifyAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Notify.BeginAnimation(Border.MarginProperty, Border_NotifyAnimation);

                ThicknessAnimation Border_MusicAnimation = new ThicknessAnimation();
                Border_MusicAnimation.From = new Thickness(10, 150, 0, 0);
                Border_MusicAnimation.To = new Thickness(10, 230, 0, 0);
                Border_MusicAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Music.BeginAnimation(Border.MarginProperty, Border_MusicAnimation);
                //10,230,0,0
            }

        }

        private void AudioDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            soundDevice = AudioDevice.SelectedIndex;

            var enumerator = new MMDeviceEnumerator();
            IEnumerable<MMDevice> speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
            if (speakDevices.Count() > 0)
            {
                MMDevice mMDevice = speakDevices.ToList()[soundDevice];
                SoundSlider.Value = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                ValueVolumeBar.Value = SoundSlider.Value;
                SoundDevice.Content = speakDevices.ToList()[soundDevice];
            }
        }

        // КНОПКИ
        public void Open_Full_Panel()
        {
            MainPanel.Visibility = Visibility.Visible;
            MusicCard.Visibility = Visibility.Hidden;

            //MainPanelGrid
            ThicknessAnimation Border_MainPanelGrid = new ThicknessAnimation();
            Border_MainPanelGrid.From = new Thickness(0, 0, -312, 0);
            Border_MainPanelGrid.To = new Thickness(-360, 0, 0, 0);
            Border_MainPanelGrid.Duration = TimeSpan.FromSeconds(0.1);
            MainPanelGrid.BeginAnimation(Border.MarginProperty, Border_MainPanelGrid);
        }
        int not_up = 0;
        private void Nitify_Open_Panel(object sender, RoutedEventArgs e)
        {
            DoubleAnimation NotifyAnimation = new DoubleAnimation();
            NotifyAnimation.From = Border_Notify.ActualHeight;
            if (not_up == 0)
            {
                
                Open_N_Image.Glyph = WPFUI.Common.Icon.CaretDown24;
                NotifyAnimation.To = 215;
                not_up = 1;
            }
            else
            {
                Open_N_Image.Glyph = WPFUI.Common.Icon.CaretUp24;
                NotifyAnimation.To = 52;
                not_up = 0;
            }
            NotifyAnimation.Duration = TimeSpan.FromSeconds(0.5);
            Border_Notify.BeginAnimation(Border.HeightProperty, NotifyAnimation);

            DoubleAnimation SoundDeviceAnimation = new DoubleAnimation();
            SoundDeviceAnimation.From = SoundBorder.ActualHeight;
            SoundDeviceAnimation.To = 70;
            SoundDeviceAnimation.Duration = TimeSpan.FromSeconds(0.5);
            SoundBorder.BeginAnimation(Border.HeightProperty, SoundDeviceAnimation);

            if (Border_Notify.Margin == new Thickness(10, 0, 0, 358))
            {
                ThicknessAnimation Border_NotifyAnimation = new ThicknessAnimation();
                Border_NotifyAnimation.From = new Thickness(10, 0, 0, 358);
                Border_NotifyAnimation.To = new Thickness(10, 0, 0, 274);
                Border_NotifyAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Notify.BeginAnimation(Border.MarginProperty, Border_NotifyAnimation);
            }

            if (Border_Music.Margin == new Thickness(10, 150, 0, 0))
            {
                ThicknessAnimation Border_MusicAnimation = new ThicknessAnimation();
                Border_MusicAnimation.From = new Thickness(10, 150, 0, 0);
                Border_MusicAnimation.To = new Thickness(10, 230, 0, 0);
                Border_MusicAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Music.BeginAnimation(Border.MarginProperty, Border_MusicAnimation);
            }
        }

        private void Full_Close_Panel(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation Border_MainPanelGrid = new ThicknessAnimation();
            Border_MainPanelGrid.From = new Thickness(-360, 0, 0, 0); 
            Border_MainPanelGrid.To = new Thickness(0, 0, -312, 0);
            Border_MainPanelGrid.Duration = TimeSpan.FromSeconds(0.1);
            MainPanelGrid.BeginAnimation(Border.MarginProperty, Border_MainPanelGrid);
        }

        //int onen_about_sound = 0;
        private void Music_About_Click(object sender, RoutedEventArgs e)
        {
            /*
            if(onen_about_sound == 0)
            {
                ThicknessAnimation Border_MainPanelGrid = new ThicknessAnimation();
                Border_MainPanelGrid.From = new Thickness(0, 0, 0, 0);
                Border_MainPanelGrid.To = new Thickness(-250, 0, 0, 0);
                Border_MainPanelGrid.Duration = TimeSpan.FromSeconds(0.1);
                SoundGridContent.BeginAnimation(Grid.MarginProperty, Border_MainPanelGrid);
                onen_about_sound = 1;
            }
            else
            {
                ThicknessAnimation Border_MainPanelGrid = new ThicknessAnimation();
                Border_MainPanelGrid.From = new Thickness(-250, 0, 0, 0);
                Border_MainPanelGrid.To = new Thickness(0, 0, 0, 0);
                Border_MainPanelGrid.Duration = TimeSpan.FromSeconds(0.1);
                SoundGridContent.BeginAnimation(Grid.MarginProperty, Border_MainPanelGrid);
                onen_about_sound = 0;
            }
            */
            Open_Full_Panel();
            MainPanel.Visibility = Visibility.Hidden;
            MusicCard.Visibility = Visibility.Visible;
        }

        private void MusicCard_Close_Click(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation Border_MainPanelGrid = new ThicknessAnimation();
            Border_MainPanelGrid.From = new Thickness(-360, 0, 0, 0);
            Border_MainPanelGrid.To = new Thickness(0, 0, -312, 0);
            Border_MainPanelGrid.Duration = TimeSpan.FromSeconds(0.1);
            MainPanelGrid.BeginAnimation(Border.MarginProperty, Border_MainPanelGrid);
        }

        private void Battery_Open_Panel(object sender, RoutedEventArgs e)
        {
            Open_Full_Panel();
            Full_Panel_Tab.SelectedIndex = 0;
        }

        private void Browser_Open_Panel(object sender, RoutedEventArgs e)
        {
            Open_Full_Panel();
            Full_Panel_Tab.SelectedIndex = 1;
        }

        private void Clock_Open_Panel(object sender, RoutedEventArgs e)
        {
            Open_Full_Panel();
            Full_Panel_Tab.SelectedIndex = 2;
        }

        private void Wifi_settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // If Windows 10-11
                Process.Start("ms-settings:network"); //ms-settings:
            }
            catch
            {
                //if Windows 7 - 8
                commands.FileName = "control.exe";
                commands.Arguments = "/name Microsoft.NetworkandSharingCenter";
                Process.Start(commands);
            }
        }
        private void Command_shutdown(object sender, RoutedEventArgs e)
        {
            //Выключение компьютера
            commands.FileName = "cmd.exe";
            commands.Arguments = "/c shutdown -s -f -t 00";
            Process.Start(commands);
        }
        private void Command_Restart(object sender, RoutedEventArgs e)
        {
            //Перезагрузка компьютера
            commands.FileName = "cmd.exe";
            commands.Arguments = "/c shutdown -r -f -t 00";
            Process.Start(commands);
        }
        private void Command_Exit(object sender, RoutedEventArgs e)
        {
            //Выход с приложения
            Application.Current.Shutdown();
        }

        // ПРОЧЕЕ
        public void Shutdown_Open_Panel(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Disign_Shutdown == "old")
            {
                Border_Shutdown.Visibility = Visibility.Visible;
            }
            else
            {
                shutdown shutdown = new shutdown();
                shutdown.Show();
            }
        }

        public void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border_Shutdown.Visibility = Visibility.Hidden;
        }

        // РАБОТА С МУЗЫКОЙ
        public static void MediaManager_OnNewSource(MediaManager.MediaSession session)
        {
            ChencheMusic("Источник: " + session.ControlSession.SourceAppUserModelId);
            if (session.ControlSession.SourceAppUserModelId == "Spotify.exe")
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/Media/spotify.png", UriKind.Relative); bi3.EndInit();
                    my.MusicPlayer.Source = bi3;
                }));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/Media/notes.png", UriKind.Relative); bi3.EndInit();
                    my.MusicPlayer.Source = bi3;
                }));
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.SoundTimeNow.Content = Convert.ToString(session.ControlSession.GetTimelineProperties().Position);

                my.SoundTimeAll.Content = session.ControlSession.GetTimelineProperties().EndTime.ToString(@"mm\:ss");

                my.media_all_sec = Convert.ToInt32(session.ControlSession.GetTimelineProperties().EndTime.ToString(@"ss")) + 60 * Convert.ToInt32(session.ControlSession.GetTimelineProperties().EndTime.ToString(@"mm"));

                var timers = new System.Windows.Threading.DispatcherTimer();
                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                timers.Interval = new TimeSpan(0, 0, 1);
                timers.IsEnabled = true;
                timers.Tick += (o, t) =>
                {
                    my.SoundTimeNow.Content = session.ControlSession.GetTimelineProperties().Position.ToString(@"mm\:ss");
                    my.music_min = session.ControlSession.GetTimelineProperties().Position.Minutes;
                    my.music_sec = session.ControlSession.GetTimelineProperties().Position.Seconds;
                };
                timers.Start();
            }));
        }

        public static void MediaManager_OnRemovedSource(MediaManager.MediaSession session)
        {
            ChencheMusic("Очередь пуста");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.NowPlayning.Content = "Очередь пуста";
            }));


            var timerChechMusic = new System.Windows.Threading.DispatcherTimer();
            timerChechMusic.Interval = new TimeSpan(0, 0, 1);
            timerChechMusic.IsEnabled = true;
            timerChechMusic.Tick += (o, t) =>
            {
                _ = NowPlay();
            };

        }

        public static void MediaManager_OnSongChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
        {
            MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ChencheMusic($"{args.Title} {(String.IsNullOrEmpty(args.Artist) ? "" : $"- {args.Artist}")}");
                    
                    my.NowPlayning.Content = $"{args.Title}";
                    my.NowPlayning_Autor.Content = $"{(String.IsNullOrEmpty(args.Artist) ? "" : $"{args.Artist}")}";
                    my.PlayIcon.Glyph = WPFUI.Common.Icon.Pause12;
                    my.PlayIcon_MusicCard.Glyph = WPFUI.Common.Icon.Pause12;
                    

                    my.SoundTimeAll.Content = sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"mm\:ss");
                    my.media_all_sec = Convert.ToInt32(sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"ss")) + 60 * Convert.ToInt32(sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"mm"));
                    //my.GetServerInfo();
                }));
            }
            catch
            {
                my.NowPlayning.Content = "Управление музыкой";
            }

        }

        public static void MediaManager_OnPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
        {
            ChencheMusic($"Состояние изменено");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (args.PlaybackStatus.ToString() == "Paused") //Paused   || Playing
                {
                    my.PlayIcon.Glyph = WPFUI.Common.Icon.Play12;
                    my.PlayIcon_MusicCard.Glyph = WPFUI.Common.Icon.Play12;
                }
                else
                {
                    my.PlayIcon.Glyph = WPFUI.Common.Icon.Pause12;
                    my.PlayIcon_MusicCard.Glyph = WPFUI.Common.Icon.Pause12;
                }

                my.SoundTimeAll.Content = sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"mm\:ss");
                my.media_all_sec = Convert.ToInt32(sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"ss")) + 60 * Convert.ToInt32(sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"mm"));

            }));
        }

        private static void ChencheMusic(string Songs)
        {
            _ = Songs;
        }

        public static async Task NowPlay()
        {
            var gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
            var mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
            if (mediaProperties.Title.Length > 0)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    my.NowPlayning.Content = mediaProperties.Title;
                }));
            }
        }

        public static async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        public static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session) =>
            await session.TryGetMediaPropertiesAsync();

        private void Play_Pause_Click(object sender, RoutedEventArgs e)
        {
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            MediaManager.OnNewSource += MediaManager_OnNewSource;
            MediaManager.OnRemovedSource += MediaManager_OnRemovedSource;
            MediaManager.OnPlaybackStateChanged += MediaManager_OnPlaybackStateChanged;
            MediaManager.OnSongChanged += MediaManager_OnSongChanged;
            MediaManager.Start();
        }

        private void Music_Right_Click(object sender, RoutedEventArgs e)
        {
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            MediaManager.OnNewSource += MediaManager_OnNewSource;
            MediaManager.OnRemovedSource += MediaManager_OnRemovedSource;
            MediaManager.OnPlaybackStateChanged += MediaManager_OnPlaybackStateChanged;
            MediaManager.OnSongChanged += MediaManager_OnSongChanged;
            MediaManager.Start();
        }

        private void Music_Left_Click(object sender, RoutedEventArgs e)
        {
            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            MediaManager.OnNewSource += MediaManager_OnNewSource;
            MediaManager.OnRemovedSource += MediaManager_OnRemovedSource;
            MediaManager.OnPlaybackStateChanged += MediaManager_OnPlaybackStateChanged;
            MediaManager.OnSongChanged += MediaManager_OnSongChanged;
            MediaManager.Start();
        }

        private void Battery_Settings_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("ms-settings:batterysaver");
        }

        private void Clear_All_Notify(object sender, RoutedEventArgs e)
        {
            Notify.Clear();
        }

        public void EnglishInterfase_Settings()
        {
            Not_Text.Content = "No notifications";
            BackText1.Content = "Notifications";
            NowPlayning.Content = "Now playing";
            NowPlayning_Autor.Content = "Queue is empty.";
            Clear_All_N.Content = "Clear all";
            MyDevice.Content = "My devices";
            BackText.Content = "Back";
            text_bbss.Content = "Battery charge";
            Batt_label_time.Content = "Battery life";
            waltagetext.Content = "Voltage";
            textbbgg.Content = "To extend battery life, \nMake a charge between 20% and 80%.";
            Battery_Settings.Content = "Power settings";
            TextApp.Content = "Applications";
            DataTimeText.Content = "Time and date";
            Audio_Settinds_text.Content = "Sound";
            SoundText.Content = "Sound settings";
            TextShutdown.Content = "Shutdown";
            TextRestartPC.Content = "Reboot PC";
            EXITtextPROGRAM.Content = "Exit program";
            TextStopwatch.Content = "Stopwatch";
        }

        public string GetContent(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Proxy = null;
                request.Method = "GET";
                request.Timeout = 360000;
                request.ContentType = "application/x-www-form-urlencoded";

                using (WebResponse response = request.GetResponse())
                {
                    Stream requestStream = response.GetResponseStream();

                    if (requestStream == null)
                    {
                        return null;
                    }

                    return new StreamReader(requestStream).ReadToEnd();
                }
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        public async void GetConnectionsPCIsEnabledAsync()
        {
            try
            {
                var radios = await Radio.GetRadiosAsync();
                var bluetoothRadio = radios.FirstOrDefault(radio => radio.Kind == RadioKind.Bluetooth);
                if (bluetoothRadio.State == RadioState.On)
                {
                    Bluetooth_Settings_Color.IsChecked = true;
                }
                else
                {
                    Bluetooth_Settings_Color.IsChecked = false;
                }
                var result = await WiFiAdapter.RequestAccessAsync();
                if (result == WiFiAccessStatus.Allowed)
                {
                    foreach (var radio in radios)
                    {
                        if (radio.Kind == RadioKind.WiFi && radio.State == RadioState.On)
                        {
                            WiFi_Color.IsChecked = true;
                            break;
                        }
                        else
                        {
                            WiFi_Color.IsChecked = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Wifi_settings.IsEnabled = false;
                Bluetooth_settings.IsEnabled = false;
            }
        }



        private async void Bluetooth_settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await Radio.RequestAccessAsync();
                if (result == RadioAccessStatus.Allowed)
                {
                    var bluetooth = (await Radio.GetRadiosAsync()).FirstOrDefault(radio => radio.Kind == RadioKind.Bluetooth);
                    if (bluetooth != null && bluetooth.State != RadioState.On)
                    {
                        await bluetooth.SetStateAsync(RadioState.On);
                        Bluetooth_Settings_Color.IsChecked = true;
                    }
                    else
                    {
                        await bluetooth.SetStateAsync(RadioState.Off);
                        Bluetooth_Settings_Color.IsChecked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Управление сетями на этой ОС недоступно.");
            }
        }

        // TESTING AND EXPEREMENT
        private async void WIFI_settings_ON_OFF(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await WiFiAdapter.RequestAccessAsync();
                if (result == WiFiAccessStatus.Allowed)
                {
                    var radios = await Radio.GetRadiosAsync();

                    foreach (var radio in radios)
                    {
                        if (radio.Kind == RadioKind.WiFi && radio.State != RadioState.On)
                        {
                            await radio.SetStateAsync(RadioState.On);
                            WiFi_Color.IsChecked = true;
                            break;
                        }
                        else
                        {
                            await radio.SetStateAsync(RadioState.Off);
                            WiFi_Color.IsChecked = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Управление сетями на этой ОС недоступно.");
            }
        }

        private void OpenMyDevice(object sender, RoutedEventArgs e)
        {
            MyDevice windowdevice = new MyDevice();
            windowdevice.Show();
        }

        private void MNConnect_Open(object sender, RoutedEventArgs e)
        {
            MyDevice mnconnect = new MyDevice();
            mnconnect.Show();
        }

        public bool stopwatch_work = false;
        int Stopwatch_mnsec = 0;
        int Stopwatch_sec = 0;
        int Stopwatch_min = 0;
        int Stopwatch_hours = 0;

        private void StartStopWatch_Click(object sender, RoutedEventArgs e)
        {
            if (stopwatch_work)
            {
                stopwatch_work = false;
                playStopwatchIcon.Glyph = WPFUI.Common.Icon.Play12;
            }
            else
            {
                stopwatch_work = true;
                playStopwatchIcon.Glyph = WPFUI.Common.Icon.Pause12;
            }
        }

        private void resetStopWatch_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch.Content = "00:00:00.000";
            stopwatch_work = false;
            Stopwatch_mnsec = 0;
            Stopwatch_sec = 0;
            Stopwatch_min = 0;
            Stopwatch_hours = 0;
            playStopwatchIcon.Glyph = WPFUI.Common.Icon.Play12;
        }
    }




    public static class MediaManager
    {
        public delegate void MediaSessionDelegate(MediaSession session);
        public static event MediaSessionDelegate OnNewSource;
        public static event MediaSessionDelegate OnRemovedSource;
        public static event TypedEventHandler<MediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo> OnPlaybackStateChanged;
        public static event TypedEventHandler<MediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties> OnSongChanged;
        public static Dictionary<string, MediaSession> CurrentMediaSessions = new Dictionary<string, MediaSession>();


        private static bool IsStarted;
        public static void Start()
        {
            if (!IsStarted)
            {
                var sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult();
                SessionsChanged(sessionManager);
                sessionManager.SessionsChanged += SessionsChanged;
                IsStarted = true;
            }
        }

        private static void SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args = null)
        {
            var sessionList = sender.GetSessions();

            foreach (var session in sessionList)
            {
                if (!CurrentMediaSessions.ContainsKey(session.SourceAppUserModelId))
                {
                    MediaSession mediaSession = new MediaSession(session);
                    CurrentMediaSessions[session.SourceAppUserModelId] = mediaSession;
                    OnNewSource?.Invoke(mediaSession);
                    mediaSession.OnSongChange(session);
                }
            }
        }


        private static void RemoveSession(MediaSession mediaSession)
        {
            CurrentMediaSessions.Remove(mediaSession.ControlSession.SourceAppUserModelId);
            OnRemovedSource?.Invoke(mediaSession);
        }

        public class MediaSession
        {
            public GlobalSystemMediaTransportControlsSession ControlSession;
            public string LastSong;

            public MediaSession(GlobalSystemMediaTransportControlsSession ctrlSession)
            {
                ControlSession = ctrlSession;
                ControlSession.MediaPropertiesChanged += OnSongChange;
                ControlSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
            }


            private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession session, PlaybackInfoChangedEventArgs args = null)
            {
                var props = session.GetPlaybackInfo();
                if (props.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed)
                {
                    session.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                    session.MediaPropertiesChanged -= OnSongChange;
                    RemoveSession(this);
                }
                else
                {
                    OnPlaybackStateChanged?.Invoke(this, props);
                }
            }


            internal async void OnSongChange(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args = null)
            {
                try
                {
                    var props = await session.TryGetMediaPropertiesAsync();
                    string song = $"{props.Title} | {props.Artist}";

                    //This is needed because for some reason this method is invoked twice every song change
                    if (LastSong != song && !(String.IsNullOrWhiteSpace(props.Title) && String.IsNullOrWhiteSpace(props.Artist)))
                    {
                        LastSong = song;
                        OnSongChanged?.Invoke(this, props);
                    }
                }
                catch
                {

                }

            }
        }

    }
}

