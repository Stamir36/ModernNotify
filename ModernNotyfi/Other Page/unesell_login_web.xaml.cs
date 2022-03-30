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
            WebLogin.Source = new Uri("https://unesell.com/login/?auth=yes&app=ModernNotify&service=app&data=2&icon=/assets/img/icons/mnlogo.png&out=http://app.modernnotify/");
        }

        private void WebLogin_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (Convert.ToString(WebLogin.Source).Contains("http://app.modernnotify/?id="))
            {
                string results = WebLogin.Source.ToString();
                results.Replace("http://app.modernnotify/?", ""); //Получаем: ключ=значение
                
                string[] response = results.Split('&');

                foreach (var sub in response)
                {
                    string data;
                    if (sub.Contains("id="))
                    {
                        data = sub.Replace("id=", "");           //id
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
    }
}
