using Microsoft.Win32;
using ModernWpf;
using QRCoder;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
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
using System.Windows.Threading;
using Woof.SystemEx;
using WPFUI;
using WPFUI.Controls;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для welcome.xaml
    /// </summary>
    public partial class welcome : Window
    {
        public welcome()
        {
            try
            {
                WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
                InitializeComponent();
                var bc = new BrushConverter();
                this.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#F21B1B1B");
                WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);
                FirsLoading.Visibility = Visibility.Visible;
                Step_Text.Visibility = Visibility.Hidden;
                WelcomTabs.Visibility = Visibility.Hidden;

                languege_combo.SelectedIndex = 0;

                Task.Run(async () =>
                {
                    await Task.Delay(2000);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FirsLoading.Visibility = Visibility.Hidden;
                        Step_Text.Visibility = Visibility.Visible;
                        WelcomTabs.Visibility = Visibility.Visible;
                    });
                });

                GifImage_Step_2.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            
        }

        private void Step_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WelcomTabs.SelectedIndex = 1;
                if (languege_combo.SelectedIndex == 0){ Step_Text.Content = "О себе"; } else{ Step_Text.Content = "About myself"; }
                try
                {
                    User_Name.Text = SysInfo.LogonUser.FullName;
                }
                catch
                {
                    if (languege_combo.SelectedIndex == 0) { User_Name.Text = "Пользователь"; } else { User_Name.Text = "User"; }
                }

                languege_combo.SelectedIndex = 0;

                Step_Progress.Value = 10;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Start_App_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //OOBE Close
                Properties.Settings.Default.First_Settings = false;
                Properties.Settings.Default.Save();

                MainWindow main = new MainWindow();
                main.Show();
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Step_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (languege_combo.SelectedIndex == 1)
                {
                    Properties.Settings.Default.Language = "English";
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.Language = "Russian";
                    Properties.Settings.Default.Save();
                }

                LOG.Content = "";
                string name = User_Name.Text;
                if (name.Length >= 4)
                {
                    GifImage_Step_1.Visibility = Visibility.Hidden;
                    GifImage_Step_2.Visibility = Visibility.Hidden;

                    BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/gif/geo_icons.png", UriKind.Relative); bi3.EndInit();
                    Step_Image.Source = bi3;

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode("https://play.google.com/store/apps/details?id=com.unesell.mnc", QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new QRCode(qrCodeData);
                    Bitmap qrCodeImage = qrCode.GetGraphic(20);

                    QRcode.Source = BitmapToImageSource(qrCodeImage);

                    WelcomTabs.SelectedIndex = 4;

                    Properties.Settings.Default.User_Name = User_Name.Text;
                    Properties.Settings.Default.Save();
                    if (languege_combo.SelectedIndex == 0) { Step_Text.Content = "Связь с экосистемой"; } else { Step_Text.Content = "Link with Android"; }
                    
                    Step_Progress.Value = 20;
                }
                else
                {
                    if (languege_combo.SelectedIndex == 0) { LOG.Content = "Пожалуйста, введите ваше имя (Больше 4 символов)."; } else { LOG.Content = "Please enter your name (More than 4 characters)."; }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void Steap_4(object sender, RoutedEventArgs e)
        {
            try
            {
                if (languege_combo.SelectedIndex == 0) { Step_Text.Content = "Базовая настройка"; } else { Step_Text.Content = "Basic setup"; }
                Step_Progress.Value = 90;
                WelcomTabs.SelectedIndex = 3;
                theme_combo.SelectedIndex = 0;
                pos_combo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Save_Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (theme_combo.SelectedIndex == 0)
                {
                    Properties.Settings.Default.theme = "light";
                    Properties.Settings.Default.color_panel = "#FFFFFFFF";
                }
                else
                {
                    Properties.Settings.Default.theme = "black";
                    Properties.Settings.Default.color_panel = "#404040";
                }

                if (pos_combo.SelectedIndex == 0) { Properties.Settings.Default.posicion = "rigth"; }
                else { Properties.Settings.Default.posicion = "left"; }

                string ExePath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\ModernNotify.exe";

                string name = "ModernNotify";
                RegistryKey reg;
                reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");

                if ((bool)StartInWindows.IsChecked)
                {

                    try
                    {
                        reg.SetValue(name, ExePath);
                        reg.Close();
                    }
                    catch
                    {
                        StartInWindows.IsChecked = false;
                    }
                }
                else
                {
                    try
                    {
                        reg.DeleteValue(name);
                        reg.Close();
                    }
                    catch
                    {
                        StartInWindows.IsChecked = false;
                    }
                }

                //OOBE Close
                Properties.Settings.Default.First_Settings = false;
                Properties.Settings.Default.Save();

                MainWindow main = new MainWindow();
                main.Show();
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void languege_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (languege_combo.SelectedIndex == 0)
            {
                Step2_text1.Content = "Давай создадим тебе профиль.";
                Step2_text2.Text = "Ваш никнейм";
                Step_2.Content = "Продолжить";

                Step3_text1.Content = "Настроим приложение?";
                Step3_text2.Content = "Всё настроить под себя вы сможете в настройках.";
                Step3_b1.Content = "Настроить параметры";
                Step3_b2.Content = "Использовать по умолчанию";

                Save_Settings.Content = "Принять и завершить";
                text_1.Content = "Основные параметры";
                text_1_Copy.Content = "Прочее";
                Step4_text1.Text = "Тема приложения";
                Step4_text2.Text = "Расположение панели";
                StartInWindows.Content = "Запуск при старте системы";

                MNconnect_text1.Content = "Связь с телефоном Android";
                MNConnect_text2.Content = "Установите приложение MN Connect из Play Маркета.";
                or_text.Content = "Или";
                HyperLincAndroid.Content = "MN Connect - Клиент на Android";
                Step_1_Copy.Content = "Пропустить";
                Step_1_Copy1.Content = "Пропустить";

                Account_text1.Content = "Связь с учётной записью";
                Account_text2.Content = "Можно войти позже в настройках";
                Account_text3.Content = "Войдите в свой аккаунт, чтобы синхронизировать аватар\nи имя вашей учётной записи с приложением.";
                Login_Unesell.Content = "Войти в Unesell Аккаунт";
                Step_Text.Content = "О себе";
            }
            else
            {
                Step_Text.Content = "About myself";
                Step2_text1.Content = "Let's create a profile for you.";
                Step2_text2.Text = "Your nickname";
                Step_2.Content = "Continue";

                Step3_text1.Content = "Setup an application?";
                Step3_text2.Content = "You can customize everything in the settings.";
                Step3_b1.Content = "Customize settings";
                Step3_b2.Content = "Use by default";

                Save_Settings.Content = "Accept and complete";
                text_1.Content = "Main settings";
                text_1_Copy.Content = "Other";
                Step4_text1.Text = "App Theme";
                Step4_text2.Text = "Panel Location";
                StartInWindows.Content = "Run at system startup";

                MNconnect_text1.Content = "Communication with an Android phone";
                MNConnect_text2.Content = "Install the MN Connect app from the Play Store.";
                or_text.Content = "Or";
                HyperLincAndroid.Content = "MN Connect - Link on Android";
                Step_1_Copy.Content = "Skip";
                Step_1_Copy1.Content = "Skip";

                Account_text1.Content = "Linking to an account";
                Account_text2.Content = "Can be logged in later in settings";
                Account_text3.Content = "Sign in to your account to sync your avatar\nand your account name with the app.";
                Login_Unesell.Content = "Login to Unesell Account";
            }
        }

        private void Step3_Open(object sender, RoutedEventArgs e)
        {
            WelcomTabs.SelectedIndex = 2;
            if (languege_combo.SelectedIndex == 0) { Step_Text.Content = "Настроим под себя"; } else { Step_Text.Content = "Customize for yourself"; }
            Step_Progress.Value = 80;
            GifImage_Step_2.Visibility = Visibility.Visible;
            Step_Image.Visibility = Visibility.Hidden;
            timer_sec.Stop();
        }

        public DispatcherTimer timer_sec = new DispatcherTimer();

        private void Step_Account_Click(object sender, RoutedEventArgs e)
        {
            WelcomTabs.SelectedIndex = 5;
            if (languege_combo.SelectedIndex == 0) { Step_Text.Content = "Unesell Аккаунт"; } else { Step_Text.Content = "Unesell Account"; }
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/gif/accountU.png", UriKind.Relative); bi3.EndInit();
            Step_Image.Source = bi3;
            Step_Progress.Value = 50;
            timer_sec.Interval = new TimeSpan(0, 0, 1);
            timer_sec.IsEnabled = true;
            timer_sec.Tick += (o, t) =>
            {
                updateaccount();
            };
            timer_sec.Start();
        }

        private void LoginWebUnesell(object sender, RoutedEventArgs e)
        {
            unesell_login_web unesell_Login_Web = new unesell_login_web();
            unesell_Login_Web.Show();
        }

        bool no_login = true;

        public void updateaccount()
        {
            if (Properties.Settings.Default.Unesell_Login == "Yes")
            {
                if (no_login)
                {
                    WelcomTabs.SelectedIndex = 2;
                    if (languege_combo.SelectedIndex == 0) { Step_Text.Content = "Настроим под себя"; } else { Step_Text.Content = "Customize for yourself"; }
                    Step_Progress.Value = 80;
                    GifImage_Step_2.Visibility = Visibility.Visible;
                    no_login = false;
                    timer_sec.Stop();
                }
            }
        }

        private void CardControl_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
