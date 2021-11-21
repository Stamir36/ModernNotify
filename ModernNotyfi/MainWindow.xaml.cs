using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using NAudio;
using NAudio.CoreAudioApi;
using System.Reflection;
using System.Diagnostics;
using ModernWpf;
using LiveCharts;
using LiveCharts.Wpf;
using Windows.Media;
using Windows.Media.Control;
using System.Media;
using System.Runtime.InteropServices;
using System.Management;
using LiveCharts.Dtos;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Interop;
using Windows.UI.Notifications.Management;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Timers;

namespace ModernNotyfi
{
    public partial class MainWindow : Window
    {
        // БАЗОВЫЕ НАСТРОЙКИ И ПАРАМЕТРЫ ---------------------------------------------------------
        public int soundDevice = 1; //Активное устройство воспроизведенеия
        public int SoundDeviceOpen = 0;
        public int AudioOnOff = 0; //Вкл \ Выкл звук.
        public int TempAudioOnOff = 0;

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
        private const int HOTKEY_ID = 9000;


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            const uint SPASE = 0x20;
            const uint MOD_CTRL = 0x0002;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, SPASE))
            {
                // handle error
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
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

        private void Search_App_TextChanged(object sender, TextChangedEventArgs e)
        {
            Items.Clear();
            string name_user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var appUser = Directory.GetFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", "*.lnk");
            var appWindows = Directory.GetFiles(name_user + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs", "*.lnk");
            var files = new string[appUser.Length + appWindows.Length];
            appUser.CopyTo(files, 0);
            appWindows.CopyTo(files, appUser.Length);
            files = SortFilesPath(files);
            foreach (var file in files)
            {
                ImageSource imageSource = null;

                FileInfo fileInfo = new FileInfo(file);
                Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileInfo.FullName);

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

            if (Properties.Settings.Default.First_Settings == true)
            {
                try
                {
                    welcome welcome = new welcome();
                    welcome.Show();
                    this.Hide();
                }
                catch
                {
                    // OOBE Error
                }
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

                InitializeComponent();

                if (Properties.Settings.Default.theme != "light")
                {
                    BitmapImage bi3 = new BitmapImage(); bi3.BeginInit();
                    
                    bi3.UriSource = new Uri("icons/settings_Light.png", UriKind.Relative); bi3.EndInit();
                    Settings_Icon.Source = bi3;

                    BitmapImage bi2 = new BitmapImage(); bi2.BeginInit();
                    bi2.UriSource = new Uri("icons/shutdown_Light.png", UriKind.Relative); bi2.EndInit();
                    shutdownIcon.Source = bi2;

                    BitmapImage bi1 = new BitmapImage(); bi1.BeginInit();
                    bi1.UriSource = new Uri("icons/App_Black_Light.png", UriKind.Relative); bi1.EndInit();
                    Battery1.Source = bi1;

                    BatteryLight.Visibility = Visibility.Visible;
                    SearchIcons.Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    SearchIcons.Foreground = System.Windows.Media.Brushes.Black;
                }

                if (Properties.Settings.Default.Language == "English")
                {
                    EnglishInterfase_Settings();
                }

                DataContext = this;

                string name_user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var appUser = Directory.GetFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", "*.lnk");
                var appWindows = Directory.GetFiles(name_user + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs", "*.lnk");
                var files = new string[appUser.Length + appWindows.Length];
                appUser.CopyTo(files, 0);
                appWindows.CopyTo(files, appUser.Length);

                files = SortFilesPath(files);

                foreach (var file in files)
                {
                    ImageSource imageSource = null;

                    FileInfo fileInfo = new FileInfo(file);
                    Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileInfo.FullName);

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

                // Применение настроек.
                var bc = new BrushConverter();
                Border_Time.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Panel.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                SoundBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Shutdown.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Music.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                MainPanel.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Notify.Background = (System.Windows.Media.Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);

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
                NowPlayning.Foreground = SoundText.Foreground;

                if (Properties.Settings.Default.Show_Exit == "False")
                {
                    Border_Shutdown.Height = 87;
                    Border_Shutdown.Margin = new Thickness(141, 361, 0, 0);
                    Border_Shutdown.UpdateLayout();
                }

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
                
                // ---------------------------------------------------------------------
                speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
                if (speakDevices.Count() > 0)
                {
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
                    SoundDevice.Content = speakDevices.ToList()[soundDevice];
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Во время загрузки произошла ошибка. Мы не смогли запустить приложение, в следующем окне будет показана детальная информация.", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show("Информация об ошибке:\n" + e + "\n\nПриложение будет закрыто, для избежания перегрузки памяти.", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private string[] SortFilesPath(string[] files)
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
        }

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

        private void ModernNotyfi_Loaded(object sender, RoutedEventArgs e)
        {
            // Отправка уведомления.
            if (Properties.Settings.Default.show_start_notify && Properties.Settings.Default.First_Settings != true)
            {
                if (Properties.Settings.Default.Language == "English")
                {
                    new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 9813)
                    .AddText("Open the panel with Ctrl + Space")
                    .AddText("This notification can be turned off in the settings.")
                    .Show();
                }
                else
                {
                    new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 9813)
                    .AddText("Откройте панель с Ctrl + Пробел")
                    .AddText("Это уведомление можно отключить в настройках.")
                    .Show();
                }
            }
            // ------------------------------------------------------------------------------

            // СЕКУНДНЫЙ ТАЙМЕР
            var timer = new System.Windows.Threading.DispatcherTimer();
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
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
                if (Properties.Settings.Default.Language == "English") {
                    SoundText.Content = "Sound settings";
                }
                else
                {
                    SoundText.Content = "Настройки звука";
                }
                DateTimeText.Content = DateTime.Now.ToString("HH:mm");
                DateTimeText1.Content = DateTime.Now.ToString("HH:mm");
                DateTimeText_Panel.Content = DateTime.Now.ToString("HH:mm");
                DateTimeText_sec.Content = ":" + DateTime.Now.ToString("ss");
                DateTimeText_sec_main.Content = ":" + DateTime.Now.ToString("ss");

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
                    MMDevice mMDevice = speakDevices.ToList()[soundDevice];
                    SoundSlider.Value = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    if(SoundSlider.Value > 0) {
                        AudioOnOff = 1;
                        Audio_settings_Color.Background = System.Windows.Media.Brushes.CornflowerBlue;
                        Audio_Settinds_text.Foreground = System.Windows.Media.Brushes.White;
                    }
                    else {
                        AudioOnOff = 0;
                        Audio_settings_Color.Background = System.Windows.Media.Brushes.Gainsboro;
                        Audio_Settinds_text.Foreground = System.Windows.Media.Brushes.Black;
                    }
                }
                MediaManager.OnNewSource += MediaManager_OnNewSource;
                MediaManager.OnRemovedSource += MediaManager_OnRemovedSource;
                MediaManager.OnPlaybackStateChanged += MediaManager_OnPlaybackStateChanged;
                MediaManager.OnSongChanged += MediaManager_OnSongChanged;
                MediaManager.Start();

                //Батарея
                GetBatteryPercent();
            };
            //timer.Start();


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
            }

            // МИНУТНЫЙ ТАЙМЕР
            var timer_minute = new System.Windows.Threading.DispatcherTimer();
            timer_minute.Interval = new TimeSpan(0, 1, 0);
            timer_minute.IsEnabled = true;
            timer_minute.Tick += (o, t) =>
            {
                foreach (var battery in allBatteries)
                {
                    BatteryChar.Add(Convert.ToInt32(battery["EstimatedChargeRemaining"]));

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
            };
            timer_minute.Start();

            //УВЕДОМЛЕНИЯ
            //NotificationAsync();
        }

        public sealed class UserNotification
        {
            public Windows.ApplicationModel.AppInfo AppInfo { get; }
            public DateTimeOffset CreationTime { get; }
            public uint Id { get; }
            public Windows.UI.Notifications.Notification Notification { get; }
        }

        public async Task NotificationAsync()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                UserNotificationListener listener = UserNotificationListener.Current;
                UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

                //IReadOnlyList<UserNotification> notifs = await listener.GetNotificationsAsync(NotificationKinds.Toast);
            }
            else
            {
                MessageBox.Show("На этом устройстве прослушивание недоступно.");
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
                        bi3.UriSource = new Uri("icons/battery_full.png", UriKind.Relative); bi3.EndInit(); Battery.Source = bi3; Battery_main.Source = bi3;
                    }
                    else if (Convert.ToDouble(battery["EstimatedChargeRemaining"]) < 80 && Convert.ToDouble(battery["EstimatedChargeRemaining"])  > 40)
                    {
                        bi3.UriSource = new Uri("icons/Battery_Normal.png", UriKind.Relative); bi3.EndInit(); Battery.Source = bi3; Battery_main.Source = bi3;
                    }
                    else
                    {
                        bi3.UriSource = new Uri("icons/Battery_Low.png", UriKind.Relative); bi3.EndInit(); Battery.Source = bi3; Battery_main.Source = bi3;
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
                    BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/Battery_Charch.png", UriKind.Relative); bi3.EndInit(); Battery.Source = bi3; Battery_main.Source = bi3;
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
                Audio_settings_Color.Background = System.Windows.Media.Brushes.Gainsboro;
                Audio_Settinds_text.Foreground = System.Windows.Media.Brushes.Black;
                TempAudioOnOff = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 0 / 100.0f;
                SoundSlider.Value = 0;
            }
            else {
                AudioOnOff = 1;
                Audio_settings_Color.Background = System.Windows.Media.Brushes.CornflowerBlue;
                Audio_Settinds_text.Foreground = System.Windows.Media.Brushes.White;
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = TempAudioOnOff / 100.0f;
                SoundSlider.Value = TempAudioOnOff;
            }
        }

        // Открытие настроек
        private void Settings_Open_Click(object sender, RoutedEventArgs e)
        {
            settings settings = new settings();
            settings.Show();
            Close();
        }

        private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (speakDevices.Count() > 0)
            {
                MMDevice mMDevice = speakDevices.ToList()[soundDevice];
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = Convert.ToInt32(SoundSlider.Value) / 100.0f;
                if (Properties.Settings.Default.Language == "English")
                {
                    SoundText.Content = "Volume: " + (Convert.ToInt32(SoundSlider.Value) / 100.0f * 100) + "%";
                }
                else
                {
                    SoundText.Content = "Громкость: " + (Convert.ToInt32(SoundSlider.Value) / 100.0f * 100) + "%";
                }
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

                RotateTransform rotateTransform1 = new RotateTransform(90);
                Open_N_Image.RenderTransform = rotateTransform1;

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

                RotateTransform rotateTransform1 = new RotateTransform(90);
                Open_N_Image.RenderTransform = rotateTransform1;

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
                SoundDevice.Content = speakDevices.ToList()[soundDevice];
            }
        }

        // КНОПКИ
        public void Open_Full_Panel()
        {
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
                RotateTransform rotateTransform1 = new RotateTransform(270);
                Open_N_Image.RenderTransform = rotateTransform1;
                NotifyAnimation.To = 215;
                not_up = 1;
            }
            else
            {
                RotateTransform rotateTransform1 = new RotateTransform(90);
                Open_N_Image.RenderTransform = rotateTransform1;
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

        int onen_about_sound = 0;
        private void Music_About_Click(object sender, RoutedEventArgs e)
        {
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
        private void Command_shutdown(object sender, MouseButtonEventArgs e)
        {
            //Выключение компьютера
            commands.FileName = "cmd.exe";
            commands.Arguments = "/c shutdown -s -f -t 00";
            Process.Start(commands);
        }
        private void Command_Restart(object sender, MouseButtonEventArgs e)
        {
            //Перезагрузка компьютера
            commands.FileName = "cmd.exe";
            commands.Arguments = "/c shutdown -r -f -t 00";
            Process.Start(commands);
        }
        private void Command_Exit(object sender, MouseButtonEventArgs e)
        {
            //Выход с приложения
            Application.Current.Shutdown();
        }

        // ПРОЧЕЕ
        private void Shutdown_Open_Panel(object sender, RoutedEventArgs e)
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

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border_Shutdown.Visibility = Visibility.Hidden;
        }

        // РАБОТА С МУЗЫКОЙ
        private static void MediaManager_OnNewSource(MediaManager.MediaSession session)
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

                var timer = new System.Windows.Threading.DispatcherTimer();
                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.IsEnabled = true;
                timer.Tick += (o, t) =>
                {
                    my.SoundTimeNow.Content = session.ControlSession.GetTimelineProperties().Position.ToString(@"mm\:ss");
                    my.music_min = session.ControlSession.GetTimelineProperties().Position.Minutes;
                    my.music_sec = session.ControlSession.GetTimelineProperties().Position.Seconds;
                };
                timer.Start();
            }));
        }

        private static void MediaManager_OnRemovedSource(MediaManager.MediaSession session)
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

        private static void MediaManager_OnSongChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
        {
            
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ChencheMusic($"{args.Title} {(String.IsNullOrEmpty(args.Artist) ? "" : $"- {args.Artist}")}");
                    MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    my.NowPlayning.Content = $"{args.Title} {(String.IsNullOrEmpty(args.Artist) ? "" : $"\n{args.Artist}")}";
                    my.NowPlayning_Autor.Content = $"{(String.IsNullOrEmpty(args.Artist) ? "" : $"{args.Artist}")}";
                    my.PlayIcon.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Pause);

                    my.SoundTimeAll.Content = sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"mm\:ss");
                    my.media_all_sec = Convert.ToInt32(sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"ss")) + 60 * Convert.ToInt32(sender.ControlSession.GetTimelineProperties().EndTime.ToString(@"mm"));

                }));
            }
            catch
            {
                //my.NowPlayning.Content = "Управление музыкой";
            }

        }

        private static void MediaManager_OnPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
        {
            ChencheMusic($"Состояние изменено");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (args.PlaybackStatus.ToString() == "Paused") //Paused   || Playing
                {
                    my.PlayIcon.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Play);
                }
                else
                {
                    my.PlayIcon.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Pause);
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

        private static async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session) =>
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
        }
    }




    public static class MediaManager
    {
        public delegate void MediaSessionDelegate(MediaSession session);

        /// <summary>
        /// Triggered when a new media source gets added to the MediaSessions
        /// </summary>
        public static event MediaSessionDelegate OnNewSource;

        /// <summary>
        /// Triggered when a media source gets removed from the MediaSessions
        /// </summary>
        public static event MediaSessionDelegate OnRemovedSource;

        /// <summary>
        /// Triggered when a playback state changes of a MediaSession
        /// </summary>
        public static event TypedEventHandler<MediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo> OnPlaybackStateChanged;

        /// <summary>
        /// Triggered when a song changes of a MediaSession
        /// </summary>
        public static event TypedEventHandler<MediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties> OnSongChanged;

        /// <summary>
        /// A dictionary of the current MediaSessions
        /// </summary>
        public static Dictionary<string, MediaSession> CurrentMediaSessions = new Dictionary<string, MediaSession>();


        private static bool IsStarted;

        /// <summary>
        /// This starts the MediaManager
        /// This can be changed to a constructor if you don't care for the first few 'new sources' events
        /// </summary>
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







/*
 
        public static async Task NowPlay()
        {
            var gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
            var mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
            //MessageBox.Show(mediaProperties.Title + " - " + mediaProperties.Artist);
        }

        private static async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session) =>
            await session.TryGetMediaPropertiesAsync();


 */
