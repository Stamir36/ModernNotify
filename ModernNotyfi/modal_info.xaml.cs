using ModernWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для modal_info.xaml
    /// </summary>
    public partial class modal_info
    {

        public DispatcherTimer timer = new DispatcherTimer();


        public modal_info(string command)
        {
            InitializeComponent();
            ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            /* Правый верхний угол
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Top + 10;
            */

            this.Left = (desktopWorkingArea.Right / 2) - (this.Width / 2);
            this.Top = desktopWorkingArea.Bottom - 50;


            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                double value = 0;
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (command == "volume")
                {
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                    timer.IsEnabled = true;
                    timer.Tick += (o, t) =>
                    {
                        my.modal_show = true;
                        value = my.SoundSlider.Value;
                        SoundValues.Value = value;
                        LabelValueSound.Content = value;
                        my.close_time = my.close_time - 1;

                        if (my.close_time <= 0)
                        {
                            this.Close();
                        }
                    };
                }                
            }));


            //SoundValues
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.modal_show = false;
                timer.Stop();
            }));
        }
    }
}
