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
using System.Windows.Threading;
using Woof.SystemEx;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для GameBar.xaml
    /// </summary>
    public partial class GameBar : Window
    {
        public DispatcherTimer timer = new DispatcherTimer();

        public GameBar()
        {
            InitializeComponent();
            ProblemDialog.Visibility = Visibility.Visible;
            ProblemDialog.Show = true;
            UserName.Text = Properties.Settings.Default.User_Name;
            var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
            AccauntImg.ImageSource = userBitmapSmall;

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                TimeClock.Text = DateTime.Now.ToString("HH:mm");
            };
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
