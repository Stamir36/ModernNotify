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

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                FileVersionInfo.GetVersionInfo("ModernNotify.exe");
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo("ModernNotify.exe");
                program_version = myFileVersionInfo.FileVersion;
                program_relise = myFileVersionInfo.ProductName;
            }
            catch
            {
                program_version = "0";
            }

            try
            {
                url = "http://version-modernnotify.ml/modernnotify/version_dev.txt";

                if (GetContent(url) == program_version)
                {
                    InfoUpdate.Content = "У вас установлена актуальная версия";
                    SubText.Content = "Обновление не требуется";
                }
                else
                {
                    InfoUpdate.Content = "Загрузка обновления...";
                    SubText.Content = "Новая версия: " + GetContent(url);
                    ProgressDownload.Visibility = Visibility.Visible;
                    download_update();
                }
            }
            catch
            {
                InfoUpdate.Content = "Проверка обновлений не удалась.";
                SubText.Content = "Проверте подключение или попробуйте позже.";
                Close.Content = "Выход";
            }
        }

        public void download_update() // Скачивание обнновления
        {
            try
            {
                string link = @"http://version-modernnotify.ml/modernnotify/update_dev.zip";
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (o, args) => ProgressDownload.Value = args.ProgressPercentage;
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
    }
}
