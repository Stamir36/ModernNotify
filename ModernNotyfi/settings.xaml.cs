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

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для settings.xaml
    /// </summary>
    public partial class settings : Window
    {

        public settings()
        {
            InitializeComponent();
            if (Properties.Settings.Default.AllowsTransparency == true)
            {
                this.AllowsTransparency = true;
            }
            else
            {
                this.AllowsTransparency = false;
            }

            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
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

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            var userBitmapSmall = new BitmapImage(new Uri(SysInfo.GetUserPicturePath()));
            AccauntImg.ImageSource = userBitmapSmall;

            NameProfile.Content = "Привет, " + Properties.Settings.Default.User_Name;

            EctInfoSys.Content = "";

            Version.Content = "Версия приложения: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            version.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get()) { WindowsVersion.Content =  "Система: " + os["Caption"].ToString(); break; }
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
            }
            else
            {
                theme_combo.SelectedIndex = 1;
                Dark_Theme();
            }

            if (Properties.Settings.Default.posicion == "rigth")
            {
                pos_combo.SelectedIndex = 0;
            }
            else
            {
                pos_combo.SelectedIndex = 1;
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

            if (Properties.Settings.Default.AllowsTransparency == true)
            {
                Chek_Allow_Window.IsChecked = true;
            }
            else
            {
                Chek_Allow_Window.IsChecked= false;
            }
        }

        // ModernWpf.ThemeManager.GetActualTheme(this).ToString(); <- Тема установленная в системе
        public void Light_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F2FFFFFF"); // Белый фон
            Color_Border.Foreground = (Brush)bc.ConvertFrom("#F2343434");
            if (Color_Border.Text == "#404040") { Color_Border.Text = "#fff"; }

            ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
        }
        public void Dark_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F2343434"); // Чёрный фон
            Color_Border.Foreground = (Brush)bc.ConvertFrom("#F2FFFFFF");
            if (Color_Border.Text == "#fff") { Color_Border.Text = "#404040"; }

            ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
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
            NavView.SelectedItem = NavView.MenuItems[0];
            BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/maim_settings.png", UriKind.Relative); bi3.EndInit(); Sett_img.Source = bi3;
        }

        private void Open_Keyboard_Click(object sender, RoutedEventArgs e)
        {
            Settings_Tab.SelectedIndex = 2; Back.Click += Backs;
            Back.Visibility = Visibility.Visible;
            if (Language_combo1.SelectedIndex == 0)
            {
                Titles.Content = "Настройки > Мини-приложения";
            }
            else if (Language_combo1.SelectedIndex == 1)
            {
                Titles.Content = "Settings > Widgets";
            }
            NavView.SelectedItem = NavView.MenuItems[1];
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
            NavView.SelectedItem = NavView.MenuItems[2];
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
            NavView.SelectedItem = NavView.MenuItems[3];
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
                    Open_Main.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case "Keyboard":
                    Open_Keyboard.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case "Library":
                    Open_Personalization.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case "Help":
                    Open_Info.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
            }
        }

        int start_update = 0;
        private void Check_Update_Click(object sender, RoutedEventArgs e)
        {
            if (start_update == 0)
            {
                InfoUpdate.Content = "Проверка...";
                Properties.Settings.Default.last_check_update = DateTime.Now.ToString();
                Properties.Settings.Default.Save();
                LastChekUpdate.Content = "Последняя проверка: " + Properties.Settings.Default.last_check_update;

                //DEV VERSION CHECK
                if (GetContent("http://version-modernnotify.ml/modernnotify/version_dev.txt") == Assembly.GetExecutingAssembly().GetName().Version.ToString())
                {
                    InfoUpdate.Content = "У вас актуальная версия";
                    BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/ok.png", UriKind.Relative); bi3.EndInit();
                    Update_img.Source = bi3;
                }
                else
                {
                    if (GetContent("http://version-modernnotify.ml/modernnotify/version_dev.txt") != "Error")
                    {
                        InfoUpdate.Content = "Доступна новая версия " + GetContent("http://version-modernnotify.ml/modernnotify/version_dev.txt");
                        LastChekUpdate.Content = "Готовы к обновлению.";
                        Check_Update.Content = "Начать обновление";
                        BitmapImage bi3 = new BitmapImage(); bi3.BeginInit(); bi3.UriSource = new Uri("icons/restart_fluent.png", UriKind.Relative); bi3.EndInit();
                        Update_img.Source = bi3;
                        start_update = 1;
                    }
                    else
                    {
                        InfoUpdate.Content = "Нет подключения к интернету.";
                        LastChekUpdate.Content = "Мы не смогли получить ответ от сервера. Проверьте подключение.";
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
                InfoUpdate.Content = "Не удалось проверить обновления";
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
                
                if (theme_combo.SelectedIndex == 0) { Properties.Settings.Default.theme = "light"; }
                else { Properties.Settings.Default.theme = "black"; }

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

                if (Chek_Allow_Window.IsChecked == true)
                {
                    Properties.Settings.Default.AllowsTransparency = true;
                }
                else
                {
                    Properties.Settings.Default.AllowsTransparency = false;
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
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            catch
            {
                MessageBox.Show("Мы не смогли сохранить данные. Проверьте все изменённые настройки.", "Ошибка применения", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Open_Update.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void theme_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (theme_combo.SelectedIndex == 0)
            {
                Light_Theme();
            }
            else
            {
                Dark_Theme();
            }
        }

        private void Go_Tab_1_Click(object sender, RoutedEventArgs e)
        {
            Open_Main.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void Color_Save_Click(object sender, RoutedEventArgs e)
        {
            Color_Border.Text = color_piker.Text;
            Open_Personalization.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Back.Click += Backs;
        }

        public void Chenche_Color(object sender, RoutedEventArgs e)
        {
            color_piker.Text = Convert.ToString(((Button)sender).Tag);
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
            Process.Start("http://modernnotify.ml/");
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
            Titles.Content = "Настройки";
            localacc.Content = "Локальный аккаунт";
            text_7.Content = "Категории:";
            mainsett_text.Content = "Основные параметры";
            perssetttext.Content = "Персонализация";
            widgettmaintext.Content = "Мини-приложения";
            aboutstext.Content = "О программе";
            text_8.Content = "Прочее:";
            updatemainsett.Content = "Обновления программы";
            close.Content = "Закрыть окно";
            Save_Settings.Content = "Сохранить изменения";
            NameProfile.Content = "Привет, " + Properties.Settings.Default.User_Name;
            text_1.Content = "Основные параметры";
            funnanduse.Content = "Функции и взаимодействие";
            subbone.Content = "Панель выключения.";
            thametext.Content = "Тема приложения";
            text0213023.Content = "Местоположение панели";
            text3243.Content = "Показывать стартовое уведомление";
            textstyleshutdown.Content = "Стиль панели выключения";
            subbone.Content = "Панель выключения.";
            texts65464.Content = "Кнопка: Выключение приложения";
            text_2.Content = "Категории и разделы";
            twxtwidgwtwidgwtmenu.Content = "Мини-приложения";
            Back.Content = "< Назад";
            // Ревизия 1.
            text_3.Content = "Вид панелей:";
            pparone.Content = "Прозрачность";
            pcolortext.Content = "Цвет";
            Open_Color.Content = "Палитра >";
            pradiusboprder.Content = "Закруглённость углов";
            infotextpreview.Content = "Информация";
            text_3_Copy1.Content = "Вид окон";
            text23r923ri.Content = "Эффекты прозрачности:";
            otherptext.Content = "Прочее:";
            texte65930.Content = "Другие настройки:";
            Go_Tab_2.Content = "Тема приложения";
            Open_Tab_Update.Content = "Проверить обновления";
            Open_Site.Content = "Сайт проекта";
            textaboutproduct.Content = "О продукте:";
            text_5.Content = "Удобная и красивая панель быстрых действий на вашем \n компьютере под управлением Windows 10 и 11.";
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
        }

        public void EnglishInterfase_Settings()
        {
            // Revision 0
            Titles.Content = "Settings";
            localacc.Content = "Local account";
            text_7.Content = "Categories:";
            mainsett_text.Content = "Main parameters";
            perssetttext.Content = "Personalization";
            widgettmaintext.Content = "Widgets";
            aboutstext.Content = "About the program";
            text_8.Content = "Other:";
            updatemainsett.Content = "Software updates";
            close.Content = "Close";
            Save_Settings.Content = "Save changes";
            NameProfile.Content = "Hi, " + Properties.Settings.Default.User_Name + "!";
            text_1.Content = "Main parameters";
            funnanduse.Content = "Functions and interactions";
            subbone.Content = "Shutdown panel";
            thametext.Content = "Application theme";
            text0213023.Content = "Panel location";
            text3243.Content = "Show start notification";
            textstyleshutdown.Content = "Shutdown bar style";
            subbone.Content = "Shutdown panel.";
            texts65464.Content = "Button: Turn off the application";
            text_2.Content = "Categories and sections";
            twxtwidgwtwidgwtmenu.Content = "Widgets";
            Back.Content = "< Back";
            // Revision 1
            text_3.Content = "Panel style:";
            pparone.Content = "Transparency";
            pcolortext.Content = "Color";
            Open_Color.Content = "Palette >";
            pradiusboprder.Content = "Roundness of corners";
            infotextpreview.Content = "Information";
            text_3_Copy1.Content = "Window type";
            text23r923ri.Content = "Transparency Effects:";
            otherptext.Content = "Other:";
            texte65930.Content = "Other settings:";
            Go_Tab_2.Content = "Application theme";
            Open_Tab_Update.Content = "Check for updates";
            Open_Site.Content = "Project site";
            textaboutproduct.Content = "About the product:";
            text_5.Content = "Convenient and beautiful quick action bar on your \n computer running Windows 10 and 11.";
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
        }
    }
}