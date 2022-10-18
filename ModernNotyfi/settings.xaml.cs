using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using System.IO;
using System.Globalization;
using Woof.SystemEx;
using System.Windows.Threading;
using System.Windows.Interop;
using WPFUI;
using WPFUI.Controls;
using static ModernNotyfi.PInvoke.ParameterTypes;
using static ModernNotyfi.PInvoke.Methods;

namespace ModernNotyfi
{
    public partial class settings : Window
    {
        public DispatcherTimer timer_sec = new DispatcherTimer();



        public settings()
        {
            InitializeComponent();

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
                        stylewin_combo.SelectedIndex = 0;
                        WPFUI.Appearance.Watcher.Watch(this, WPFUI.Appearance.BackgroundType.Mica, true);
                    }
                    else
                    {
                        WPFUI.Appearance.Watcher.Watch(this, WPFUI.Appearance.BackgroundType.Auto, true);
                    }
                };
            }

            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            timer_sec.Interval = new TimeSpan(0, 0, 1);
            timer_sec.IsEnabled = true;
            timer_sec.Tick += (o, t) =>
            {
                updateaccount();
            };
            timer_sec.Start();


            // Закрытие сервисов
            ServiceMyDeviceNet service = Application.Current.Windows.OfType<ServiceMyDeviceNet>().FirstOrDefault();
            if (service.IsActive == true)
            {
                service.Close();
            }
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

        bool no_login = true;

        public void updateaccount()
        {
            if (Properties.Settings.Default.Unesell_Login == "Yes")
            {
                localacc.Content = "Unesell Account";
                try
                {
                    if (no_login)
                    {
                        var userBitmapSmall = new BitmapImage(new Uri("https://unesell.com/data/users/avatar/" + Properties.Settings.Default.Unesell_Avatar));
                        AccauntImg.ImageSource = userBitmapSmall;
                        no_login = false;
                    }
                }
                catch
                {
                    var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
                    AccauntImg.ImageSource = userBitmapSmall;
                }
                Login_Unesell.Content = "Выйти с этого устройства";
            }
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            MicaBool.IsChecked = Properties.Settings.Default.MicaBool;
            if (MicaBool.IsChecked == false) { snakshow = true; }

            var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
            AccauntImg.ImageSource = userBitmapSmall;

            NameProfile.Content = "Привет, " + Properties.Settings.Default.User_Name;

            updateaccount();

            EctInfoSys.Content = "";
            
            Version.Content = "Версия приложения: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            version.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get()) { WindowsVersion.Content = "Система: " + os["Caption"].ToString(); break; }
            Memory.Content = "RAM: " + Math.Round(SysInfo.SystemMemoryTotal, 2) + "GB";

            LastChekUpdate.Content = "Последняя проверка: " + Properties.Settings.Default.last_check_update;

            //Настройки
            Opacity_Panel_Settings.Value = Properties.Settings.Default.opacity_panel * 100;
            Color_Border.Text = Properties.Settings.Default.color_panel;
            Border_Preview.CornerRadius = new CornerRadius(Properties.Settings.Default.CornerRadius);
            CRadius_Panel_Settings.Value = Properties.Settings.Default.CornerRadius;

            if (Properties.Settings.Default.theme == "light")
            {
                theme_combo.SelectedIndex = 0;
                Light_Theme();
            } else if(Properties.Settings.Default.theme == "dark")
            {
                theme_combo.SelectedIndex = 1;
                Dark_Theme();
            }
            else
            {
                theme_combo.SelectedIndex = 2;
            }

            if (Properties.Settings.Default.posicion == "rigth")
            {
                pos_combo.SelectedIndex = 0;
            }
            else
            {
                pos_combo.SelectedIndex = 1;
            }

            if (Properties.Settings.Default.progressbarstyle == "new")
            {
                Chek_Allow_NEW_PgBar.IsChecked = true;
            }
            else
            {
                Chek_Allow_NEW_PgBar.IsChecked = false;
            }

            if (Properties.Settings.Default.Language == "Russian")
            {
                Language_combo1.SelectedIndex = 0;
                RussianInterfase_Settings();
            }
            else if(Properties.Settings.Default.Language == "English")
            {
                Language_combo1.SelectedIndex = 1;
                EnglishInterfase_Settings();
            }

            if (Properties.Settings.Default.Show_Exit == "True")
            {
                Exit_Setting.IsChecked = true;
            }
            else
            {
                Exit_Setting.IsChecked = false;
            }

            if (Properties.Settings.Default.show_start_notify == true)
            {
                Chek_Start_Notify.IsChecked = true;
            }
            else
            {
                Chek_Start_Notify.IsChecked = false;
            }

            if (Properties.Settings.Default.Disign_Shutdown == "old")
            {
                Style_Shutdown.SelectedIndex = 0;
            }
            else
            {
                Style_Shutdown.SelectedIndex = 1;
            }

            if (Properties.Settings.Default.WinStyle == "Mica")
            {
                if (MicaBool.IsChecked == true)
                {
                    ApplyBackgroundEffect(0);
                }
                stylewin_combo.SelectedIndex = 0;
            }
            if (Properties.Settings.Default.WinStyle == "Acrylic")
            {
                if (MicaBool.IsChecked == true)
                {
                    ApplyBackgroundEffect(1);
                }
                stylewin_combo.SelectedIndex = 1;
            }
            if (Properties.Settings.Default.WinStyle == "Tabbed")
            {
                if (MicaBool.IsChecked == true)
                {
                    ApplyBackgroundEffect(2);
                }
                stylewin_combo.SelectedIndex = 2;
            }

            if (Properties.Settings.Default.Startup == "Panel")
            {
                StartModernNotify.IsChecked = true;
            }
            if (Properties.Settings.Default.Startup == "Connect")
            {
                StartMyDevice.IsChecked = true;
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Startup mainWindow = new Startup();
            mainWindow.Show();
            Close();
        }

        private void Backs(object sender, RoutedEventArgs e)
        {
            Back.Visibility = Visibility.Hidden;
            Settings_Tab.SelectedIndex = 0;
            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings";
            }
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/settings.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
        }

        private void Open_Main_Click(object sender, RoutedEventArgs e)
        {
            Settings_Tab.SelectedIndex = 1; Back.Click += Backs;
            Back.Visibility = Visibility.Visible;
            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки > Основные настройки";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings > Basic settings";
            }
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/maim_settings.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
        }

        private void Open_Keyboard_Click(object sender, RoutedEventArgs e)
        {
            Settings_Tab.SelectedIndex = 2; Back.Click += Backs;
            Back.Visibility = Visibility.Visible;
            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки > Боковая игровая панель";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings > Game performance bar";
            }
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/App_Color.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
        }

        private void Open_Personalization_Click(object sender, RoutedEventArgs e)
        {
            Settings_Tab.SelectedIndex = 3; Back.Click += Backs;
            Back.Visibility = Visibility.Visible;
            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки > Персонализация";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings > Personalization";
            }
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/Personalization.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
        }

        private void Open_Info_Click(object sender, RoutedEventArgs e)
        {
            Settings_Tab.SelectedIndex = 4; Back.Click += Backs;
            Back.Visibility = Visibility.Visible;
            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки > О программе";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings > About";
            }
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/info.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
        }

        private void Open_Update_Click(object sender, RoutedEventArgs e)
        {
            Settings_Tab.SelectedIndex = 5; Back.Click += Backs;
            Back.Visibility = Visibility.Visible;

            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки > Обновления программы";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings > Software updates";
            }

            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/update.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
        }

        private void Open_Help_Click(object sender, RoutedEventArgs e)
        {
            Settings_Tab.SelectedIndex = 4; Back.Click += Backs;
            Back.Visibility = Visibility.Visible;
            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки > Помощь и информация";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings > About";
            }
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/info.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
        }

        private void Open_Color_Click(object sender, RoutedEventArgs e)
        {
            Settings_Tab.SelectedIndex = 6;
            Back.Visibility = Visibility.Visible;

            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки > Персонализация > Цвет";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings > Personalization > Color";
            }

            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/color.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
            color_piker.Text = Color_Border.Text;
            Back.Click += Open_Personalization_Click;

            string hexString = Color_Border.Text;

            if (hexString.IndexOf('#') != -1)
                hexString = hexString.Replace("#", "");

            Red.Value = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            Green.Value = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            Blue.Value = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
            NavView_Navigate(item as NavigationViewItem);
        }

        private void NavView_Navigate(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "Home":
                    Open_Main.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    break;
                case "Keyboard":
                    Open_Keyboard.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    break;
                case "Library":
                    Open_Personalization.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    break;
                case "Help":
                    Open_Info.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    break;
            }
        }

        int start_update = 0;
        public string update_version;
        private void Check_Update_Click(object sender, RoutedEventArgs e)
        {
            ChangeLog.Visibility = Visibility.Hidden;
            LastChekUpdate.Visibility = Visibility.Visible;

            //Проверка модуля обновления.
            if (Properties.Settings.Default.Language == "Russian")
            {
                InfoUpdate.Content = "Проверка модуля обновления...";
            }
            else if (Properties.Settings.Default.Language == "English")
            {
                InfoUpdate.Content = "Checking update module...";
            }
            
            try
            {
                FileVersionInfo.GetVersionInfo("update.exe");
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo("update.exe");
                update_version = myFileVersionInfo.FileVersion;
            }
            catch
            {
                update_version = "0";
            }

            try
            {
                string url_version_update = "https://unesell.com/api/version/modernnotify/update_version.txt";

                if (GetContent(url_version_update) == update_version)
                {
                    // АКТУАЛЬНАЯ ВЕРСИЯ
                    CheckUpdateApp();
                }
                else
                {
                    
                    if (Properties.Settings.Default.Language == "Russian")
                    {
                        InfoUpdate.Content = "Загрузка модуля обновления...";
                    }
                    else if (Properties.Settings.Default.Language == "English")
                    {
                        InfoUpdate.Content = "Loading update module...";
                    }

                    try
                    {
                        string link = @"https://unesell.com/api/version/modernnotify/update.exe";
                        WebClient webClient = new WebClient();
                        webClient.DownloadProgressChanged += (o, args) => LastChekUpdate.Content = "Скачивание: " + args.ProgressPercentage + "%";
                        webClient.DownloadFileCompleted += (o, args) => CheckUpdateApp();
                        webClient.DownloadFileAsync(new Uri(link), "update.exe");
                    }
                    catch
                    {
                        if (Properties.Settings.Default.Language == "Russian")
                        {
                            InfoUpdate.Content = "Загрузка обновления не удалась.";
                            LastChekUpdate.Content = "Проверьте подключение или попробуйте позже.";
                        }
                        else if (Properties.Settings.Default.Language == "English")
                        {
                            InfoUpdate.Content = "Update download failed.";
                            LastChekUpdate.Content = "Check your connection or try again later.";
                        }
                    }
                }
            }
            catch
            {
                if (Properties.Settings.Default.Language == "Russian")
                {
                    InfoUpdate.Content = "Загрузка обновления не удалась.";
                    LastChekUpdate.Content = "Проверьте подключение или попробуйте позже.";
                }
                else if (Properties.Settings.Default.Language == "English")
                {
                    InfoUpdate.Content = "Update download failed.";
                    LastChekUpdate.Content = "Check your connection or try again later.";
                }
            }
        }

        public void CheckUpdateApp()
        {
            // Файл обновления существует.
            if (start_update == 0)
            {
                Properties.Settings.Default.last_check_update = DateTime.Now.ToString();
                Properties.Settings.Default.Save();
                
                if (Properties.Settings.Default.Language == "Russian")
                {
                    InfoUpdate.Content = "Проверка...";
                    LastChekUpdate.Content = "Последняя проверка: " + Properties.Settings.Default.last_check_update;
                }
                else if (Properties.Settings.Default.Language == "English")
                {
                    InfoUpdate.Content = "Checking...";
                    LastChekUpdate.Content = "Last check: " + Properties.Settings.Default.last_check_update;
                }

                //DEV VERSION CHECK
                if (GetContent("https://unesell.com/api/version/modernnotify/version_dev.txt") == Assembly.GetExecutingAssembly().GetName().Version.ToString())
                {
                    
                    if (Properties.Settings.Default.Language == "Russian")
                    {
                        InfoUpdate.Content = "У вас актуальная версия";
                    }
                    else if (Properties.Settings.Default.Language == "English")
                    {
                        InfoUpdate.Content = "You have the current version.";
                    }

                    BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/ok.png", UriKind.Relative); bi3.EndInit();
                    Update_img.Source = bi3;
                }
                else
                {
                    if (GetContent("https://unesell.com/api/version/modernnotify/version_dev.txt") != "Error")
                    {
                        if (Properties.Settings.Default.Language == "Russian")
                        {
                            InfoUpdate.Content = "Доступна новая версия " + GetContent("https://unesell.com/api/version/modernnotify/version_dev.txt");
                            ChangeLog.Visibility = Visibility.Visible;
                            LastChekUpdate.Visibility = Visibility.Hidden;
                            ChangeLog.Content = "Что нового?";
                            ///LastChekUpdate.Content = "Готовы к обновлению.";
                            Check_Update.Content = "Начать обновление";
                        }
                        else if (Properties.Settings.Default.Language == "English")
                        {
                            InfoUpdate.Content = "New version available: " + GetContent("https://unesell.com/api/version/modernnotify/version_dev.txt");
                            ChangeLog.Visibility = Visibility.Visible;
                            LastChekUpdate.Visibility = Visibility.Hidden;
                            ChangeLog.Content = "What's new?";
                            //LastChekUpdate.Content = "Ready to upgrade.";
                            Check_Update.Content = "Start update";
                        }
                        BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/restart_fluent.png", UriKind.Relative); bi3.EndInit();
                        Update_img.Source = bi3;
                        start_update = 1;
                    }
                    else
                    {
                        if (Properties.Settings.Default.Language == "Russian")
                        {
                            InfoUpdate.Content = "Нет подключения к интернету.";
                            LastChekUpdate.Content = "Мы не смогли получить ответ от сервера. Проверьте подключение.";
                        }
                        else if (Properties.Settings.Default.Language == "English")
                        {
                            InfoUpdate.Content = "No internet connection.";
                            LastChekUpdate.Content = "We were unable to get a response from the server. Check connection.";
                        }
                    }
                }
            }
            else
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.FileName = "update.exe";
                    p.Start();
                    Application.Current.Shutdown();
                }
                catch
                {
                    if (Properties.Settings.Default.Language == "Russian")
                    {

                    }
                    else if (Properties.Settings.Default.Language == "English")
                    {

                    }
                    InfoUpdate.Content = "Ошибка запуска процесса обновления.";
                    LastChekUpdate.Content = "Не найден файл обновления. Переустановите программу.";
                }
            }
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
                InfoUpdate.Content = "Не удалось проверить обновления.";
                Properties.Settings.Default.last_check_update = DateTime.Now.ToString();
                Properties.Settings.Default.Save();
                LastChekUpdate.Content = "Последняя проверка: " + Properties.Settings.Default.last_check_update;
                BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/shutdown.png", UriKind.Relative); bi3.EndInit();
                Update_img.Source = bi3;
                return "Error";
            }
        }

        private void Save_Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bc = new BrushConverter();
                Border_Preview.Background = (Brush)bc.ConvertFrom(Color_Border.Text);
                Material_Border.Background = (Brush)bc.ConvertFrom(Color_Border.Text);

                //Сохранение.
                Properties.Settings.Default.opacity_panel = Opacity_Panel_Settings.Value / 100;
                Properties.Settings.Default.color_panel = Color_Border.Text;

                if (MicaBool.IsChecked == true)
                {
                    Properties.Settings.Default.MicaBool = true;
                    if (stylewin_combo.SelectedIndex == 0)
                    {
                        Properties.Settings.Default.WinStyle = "Mica";
                    }
                    if (stylewin_combo.SelectedIndex == 1)
                    {
                        Properties.Settings.Default.WinStyle = "Acrylic";
                    }
                    if (stylewin_combo.SelectedIndex == 2)
                    {
                        Properties.Settings.Default.WinStyle = "Tabbed";
                    }
                }
                else
                {
                    Properties.Settings.Default.MicaBool = false;
                }

                if (theme_combo.SelectedIndex == 0) { Properties.Settings.Default.theme = "light"; }
                else if(theme_combo.SelectedIndex == 1) { Properties.Settings.Default.theme = "dark"; } else { Properties.Settings.Default.theme = "system"; }

                if (pos_combo.SelectedIndex == 0) { Properties.Settings.Default.posicion = "rigth"; }
                else { Properties.Settings.Default.posicion = "left"; }

                if (Exit_Setting.IsChecked == true){
                    Properties.Settings.Default.Show_Exit = "True";
                }
                else {
                    Properties.Settings.Default.Show_Exit = "False";
                }

                if (Chek_Start_Notify.IsChecked == true)
                {
                    Properties.Settings.Default.show_start_notify = true;
                }
                else
                {
                    Properties.Settings.Default.show_start_notify = false;
                }

                if (Chek_Allow_NEW_PgBar.IsChecked == true)
                {
                    Properties.Settings.Default.progressbarstyle = "new";
                }
                else
                {
                    Properties.Settings.Default.progressbarstyle = "old";
                }

                Properties.Settings.Default.CornerRadius = (int)CRadius_Panel_Settings.Value;

                if (Style_Shutdown.SelectedIndex == 0) { Properties.Settings.Default.Disign_Shutdown = "old"; }
                else { Properties.Settings.Default.Disign_Shutdown = "new"; }

                if (Language_combo1.SelectedIndex == 0)
                {
                    Properties.Settings.Default.Language = "Russian";
                }
                else if (Language_combo1.SelectedIndex == 1)
                {
                    Properties.Settings.Default.Language = "English";
                }

                Properties.Settings.Default.Save();


                //Применение.
                Startup mainWindow = new Startup();
                mainWindow.Show();
                Close();
            }
            catch
            {
                System.Windows.MessageBox.Show("Мы не смогли сохранить данные. Проверьте все изменённые настройки.", "Ошибка применения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValueUpdate(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                Border_Preview.Background.Opacity = Convert.ToDouble(Opacity_Panel_Settings.Value / 100);
                Material_Border.Background.Opacity = Convert.ToDouble(Opacity_Panel_Settings.Value / 100);
                timer.Stop();
            };
            timer.Start();
        }

        private void RGBtoHESH(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            REDtext.Content = Convert.ToInt16(Red.Value); GREENtext.Content = Convert.ToInt16(Green.Value); BLUEtext.Content = Convert.ToInt16(Blue.Value);
            Color myColor = Color.FromRgb(Convert.ToByte(Red.Value), Convert.ToByte(Green.Value), Convert.ToByte(Blue.Value));
            string hex = myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");

            color_piker.Text = "#" + hex;

            var bc = new BrushConverter();
            Material_Border.Background = (Brush)bc.ConvertFrom(color_piker.Text);
            Material_Border.Background.Opacity = Convert.ToDouble(Opacity_Panel_Settings.Value / 100);
        }

        private void Color_Border_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var bc = new BrushConverter();
                Border_Preview.Background = (Brush)bc.ConvertFrom(Color_Border.Text);
                Border_Preview.Background.Opacity = Convert.ToDouble(Opacity_Panel_Settings.Value / 100);

                Material_Border.Background = (Brush)bc.ConvertFrom(Color_Border.Text);
                Material_Border.Background.Opacity = Convert.ToDouble(Opacity_Panel_Settings.Value / 100);
            }
            catch
            {
                Color_Border.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                Color_Border.BorderBrush = new SolidColorBrush(Colors.Black);
            }
        }

        private void Site_color_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://htmlcolorcodes.com/");
        }

        private void Open_Tab_Update_Click(object sender, RoutedEventArgs e)
        {
            Open_Update.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        }

        private void theme_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (theme_combo.SelectedIndex == 0)
            {
                Light_Theme();
            }
            else if (theme_combo.SelectedIndex == 1)
            {
                Dark_Theme();
            }
            else
            {

            }
        }

        private void Go_Tab_1_Click(object sender, RoutedEventArgs e)
        {
            Open_Main.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        }

        private void Color_Save_Click(object sender, RoutedEventArgs e)
        {
            Color_Border.Text = color_piker.Text;
            Open_Personalization.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            Back.Click += Backs;
        }

        public void Chenche_Color(object sender, RoutedEventArgs e)
        {
            color_piker.Text = Convert.ToString(((System.Windows.Controls.Button)sender).Tag);
        }

        private void Open_Sourse(object sender, RoutedEventArgs e)
        {
            DisplayDialog("Используемый исходный код.", "1. ModernWpf | Modern styles and controls for your WPF applications\n2. nAudio | Audio and MIDI library for .NET\n3. LiveCharts | Powerful charts, maps and gauges for .Net");
        }

        private void CRadius_Panel_Settings_Value(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Border_Preview.CornerRadius = new CornerRadius(CRadius_Panel_Settings.Value);
        }

        private void Open_Sourse_Code(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Stamir36/ModernNotyfi");
        }

        private void Open_Site_Progect(object sender, RoutedEventArgs e)
        {
            Process.Start("https://unesell.com/modernnotify/");
        }

        private void Select_Language(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Language_combo1.SelectedIndex == 0)
            {
                RussianInterfase_Settings();
            }
            if (Language_combo1.SelectedIndex == 1)
            {
                EnglishInterfase_Settings();
            }
        }

        public void RussianInterfase_Settings()
        {
            TitleBarApp.Title = "Параметры ModernNotify";
            if (Properties.Settings.Default.Unesell_Login == "Yes")
            {
                localacc.Content = "Unesell аккаунт";
            }
            else
            {
                localacc.Content = "Локальный аккаунт";
            }
            text_7.Content = "Категории:";
            mainsett_text.Text = "Основные параметры";
            perssetttext.Text = "Персонализация";
            widgettmaintext.Text = "Игровая боковая панель";
            aboutstext.Text = "О программе";
            updatemainsett.Text = "Центр обновления";
            close.Content = "Закрыть окно";
            Save_Settings.Content = "Сохранить изменения";
            NameProfile.Content = "Привет, " + Properties.Settings.Default.User_Name;
            text_1.Content = "Основные параметры";
            funnanduse.Content = "Функции и взаимодействие";
            subbone.Content = "Панель выключения.";
            thametext.Title = "Тема приложения";
            text0213023.Title = "Местоположение панели";
            text3243.Title = "Показывать стартовое уведомление";
            textstyleshutdown.Title = "Стиль панели выключения";
            subbone.Content = "Панель выключения.";
            texts65464.Title = "Кнопка: Выключение приложения";
            text_2.Content = "Настройки игрового оверлея";
            Back.Content = "< Назад";
            // Ревизия 1.
            text_3.Content = "Вид панелей:";
            pparone.Title = "Прозрачность";
            pcolortext.Title = "Цвет";
            Open_Color.Content = "Палитра >";
            pradiusboprder.Title = "Закруглённость углов";
            infotextpreview.Content = "Информация";
            text_3_Copy1.Content = "Вид окон";
            stylewindowtext.Title = "Стиль окон:"; 
            otherptext.Content = "Прочее:";
            otherptext2.Content = "Прочее:";
            Open_Tab_Update.Content = "Проверить обновления";
            Open_Site.Content = "Сайт проекта";
            textaboutproduct.Content = "О продукте:";
            text_5.Content = "Удобная и красивая панель быстрых действий на вашем\nкомпьютере под управлением Windows 10 и 11.";
            Devetxt.Content = "Разработчик:";
            text_6.Content = "Станислав Мирошниченко";
            text_5_Copy.Content = "Прочая информация:";
            text_5_dadsc.Content = "Используемый исходный код";
            text_5_dvdsacv.Content = "Репозиторий";
            Check_Update.Content = "Проверить наличие обновлений";
            inforelithe.Content = "Информация о выпуске";
            SysInfoHeader.Content = "Информация о системе";
            Color_Save.Content = "Сохранить";
            Site_color.Content = "Сайт кодов";
            redtext.Content = "Крассный";
            greentext.Content = "Зелёный";
            bluetext.Content = "Синий";
            text23r923ri12.Title = "Широкий слайдер громкости";

            // Revision 3
            Login_Unesell.Content = "Войти в Unesell Аккаунт";
            Tab1.Content = "Главная";
            Tab2.Content = "GameBar";
            Tab3.Content = "Основное";
            Tab4.Content = "Дизайн";
            Tab5.Content = "Версия";
            Tab6.Content = "Помощь";
            Link1.Content = "Тема приложения";
            Link2.Content = "Цвет акцента элементов";
            twxtwidgwtwidgwtmenu1.Text = "Мои устройства";
            InDevelop.Text = "В разработке";
            InfoUpdate.Content = "Выполните проверку обновлений";
            SubText1.Text = "Редактируйте главные параметры приложения, такие как тема, положение и взаимодействие.";
            SubText2.Text = "Изменяйте внешний вид панели на свой вкус и предпочтения.";
            SubText3.Text = "Настройка игрового оверлея, и других параметров панели.";
            SubText4.Text = "Информация о разработчике и продукте. Дополнительные ссылки.";
            SubText5.Text = "Проверка обновлений ModernNotify и компонентов сервисов.";
            thametext.Subtitle = "Основной стиль оформления приложения.";
            text0213023.Subtitle = "Выберите, на какой стороне рабочего стола будет находиться панель.";
            SubText8.Subtitle = "Выберите перевод приложения";
            text3243.Subtitle = "При запуске приложения будет выводится подсказка об 'Ctrl + Пробел'";
            textstyleshutdown.Subtitle = "Панель выключения может быть компактной или на весь экран.";
            texts65464.Subtitle = "Отображать кнопку 'Закрыть панель' на панели выключения.";
            pcolortext.Subtitle = "Фоновый цвет панелей";
            pparone.Subtitle = "Показатель, насколько будет просвечивать фон панель.";
            pradiusboprder.Subtitle = "Радиус закругления углов панелей.";
            stylewindowtext.Subtitle = "Только для Windows 11 (build 22581+)";
            text23r923ri12.Subtitle = "Увеличивает задний план слайдера громкости.";
            MicaBool.Content = "Разрешить";
            LinkOpenUpdate.Content = "Запустить модуль обновления";
            InfoGamePanel.Title = "Запуск игрового оверлея";
            InfoGamePanel.Subtitle = "Комбинация клавиш для вызова панели производительности";
        }

        public void EnglishInterfase_Settings()
        {
            // Revision 0
            TitleBarApp.Title = "ModernNotify Settings";
            if (Properties.Settings.Default.Unesell_Login == "Yes")
            {
                localacc.Content = "Unesell account";
            }
            else
            {
                localacc.Content = "Local account";
            }
            text_7.Content = "Categories:";
            mainsett_text.Text = "Main parameters";
            perssetttext.Text = "Personalization";
            widgettmaintext.Text = "Game performance bar";
            aboutstext.Text = "About the program";
            updatemainsett.Text = "Software updates";
            close.Content = "Close";
            Save_Settings.Content = "Save changes";
            NameProfile.Content = "Hi, " + Properties.Settings.Default.User_Name + "!";
            text_1.Content = "Main parameters";
            funnanduse.Content = "Functions and interactions";
            subbone.Content = "Shutdown panel";
            thametext.Title = "Application theme";
            text0213023.Title = "Panel location";
            text3243.Title = "Show start notification";
            textstyleshutdown.Title = "Shutdown bar style";
            subbone.Content = "Shutdown panel.";
            texts65464.Title = "Button: Turn off the application";
            text_2.Content = "Settings Game Bar";
            text23r923ri12.Title = "Wide volume slider";
            Back.Content = "< Back";
            // Revision 1
            text_3.Content = "Panel style:";
            pparone.Title = "Transparency";
            pcolortext.Title = "Color";
            Open_Color.Content = "Palette >";
            pradiusboprder.Title = "Roundness of corners";
            infotextpreview.Content = "Information";
            text_3_Copy1.Content = "Window type";
            stylewindowtext.Title = "Window style:";
            otherptext.Content = "Other:";
            otherptext2.Content = "Other:";
            Open_Tab_Update.Content = "Check for updates";
            Open_Site.Content = "Project site";
            textaboutproduct.Content = "About the product:";
            text_5.Content = "Convenient and beautiful quick action bar on your\ncomputer running Windows 10 and 11.";
            Devetxt.Content = "Developer:";
            text_6.Content = "Stanislav Miroshnichenko";
            text_5_Copy.Content = "Other information:";
            text_5_dadsc.Content = "Source code used";
            text_5_dvdsacv.Content = "Repository";
            Check_Update.Content = "Check for updates";
            inforelithe.Content = "Release Notes";
            SysInfoHeader.Content = "System information";
            Color_Save.Content = "Save";
            Site_color.Content = "Codes site";
            redtext.Content = "Red";
            greentext.Content = "Green";
            bluetext.Content = "Blue";
            // Revision 3
            Login_Unesell.Content = "Login to Unesell Account";
            Tab1.Content = "Home";
            Tab2.Content = "GameBar";
            Tab3.Content = "Main";
            Tab4.Content = "Design";
            Tab5.Content = "Update";
            Tab6.Content = "Help";
            Link1.Content = "App Theme";
            Link2.Content = "Element accent color";
            twxtwidgwtwidgwtmenu1.Text = "My devices";
            InDevelop.Text = "In develop";
            InfoUpdate.Content = "Check for updates";
            SubText1.Text = "Edit the application's main parameters such as theme, position, and interaction.";
            SubText2.Text = "Change the appearance of the panel to your taste and preferences.";
            SubText3.Text = "Setting the game overlay, and other panel options.";
            SubText4.Text = "Information about the developer and product. Additional links.";
            SubText5.Text = "Check for updates to ModernNotify and service components.";
            thametext.Subtitle = "The main style of the application.";
            text0213023.Subtitle = "Choose which side of the desktop the panel will be on.";
            SubText8.Subtitle = "Choose app translation";
            text3243.Subtitle = "When starting the application, a tooltip about 'Ctrl + Space' will be displayed.";
            textstyleshutdown.Subtitle = "The shutdown panel can be compact or full screen.";
            texts65464.Subtitle = "Display a 'Close Panel' button on the shutdown panel.";
            pcolortext.Subtitle = "Panel background color";
            pparone.Subtitle = "An indicator of how much the background of the panel will shine through.";
            pradiusboprder.Subtitle = "Panel corner radius.";
            stylewindowtext.Subtitle = "Windows 11 only (build 22581+)";
            text23r923ri12.Subtitle = "Increases the background of the volume slider.";
            MicaBool.Content = "Allow";
            LinkOpenUpdate.Content = "Start update module";
        }

        private void LoginWebUnesell(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Unesell_Login == "Yes")
            {
                var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
                AccauntImg.ImageSource = userBitmapSmall;
                Login_Unesell.Content = "Войти в Unesell Аккаунт";
                localacc.Content = "Локальный аккаунт";
                Properties.Settings.Default.Unesell_id = "";
                Properties.Settings.Default.Unesell_Email = "";
                Properties.Settings.Default.Unesell_Avatar = "";
                Properties.Settings.Default.Unesell_Login = "No";
                Properties.Settings.Default.Save();
                no_login = true;
            }
            else
            {
                Other_Page.UiLoginUnesell UiLogin = new Other_Page.UiLoginUnesell();
                UiLogin.Show();
            }
        }

        private void OpenGameBar(object sender, MouseButtonEventArgs e)
        {
            GameBar gameBar = new GameBar();
            gameBar.Show();
        }

        private void Open_MyDivece_Click(object sender, MouseButtonEventArgs e)
        {
            MyDevice myDevice = new MyDevice();
            myDevice.Show();
        }

        // ModernWpf.ThemeManager.GetActualTheme(this).ToString(); <- Тема установленная в системе
        public void Light_Theme()
        {
            this.Background = Brushes.Transparent;
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#E5E1E1E1");
            MicaBool.IsChecked = false;

            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Light);

            Color_Border.Foreground = (Brush)bc.ConvertFrom("#F2343434");
            Titles.Foreground = (Brush)bc.ConvertFrom("#FF000000");
            if (Color_Border.Text == "#404040" || Color_Border.Text == "#333333") { Color_Border.Text = "#ffffff"; }
        }

        public void Dark_Theme()
        {
            //Background="#F21B1B1B"
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F21B1B1B");
            
            
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);

            Color_Border.Foreground = (Brush)bc.ConvertFrom("#F2FFFFFF");
            Titles.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
            if (Color_Border.Text == "#ffffff") { Color_Border.Text = "#404040"; }
        }

        private void WindowsStyle_Select(object sender, SelectionChangedEventArgs e)
        {
            
            if (MicaBool.IsChecked == true)
            {
                // Properties.Settings.Default.WinStyle == "Mica";
                if (stylewin_combo.SelectedIndex == 0)
                {
                    //Mica
                    ApplyBackgroundEffect(0);
                }
                if (stylewin_combo.SelectedIndex == 1)
                {
                    //Acrylic
                    ApplyBackgroundEffect(1);
                }
                if (stylewin_combo.SelectedIndex == 2)
                {
                    //Tabbed
                    ApplyBackgroundEffect(2);
                }
            }

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
            theme_combo.SelectedIndex = 1;
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;

            WPFUI.Appearance.Background.Remove(windowHandle);

            if (Properties.Settings.Default.theme == "dark" || theme_combo.SelectedIndex == 2)
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
                    this.WindowStyle = WindowStyle.None;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 2);
                    break;

                case 1:
                    Dark_Theme();
                    this.Background = Brushes.Transparent;
                    this.WindowStyle = WindowStyle.None;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 3);
                    break;

                case 2:
                    Dark_Theme();
                    this.Background = Brushes.Transparent;
                    this.WindowStyle = WindowStyle.None;
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 4);
                    break;
            }
        }

        bool snakshow = false;

        private void MicaBoolCheked(object sender, RoutedEventArgs e)
        {
            if (MicaBool.IsChecked == true && snakshow)
            {
                Properties.Settings.Default.MicaBool = true;
                RootSnackbar.Show = true;
                stylewin_combo.SelectedIndex = 0;
                ApplyBackgroundEffect(0);

                Task.Run(async () =>
                {
                    await Task.Delay(5000);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RootSnackbar.Show = false;
                    });
                });
            }
            else
            {
                Properties.Settings.Default.MicaBool = false;
                WPFUI.Appearance.Background.Apply(new WindowInteropHelper(this).Handle, WPFUI.Appearance.BackgroundType.Mica);
                stylewin_combo.SelectedIndex = -1;
                stylewin_combo.SelectedIndex = 0;
                theme_combo.SelectedIndex = 1;
            }
            snakshow = true;
        }

        private void RootSnackbar_OnClosed(Snackbar snackbar)
        {
            RootSnackbar.Show = false;
        }

        private void Language_combo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Language_combo1.SelectedIndex == 0)
            {
                RussianInterfase_Settings();
            }
            if (Language_combo1.SelectedIndex == 1)
            {
                EnglishInterfase_Settings();
            }
        }

        private void StartupChencheOpen(object sender, RoutedEventArgs e)
        {
            // Запуск диагога выбора запускаемого обьекта.
            StartupDialog.Show = true;
            StartupDialog.Visibility = Visibility.Visible;
        }

        private void StartupDialogSave(object sender, RoutedEventArgs e)
        {
            if (StartModernNotify.IsChecked == true)
            {
                Properties.Settings.Default.Startup = "Panel";
                Properties.Settings.Default.Save();
            }
            if (StartMyDevice.IsChecked == true)
            {
                Properties.Settings.Default.Startup = "Connect";
                Properties.Settings.Default.Save();
            }
            StartupDialog.Show = false;
            StartupDialog.Visibility = Visibility.Hidden;
        }

        private void StartupDialogClose(object sender, RoutedEventArgs e)
        {
            StartupDialog.Show = false;
            StartupDialog.Visibility = Visibility.Hidden;

            StartModernNotify.IsChecked = false;
            StartMyDevice.IsChecked = false;

            if (Properties.Settings.Default.Startup == "Panel")
            {
                StartModernNotify.IsChecked = true;
            }
            if (Properties.Settings.Default.Startup == "Connect")
            {
                StartMyDevice.IsChecked = true;
            }
        }

        private void StartMN_Click(object sender, RoutedEventArgs e)
        {
            StartModernNotify.IsChecked = true;
            StartMyDevice.IsChecked = false;
        }

        private void StartMD_Click(object sender, RoutedEventArgs e)
        {
            StartModernNotify.IsChecked = false;
            StartMyDevice.IsChecked = true;
        }

        private void OpenUpdateModule(object sender, RoutedEventArgs e)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.Verb = "runas";
                p.StartInfo.FileName = "update.exe";
                p.Start();
            }
            catch
            {
                DisplayDialog("Невозможно запустить модуль", "Попробуйте проверить наличие обновлений, " +
                    "или запустите файл update.exe вручную.");
            }
        }

        private void ChangeLog_Open(object sender, RoutedEventArgs e)
        {
            DisplayDialog("Что нового в версии " + GetContent("https://unesell.com/api/version/modernnotify/version_dev.txt"), GetContent("https://unesell.com/api/version/modernnotify/changelog.txt"));
        }
    }
}