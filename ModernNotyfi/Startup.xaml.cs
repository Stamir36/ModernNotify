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
using WPFUI.Common;
using WPFUI;
using static ModernNotyfi.PInvoke.ParameterTypes;
using static ModernNotyfi.PInvoke.Methods;
using System.Windows.Interop;
using System.Management;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using static ModernNotyfi.settings;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для Startup.xaml
    /// </summary>
    public partial class Startup : Window
    {
        public Startup()
        {
            InitializeComponent();
            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom("#F21B1B1B");
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);
        }

        public async Task<bool> CheckInternetConnectionAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync("https://www.unesell.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private async void StartProgram(object sender, RoutedEventArgs e)
        {
            bool hasInternetConnection = await CheckInternetConnectionAsync();
            if (Properties.Settings.Default.Unesell_Login == "Yes" && hasInternetConnection)
            {
                await SettingSyncAsync(); // Интернет есть
            }
            else
            {
                await StartingAsync(); // Интернета нет
            } 
        }

        public async Task StartingAsync()
        {
            string proc = Process.GetCurrentProcess().ProcessName;
            Process[] processess = Process.GetProcessesByName(proc);

            if (processess.Length > 1)
            {
                MessageBox.Show("Экземпляр уже запущен. Нажмите Ctrl + Spase чтобы открыть панель.", "Панель уже активна.", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
            }
            else
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                foreach (ManagementObject os in searcher.Get()) { Properties.Settings.Default.System = os["Caption"].ToString(); break; }

                Properties.Settings.Default.System = Properties.Settings.Default.System.Replace("Майкрософт ", "");

                Properties.Settings.Default.Save();

                string UserData = "https://unesell.com/api/account.info.id.php?id=" + Properties.Settings.Default.Unesell_id;

                if (Properties.Settings.Default.Unesell_id != "")
                {
                    try
                    {
                        string responseString = string.Empty;
                        using (var webClient = new WebClient())
                        {
                            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                            responseString = await webClient.DownloadStringTaskAsync(new Uri(UserData));
                        }

                        if (responseString != "null")
                        {
                            string name = Convert.ToString(JObject.Parse(responseString).SelectToken("name"));
                            Properties.Settings.Default.User_Name = name;
                            Properties.Settings.Default.Save();
                        }
                    }
                    catch { }
                }

                if (Properties.Settings.Default.Startup == "Panel")
                {
                    if (Properties.Settings.Default.PanelStyle == 1)
                    {
                        MainWindow panel = new MainWindow();
                        panel.Show();
                    }
                    else if (Properties.Settings.Default.PanelStyle == 2)
                    {
                        ModernUI panel = new ModernUI();
                        panel.Show();
                    }
                    else
                    {
                        MainWindow panel = new MainWindow();
                        panel.Show();
                    }
                }
                else if (Properties.Settings.Default.Startup == "Connect")
                {
                    MyDevice connect = new MyDevice();
                    connect.Show();
                }
                else
                {
                    MessageBox.Show("Error", "Конфигурация установлена не правильно. Очистите настройки или поставте параметр 'Startup' на значение ''Panel''.");
                }

                try
                {
                    // Запуск сервисов.
                    ServiceMyDeviceNet serviceMyDeviceNet = new ServiceMyDeviceNet();
                    serviceMyDeviceNet.Show();
                }
                catch { }

                this.Close();
            }
        }

        public async Task SettingSyncAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://unesell.com/api/connect/");
                HttpResponseMessage response = await client.GetAsync("get_settings.php?user_id=" + Properties.Settings.Default.Unesell_id);

                if (response.IsSuccessStatusCode)
                {
                    string receivedJsonSettings = await response.Content.ReadAsStringAsync();
                    UserSettings receivedSettings = JsonConvert.DeserializeObject<UserSettings>(receivedJsonSettings);

                    // Сохранение полученных настроек в файл настроек
                    Properties.Settings.Default.opacity_panel = Convert.ToDouble(receivedSettings.opacity_panel);
                    Properties.Settings.Default.color_panel = receivedSettings.color_panel;
                    Properties.Settings.Default.theme = receivedSettings.theme;
                    Properties.Settings.Default.Show_Exit = receivedSettings.Show_Exit;
                    Properties.Settings.Default.Disign_Shutdown = receivedSettings.Disign_Shutdown;
                    Properties.Settings.Default.show_start_notify = Convert.ToBoolean(receivedSettings.show_start_notify);
                    Properties.Settings.Default.posicion = receivedSettings.posicion;
                    Properties.Settings.Default.CornerRadius = Convert.ToInt32(receivedSettings.CornerRadius);
                    Properties.Settings.Default.WinStyle = receivedSettings.WinStyle;
                    Properties.Settings.Default.Language = receivedSettings.Language;
                    Properties.Settings.Default.progressbarstyle = receivedSettings.progressbarstyle;
                    Properties.Settings.Default.ConnectMobile = receivedSettings.ConnectMobile;
                    Properties.Settings.Default.MicaBool = Convert.ToBoolean(receivedSettings.MicaBool);
                    Properties.Settings.Default.Startup = receivedSettings.Startup;
                    Properties.Settings.Default.PanelStyle = Convert.ToInt32(receivedSettings.PanelStyle);
                    Properties.Settings.Default.Save();

                    await StartingAsync();
                }
                else
                {
                    await StartingAsync();
                }
            }
        }
    }
}
