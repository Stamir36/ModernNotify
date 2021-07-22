using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
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
    /// Логика взаимодействия для welcome.xaml
    /// </summary>
    public partial class welcome : Window
    {
        public welcome()
        {
            InitializeComponent();
            GifImage_Step_2.Visibility = Visibility.Hidden;
        }

        private void Step_1_Click(object sender, RoutedEventArgs e)
        {
            WelcomTabs.SelectedIndex = 1;
            Step_Text.Content = "О себе";
            try
            {
                User_Name.Text = UserPrincipal.Current.DisplayName;
            }
            catch
            {
                User_Name.Text = "Пользователь";
            }
            
        }

        private void Start_App_Click(object sender, RoutedEventArgs e)
        {
            //OOBE Close
            MainWindow main = new MainWindow();
            main.Show();
            Close();
        }

        private void Step_2_Click(object sender, RoutedEventArgs e)
        {
            string name = User_Name.Text;
            if (name.Length >= 4)
            {
                GifImage_Step_1.Visibility = Visibility.Hidden;
                GifImage_Step_2.Visibility = Visibility.Visible;
                WelcomTabs.SelectedIndex = 2;

                Properties.Settings.Default.User_Name = User_Name.Text;
                Properties.Settings.Default.Save();

                Step_Text.Content = "Настроим под себя";
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите ваше имя (Больше 4 символов, у вас: " + name.Length + ").", "Нет имени");
            }
            
        }
    }
}
