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

namespace ModernNotyfi.Other_Page
{
    /// <summary>
    /// Логика взаимодействия для DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
        }

        private void OpenSetupPage(object sender, RoutedEventArgs e)
        {
            welcome welcome = new welcome();
            welcome.Show();
        }

        private void OpenUnesellLogin(object sender, RoutedEventArgs e)
        {
            unesell_login_web unesell_Login_Web = new unesell_login_web();
            unesell_Login_Web.Show();
        }
    }
}
