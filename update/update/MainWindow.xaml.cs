using ModernWpf;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace update
{
    public partial class MainWindow
    {
        public string program_version;
        public string program_relise = "ModernNotyfi Dev";
        public string url = "";
        public string new_version;

        public MainWindow()
        {
            InitializeComponent();
            ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
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

            try
            {
                url = "http://version-modernnotify.ml/modernnotify/version_dev.txt";
                if (Properties.Settings.Default.Server == "GitHub")
                {
                    url = @"https://github.com/Stamir36/ModernNotyfi/raw/main/server/modernnotify/version_dev.txt";
                }

                if (GetContent(url) == program_version)
                {
                    InfoUpdate.Content = "У вас установлена актуальная версия";
                    SubText.Content = "Обновление не требуется";
                }
                else
                {
                    Settings.Visibility = Visibility.Hidden;
                    InfoUpdate.Content = "Загрузка обновления...";
                    SubText.Content = "Новая версия: " + GetContent(url);
                    ProgressDownload.Visibility = Visibility.Visible;
                    new_version = GetContent(url);
                    download_update();
                }
            }
            catch
            {
                InfoUpdate.Content = "Проверка обновлений не удалась.";
                SubText.Content = "Проверте подключение или попробуйте позже.";
                Close.Content = "Выход";
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

        public void download_update() // Скачивание обнновления
        {
            try
            {
                string link = @"http://version-modernnotify.ml/modernnotify/update_dev.zip";
                if (Properties.Settings.Default.Server == "GitHub")
                {
                    link = @"https://github.com/Stamir36/ModernNotyfi/raw/main/server/modernnotify/update_dev.zip";
                }

                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (o, args) => { ProgressDownload.Value = args.ProgressPercentage; SubText.Content = "Прогресс: " + args.ProgressPercentage + "% | Версия: " + new_version; };
                webClient.DownloadFileCompleted += (o, args) => update();
                webClient.DownloadFileAsync(new Uri(link), "update.zip");
            }
            catch
            {
                InfoUpdate.Content = "Загрузка обновления не удалась.";
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
            
            string zipPath = "update.zip";
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
                            entry.ExtractToFile(Path.Combine(mainPath, entry.FullName), true); // Расспаковка с заменой
                        }
                    }
                }
            }
            catch
            {
                InfoUpdate.Content = "Произошла ошибка.";
                SubText.Content = "Пропробуйте ещё раз.";
                Close.Content = "Выход";
                throw;
            }
            finally
            {
                InfoUpdate.Content = "Завершение обновления.";
                SubText.Content = "Удаление временных файлов.";
                File.Delete(zipPath);

                InfoUpdate.Content = "Обновление завершено.";
                SubText.Content = "Запуск программы.";
                Process.Start("ModernNotify.exe"); // Запуск программы
                Application.Current.Shutdown(); // Выход
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
                File.Delete("update.zip");
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
    }
}
