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
using Windows.Media;
using Windows.Media.Control;
using System.Media;
using System.Runtime.InteropServices;

namespace ModernNotyfi
{
    public partial class MainWindow : Window
    {
        public int soundDevice = 1; //Активное устройство воспроизведенеия
        public int SoundDeviceOpen = 0;
        public int AudioOnOff = 0; //Вкл \ Выкл звук.
        public int TempAudioOnOff = 0;

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

        // Инициализация
        public MainWindow()
        {
            try
            {
                // Сохранение настроек после обновления
                if (Properties.Settings.Default.UpgradeRequired)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeRequired = false;
                    Properties.Settings.Default.Save();
                }

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
                // Применение настроек.
                var bc = new BrushConverter();
                Border_Time.Background = (Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Panel.Background = (Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                SoundBorder.Background = (Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Shutdown.Background = (Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);
                Border_Music.Background = (Brush)bc.ConvertFrom(Properties.Settings.Default.color_panel);

                Border_Time.Background.Opacity = Properties.Settings.Default.opacity_panel;
                Border_Panel.Background.Opacity = Properties.Settings.Default.opacity_panel;
                SoundBorder.Background.Opacity = Properties.Settings.Default.opacity_panel;
                Border_Music.Background.Opacity = Properties.Settings.Default.opacity_panel;
                NowPlayning.Foreground = SoundText.Foreground;

                if (Properties.Settings.Default.Show_Exit == "False")
                {
                    Border_Shutdown.Height = 87;
                    Border_Shutdown.Margin = new Thickness(141, 361, 0, 0);
                    Border_Shutdown.UpdateLayout();
                }

                // Положение: правый-нижний угол.
                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                this.Left = desktopWorkingArea.Right - this.Width;
                this.Top = desktopWorkingArea.Bottom - this.Height;
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

        // Потеря фокуса
        private void ModernNotyfi_Deactivated(object sender, EventArgs e)
        {
            ModernNotyfi.WindowState = WindowState.Minimized;
        }
        
        private void ModernNotyfi_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                // Время
                SoundText.Content = "Настройки звука";
                DateTimeText.Content = DateTime.Now.ToString("HH:mm");
                DateTimeText_sec.Content = ":" + DateTime.Now.ToString("ss");
                // Положение: правый-нижний угол.
                this.Left = desktopWorkingArea.Right - this.Width;
                this.Top = desktopWorkingArea.Bottom - this.Height;
                // ---------------------------------------------------------------------
                if (speakDevices.Count() > 0)
                {
                    MMDevice mMDevice = speakDevices.ToList()[soundDevice];
                    SoundSlider.Value = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    if(SoundSlider.Value > 0) {
                        AudioOnOff = 1;
                        Audio_settings_Color.Background = Brushes.CornflowerBlue;
                        Audio_Settinds_text.Foreground = Brushes.White;
                    }
                    else {
                        AudioOnOff = 0;
                        Audio_settings_Color.Background = Brushes.Gainsboro;
                        Audio_Settinds_text.Foreground = Brushes.Black;
                    }
                }
                MediaManager.OnNewSource += MediaManager_OnNewSource;
                MediaManager.OnRemovedSource += MediaManager_OnRemovedSource;
                MediaManager.OnPlaybackStateChanged += MediaManager_OnPlaybackStateChanged;
                MediaManager.OnSongChanged += MediaManager_OnSongChanged;
                MediaManager.Start();
            };
            timer.Start();
        }

        private void On_OFF_Audio(object sender, RoutedEventArgs e)
        {
            MMDevice mMDevice = speakDevices.ToList()[soundDevice];
            if (AudioOnOff == 1) {
                AudioOnOff = 0;
                Audio_settings_Color.Background = Brushes.Gainsboro;
                Audio_Settinds_text.Foreground = Brushes.Black;
                TempAudioOnOff = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 0 / 100.0f;
            }
            else {
                AudioOnOff = 1;
                Audio_settings_Color.Background = Brushes.CornflowerBlue;
                Audio_Settinds_text.Foreground = Brushes.White;
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = TempAudioOnOff / 100.0f;
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
                SoundText.Content = "Громкость: " + (Convert.ToInt32(SoundSlider.Value) / 100.0f * 100) + "%";
            }
        }

        private void SoundDevice_Click(object sender, RoutedEventArgs e)
        {
            if (SoundDeviceOpen == 0)
            {
                SoundDeviceOpen = 1;
                DoubleAnimation SoundDeviceAnimation = new DoubleAnimation();
                SoundDeviceAnimation.From = SoundBorder.ActualHeight;
                SoundDeviceAnimation.To = 150;
                SoundDeviceAnimation.Duration = TimeSpan.FromSeconds(0.5);
                SoundBorder.BeginAnimation(Border.HeightProperty, SoundDeviceAnimation);

                ThicknessAnimation Border_MusicAnimation = new ThicknessAnimation();
                Border_MusicAnimation.From = new Thickness(10, 230, 0, 0);
                Border_MusicAnimation.To = new Thickness(10, 150, 0, 0);
                Border_MusicAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Border_Music.BeginAnimation(Border.MarginProperty, Border_MusicAnimation);
                //Margin="10,150,0,0"
            }
            else
            {
                SoundDeviceOpen = 0;
                DoubleAnimation SoundDeviceAnimation = new DoubleAnimation();
                SoundDeviceAnimation.From = SoundBorder.ActualHeight;
                SoundDeviceAnimation.To = 70;
                SoundDeviceAnimation.Duration = TimeSpan.FromSeconds(0.5);
                SoundBorder.BeginAnimation(Border.HeightProperty, SoundDeviceAnimation);

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
            timerChechMusic.Start();
        }

        private static void MediaManager_OnSongChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
        {
            ChencheMusic($"{args.Title} {(String.IsNullOrEmpty(args.Artist) ? "" : $"- {args.Artist}")}");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.NowPlayning.Content = $"{args.Title} {(String.IsNullOrEmpty(args.Artist) ? "" : $"- {args.Artist}")}";
            }));
        }

        private static void MediaManager_OnPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
        {
            ChencheMusic($"Состояние изменено");
        }

        private static void ChencheMusic(string Songs)
        {
            _ = Songs;
        }

        public static async Task NowPlay()
        {
            var gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
            var mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
            //MessageBox.Show(mediaProperties.Title + " - " + mediaProperties.Artist);
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
        }

        private void Music_Right_Click(object sender, RoutedEventArgs e)
        {
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        private void Music_Left_Click(object sender, RoutedEventArgs e)
        {
            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
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
                var props = await session.TryGetMediaPropertiesAsync();
                string song = $"{props.Title} | {props.Artist}";

                //This is needed because for some reason this method is invoked twice every song change
                if (LastSong != song && !(String.IsNullOrWhiteSpace(props.Title) && String.IsNullOrWhiteSpace(props.Artist)))
                {
                    LastSong = song;
                    OnSongChanged?.Invoke(this, props);
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