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
using Woof.SystemEx;

namespace ModernNotyfi.Other_Page
{
    /// <summary>
    /// Логика взаимодействия для DebugWindow.xaml
    /// </summary>
    public partial class UiLoginUnesell : Window
    {

        public string api = "http://api.unesell.com/";
        //public string api = "http://localhost/api/";

        public UiLoginUnesell()
        {
            InitializeComponent();
            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
        }

        private void OpenRegisterPage(object sender, RoutedEventArgs e)
        {
            Process.Start("https://unesell.com/registration/");
        }

        private void OpenWebLogin(object sender, RoutedEventArgs e)
        {
            unesell_login_web _Login_WebLogin = new unesell_login_web();
            _Login_WebLogin.Show();
            this.Close();
        }

        private void LoginCheckGO(object sender, RoutedEventArgs e)
        {
            string email = Login_Data_Mail.Text;
            string password = Login_Data_Password.Text;

            try
            {
                if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(password))
                {
                    // api + "applogin.php?email=" + "&password="
                    string responseString = string.Empty;
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                        responseString = webClient.DownloadString(api + "applogin.php?email=" + email + "&password=" + password);
                    }

                    if (responseString == "null")
                    {
                        RootSnackbar.Show = true;
                        RootSnackbar.Message = "Неверный логин или пароль, проверьте данные.";
                        Task.Run(async () =>
                        {
                            await Task.Delay(3000);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                RootSnackbar.Show = false;
                            });
                        });
                        return;
                    }
                    else
                    {
                        LoginInfo.Visibility = Visibility.Visible;


                        string id = Convert.ToString(JObject.Parse(responseString).SelectToken("id"));
                        string name = Convert.ToString(JObject.Parse(responseString).SelectToken("name"));
                        string emaile = Convert.ToString(JObject.Parse(responseString).SelectToken("email"));
                        string avatar = Convert.ToString(JObject.Parse(responseString).SelectToken("avatar"));

                        Properties.Settings.Default.Unesell_id = id;
                        Properties.Settings.Default.User_Name = name;
                        Properties.Settings.Default.Unesell_Email = emaile;
                        Properties.Settings.Default.Unesell_Avatar = avatar;
                        Properties.Settings.Default.Unesell_Login = "Yes";
                        Properties.Settings.Default.Save();

                        LoginCard.Visibility = Visibility.Hidden;
                        LoginCheck.Visibility = Visibility.Hidden;
                        Register.Visibility = Visibility.Hidden;

                        NameProfile.Content = "Привет, " + name;
                        try
                        {
                            Task.Run(() =>
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var userBitmapSmall = new BitmapImage(new Uri("https://unesell.com/data/users/avatar/" + avatar));
                                    AccauntImg.ImageSource = userBitmapSmall;
                                });
                            });
                            
                        }
                        catch
                        {
                            var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
                            AccauntImg.ImageSource = userBitmapSmall;
                        }


                        Task.Run(async () =>
                        {
                            await Task.Delay(5000);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.Close();
                            });
                        });
                    }

                    
                }
                else
                {
                    RootSnackbar.Show = true;
                    RootSnackbar.Message = "Вы ввели не все данные.";
                    Task.Run(async () =>
                    {
                        await Task.Delay(3000);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RootSnackbar.Show = false;
                        });
                    });
                }
            }
            catch
            {
                RootSnackbar.Show = true;
                RootSnackbar.Message = "Ошибка обратки. Проверьте интернет.";
                Task.Run(async () =>
                {
                    await Task.Delay(5000);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RootSnackbar.Show = false;
                    });
                });
            }
        }

        private void RootSnackbar_OnClosed(WPFUI.Controls.Snackbar snackbar)
        {
            RootSnackbar.Show = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginCard.Visibility = Visibility.Visible;
            LoginCheck.Visibility = Visibility.Visible;
            Register.Visibility = Visibility.Visible; 
            LoginInfo.Visibility = Visibility.Hidden;
        }
    }
}
