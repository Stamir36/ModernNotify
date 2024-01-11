using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using static update.PInvoke.ParameterTypes;
using static update.PInvoke.Methods;
using System.Windows.Interop;
using System.Windows.Media;
using ModernWpf;
using System.Drawing;
using System.Threading.Tasks;
using System.Management;

namespace update
{
    public partial class MainWindow
    {
        public string program_version;
        public string program_relise = "ModernNotyfi Dev";
        public string url = "";
        public string new_version;
        string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";


        public MainWindow()
        {
            InitializeComponent();
        }

        public static bool CheckForInternetConnection() //Проверка интернета
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("https://unesell.com/"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void download_update() // Скачивание обнновления
        {
            try
            {
                string link = @"https://unesell.com/api/version/modernnotify/update_dev.zip";
                if (Properties.Settings.Default.Server == "GitHub")
                {
                    link = @"https://github.com/Stamir36/ModernNotyfi/raw/main/server/modernnotify/update_dev.zip";
                }

                WebClient webClient = new WebClient();
                DateTime startTime = DateTime.Now;
                long fileSize = 0;
                DownloadSpeed.Visibility = Visibility.Visible;
                webClient.DownloadProgressChanged += (o, args) =>
                {
                    ProgressDownload.Value = args.ProgressPercentage;
                    SubText.Content = "Прогресс: " + args.ProgressPercentage + "% | Версия: " + new_version;
                    fileSize = args.TotalBytesToReceive;
                    double speed = args.BytesReceived / (DateTime.Now - startTime).TotalSeconds;
                    string speedUnit = "кб/с";
                    if (speed > 1024)
                    {
                        speed /= 1024;
                        speed /= 1024;
                        speedUnit = "мб/с";
                    }
                    speed = Math.Round(speed, 2);
                    DownloadSpeed.Content = speed + " " + speedUnit;
                };

                webClient.DownloadFileCompleted += (o, args) =>
                {
                    update();
                    DownloadSpeed.Visibility = Visibility.Hidden;
                };
                webClient.DownloadFileAsync(new Uri(link), path + "update.zip");
            }
            catch
            {
                InfoUpdate.Content = "Загрузка обновления не удалась.";
                Log.Content = "";
                SubText.Content = "Проверте подключение или попробуйте позже.";
                Close.Content = "Выход";
            }
        }

        private void download_mnconnect(object sender, RoutedEventArgs e) // Скачивание MN Connect
        {
            InfoUpdate.Content = "Скачивание MN Connect.";
            SubText.Content = "Загрузка...";
            Page.SelectedIndex = 0;
            ProgressDownload.Visibility = Visibility.Visible;
            try
            {
                string link = @"https://github.com/Stamir36/ModernNotyfi/raw/main/mnconnect.apk";
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (o, args) => { ProgressDownload.Value = args.ProgressPercentage; SubText.Content = "Прогресс: " + args.ProgressPercentage + "%"; };
                webClient.DownloadFileCompleted += (o, args) => {
                    InfoUpdate.Content = "MN Connect скачен.";
                    SubText.Content = "Установщик находится в папке программы.";
                };
                webClient.DownloadFileAsync(new Uri(link), "mnconnect.apk");
            }
            catch
            {
                InfoUpdate.Content = "Загрузка MN Connect не удалась.";
                SubText.Content = "Проверте подключение или попробуйте позже.";
                Close.Content = "Выход";
            }
        }

        public void update() // Распаковка обновления
        {
            ProgressDownload.Value = 100;
            InfoUpdate.Content = "Установка обновления.";
            
            string zipPath = path + "update.zip";
            string mainPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            //Расспаковка
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName != "ModernWpf.dll" && entry.FullName != "ModernWpf.Controls.dll" && entry.FullName != "update.exe")
                        {
                            string entryPath = Path.Combine(mainPath, entry.FullName);
                            string entryDirectory = Path.GetDirectoryName(entryPath);
                            Directory.CreateDirectory(entryDirectory);
                            if (!entry.FullName.EndsWith("/"))
                            {
                                entry.ExtractToFile(entryPath, true);
                            }
                        }
                    }
                }
            }
            catch
            {
                InfoUpdate.Content = "Произошла ошибка.";
                Log.Content = "";
                SubText.Content = "Пропробуйте ещё раз.";
                Close.Content = "Выход";
            }
            finally
            {
                InfoUpdate.Content = "Завершение обновления...";
                Log.Content = "";
                SubText.Content = "Удаление временных файлов...";
                File.Delete(zipPath);

                Task.Run(async () =>
                {
                    await Task.Delay(2000);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Process.Start("ModernNotify.exe"); // Запуск программы
                        Application.Current.Shutdown(); // Выход
                    });
                });

                InfoUpdate.Content = "Обновление завершено!";
                SubText.Content = "Запуск программы...";
            }
        }

        public static string GetContent(string url) // Получение информации с файла сервера.
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
            catch (Exception e)
            {
                return Convert.ToString(e);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.Delete(path + "update.zip");
            }
            catch
            {
                // none
            }
            Application.Current.Shutdown();
        }

        private void Open_Settings(object sender, RoutedEventArgs e)
        {
            Page.SelectedIndex = 1;
        }

        private void Update_Open(object sender, RoutedEventArgs e)
        {
            Page.SelectedIndex = 0;
        }

        private void theme_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(theme_combo.SelectedIndex == 0)
            {
                Properties.Settings.Default.Server = "Site";
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Server = "GitHub";
                Properties.Settings.Default.Save();
            }
        }

        private void update_window_Loaded(object sender, RoutedEventArgs e)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get()) {

                if (os["Caption"].ToString().Contains("Windows 11"))
                {
                    ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);
                    RefreshFrame();
                    RefreshDarkMode();
                    SetWindowAttribute(new WindowInteropHelper(this).Handle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 2);
                }
                else
                {
                    this.Background = System.Windows.Media.Brushes.White;
                    ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
                }
                //MessageBox.Show(os["Caption"].ToString(), "О системе");
                break; 
            }

            Task.Run(async () =>
            {
                await Task.Delay(500);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StartCheckUpdate();
                });
            });
        }

        public void StartCheckUpdate()
        {
            UVersion.Content = "Установщик: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            try
            {
                FileVersionInfo.GetVersionInfo("ModernNotify.exe");
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo("ModernNotify.exe");
                program_version = myFileVersionInfo.FileVersion;
                PVersion.Content = "Приложение: " + myFileVersionInfo.FileVersion;
                program_relise = myFileVersionInfo.ProductName;
            }
            catch
            {
                PVersion.Content = "Приложение: " + "Не установлено.";
                program_version = "0";
            }

            if (CheckForInternetConnection())
            {
                try
                {
                    url = "https://unesell.com/api/version/modernnotify/version_dev.txt";
                    if (Properties.Settings.Default.Server == "GitHub")
                    {
                        url = @"https://github.com/Stamir36/ModernNotyfi/raw/main/server/modernnotify/version_dev.txt";
                    }

                    if (GetContent(url) == program_version)
                    {
                        InfoUpdate.Content = "У вас установлена актуальная версия";
                        Log.Content = "Всё в порядке, спасибо, что используете ModernNotify!";
                        SubText.Content = "Обновление не требуется";
                    }
                    else
                    {
                        Settings.Visibility = Visibility.Hidden;
                        InfoUpdate.Content = "Загрузка обновления...";
                        Log.Content = "Пожалуйста, подождите...";
                        SubText.Content = "Новая версия: " + GetContent(url);
                        ProgressDownload.Visibility = Visibility.Visible;
                        new_version = GetContent(url);
                        download_update();
                    }
                }
                catch
                {
                    InfoUpdate.Content = "Проверка обновлений не удалась.";
                    Log.Content = "";
                    SubText.Content = "Проверте подключение или попробуйте позже.";
                    Close.Content = "Выход";
                }
            }
            else
            {
                InfoUpdate.Content = "Нет соединения с интернетом.";
                Log.Content = "Проверьте настройки WiFi или кабеля интернет.\nЕсли всё в порядке, вы можете скачать новое\nобновление напрямую с сайта приложения.";
                SubText.Content = "Попробуйте запустить:";
                Close.Content = "Выход";
                NetworkFix.Visibility = Visibility.Visible;
            }

            if (Properties.Settings.Default.Server == "Site")
            {
                theme_combo.SelectedIndex = 0;
            }
            else
            {
                theme_combo.SelectedIndex = 1;
            }
        }

        private void RefreshFrame()
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.CompositionTarget.BackgroundColor = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);

            System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
            float DesktopDpiX = desktop.DpiX;

            MARGINS margins = new MARGINS();
            margins.cxLeftWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
            margins.cxRightWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
            margins.cyTopHeight = Convert.ToInt32(((int)ActualHeight + 5) * (DesktopDpiX / 96));
            margins.cyBottomHeight = Convert.ToInt32(5 * (DesktopDpiX / 96));

            ExtendFrame(mainWindowSrc.Handle, margins);
        }

        private void RefreshDarkMode()
        {
            var isDark = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark;
            int flag = isDark ? 1 : 0;
            SetWindowAttribute(
                new WindowInteropHelper(this).Handle,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                flag);
        }

        private void NetworkFixStart(object sender, RoutedEventArgs e)
        {
            Process.Start("msdt.exe", "-skip TRUE -path C:\\Windows\\diagnostics\\system\\Networking");
        }
    }
}
