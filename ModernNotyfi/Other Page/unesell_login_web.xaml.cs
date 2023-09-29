using Microsoft.Web.WebView2.Core;
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
using System.Windows.Shapes;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для unesell_login_web.xaml
    /// </summary>
    public partial class unesell_login_web : Window
    {
        public unesell_login_web()
        {
            InitializeComponent();
            webView.NavigationStarting += EnsureHttps;
        }

        void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            String uri = args.Uri;
            if (uri.StartsWith("http://app.modernnotify/?id="))
            {
                string results = uri;
                results.Replace("http://app.modernnotify/?", ""); //Получаем: ключ=значение
                
                string[] response = results.Split('&');

                foreach (var sub in response)
                {
                    string data;
                    if (sub.Contains("id="))
                    {
                        data = sub.Replace("id=", "");           //id
                        data = data.Replace("http://app.modernnotify/?", "");
                        Properties.Settings.Default.Unesell_id = data;
                        Properties.Settings.Default.Save();
                    }
                    if (sub.Contains("name="))
                    {
                        data = sub.Replace("name=", "");         //name
                        Properties.Settings.Default.User_Name = data;
                        Properties.Settings.Default.Save();
                    }
                    if (sub.Contains("email="))
                    {
                        data = sub.Replace("email=", "");        //email
                        Properties.Settings.Default.Unesell_Email = data;
                        Properties.Settings.Default.Save();
                    }
                    if (sub.Contains("avatar="))
                    {
                        data = sub.Replace("avatar=", "");       //avatar
                        Properties.Settings.Default.Unesell_Avatar = data;
                        Properties.Settings.Default.Save();
                    }
                }
                Properties.Settings.Default.Unesell_Login = "Yes";
                Properties.Settings.Default.Save();

                this.Close();
            }
        }

        private void OpeUiLogin(object sender, RoutedEventArgs e)
        {
            Other_Page.UiLoginUnesell uiLoginUnesell = new Other_Page.UiLoginUnesell();
            uiLoginUnesell.Show();
            this.Close();
        }
    }
}
