using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using IWshRuntimeLibrary;

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

            Path_textbox.Text = "C:/Program Files (x86)/ModernNotify/";
            if (CheckForInternetConnection() == false)
            {
                install_process = 3;
                Close.Visibility = Visibility.Hidden;
                Install.Content = "Закрыть установщик";
                InfoUpdate.Content = "Нет подключения к интернету.";
                SubText.Content = "Сервер недоступен. Скачивание программы невозможно.";
            }
        }

        public void download_update() // Скачивание обнновления
        {
            try
            {
                string link = @"https://unesell.com/api/version/modernnotify/update_dev.zip";
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (o, args) => ProgressDownload.Value = args.ProgressPercentage;
                webClient.DownloadFileCompleted += (o, args) => download_updater();
                webClient.DownloadFileAsync(new Uri(link), Path_textbox.Text + "update.zip");
            }
            catch
            {
                InfoUpdate.Content = "Загрузка файлов не удалась.";
                SubText.Content = "Проверте подключение или попробуйте позже.";
                Close.Content = "Выход";
            }
        }

        public void download_updater()
        {
            InfoUpdate.Content = "Загрузка модуля обновления...";
            try
            {
                string link = @"https://unesell.com/api/version/modernnotify/update.exe";
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (o, args) => ProgressDownload.Value = args.ProgressPercentage;
                webClient.DownloadFileCompleted += (o, args) => update();
                webClient.DownloadFileAsync(new Uri(link), Path_textbox.Text + "update.exe");
            }
            catch
            {
                InfoUpdate.Content = "Загрузка модуля не удалась.";
                SubText.Content = "Проверте подключение или попробуйте позже.";
            }
        }

        public void update() // Распаковка файлов программы
        {
            ProgressDownload.Value = 100;
            InfoUpdate.Content = "Установка ModernNotify...";

            string zipPath = Path_textbox.Text + "update.zip";
            string mainPath = Path_textbox.Text;

            //Расспаковка
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
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
            catch
            {
                InfoUpdate.Content = "Произошла ошибка.";
                SubText.Content = "Пропробуйте ещё раз.";
                Close.Content = "Выход";
                throw;
            }
            finally
            {
                InfoUpdate.Content = "Завершение установки...";
                SubText.Content = "Удаление временных файлов.";
                System.IO.File.Delete(zipPath);

                InfoUpdate.Content = "ModernNotify установлена на ваш ПК!.";
                Install.Content = "Запустить приложение";
                Close.Content = "Закрыть установщик";
                SubText.Content = "Спасибо, что используете продукы Unesell Studio!";
                install_process = 2;
                ProgressDownload.Visibility = Visibility.Hidden;
                Install.IsEnabled = true;
                AddShortcut();
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
                System.IO.File.Delete("update.zip");
            }
            catch
            {
                // none
            }
            System.Windows.Application.Current.Shutdown();
        }

        int install_process = 0;
        public static string path_exe;

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (install_process == 0)
            {
                path_grid.Visibility = Visibility.Visible;
                install_process = 1;
                Install.Content = "Начать установку";
                InfoUpdate.Content = "Куда установить панель?";
                SubText.Content = "Выберите место на вашем накопителе.";
                return;
            }
            if (install_process == 1)
            {
                path_exe = Path_textbox.Text;
                DirectoryInfo di = Directory.CreateDirectory(Path_textbox.Text);
                path_grid.Visibility = Visibility.Hidden;
                Install.IsEnabled = false;
                Install.Content = "Подождите...";

                try
                {
                    url = "https://unesell.com/api/version/modernnotify/version_dev.txt";

                    InfoUpdate.Content = "Загрузка файлов программы...";
                    SubText.Content = "Версия: " + GetContent(url);
                    ProgressDownload.Visibility = Visibility.Visible;
                    download_update();
                    return;
                }
                catch
                {
                    InfoUpdate.Content = "Подключение к серверу не удалось.";
                    SubText.Content = "Проверте подключение или попробуйте позже.";
                    Close.Content = "Выход";
                    return;
                }
            }
            if (install_process == 2)
            {
                Process.Start(Path_textbox.Text + "ModernNotify.exe");
                System.Windows.Application.Current.Shutdown();
            }
            if (install_process == 3)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void CheckPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Path_textbox.Text = fbd.SelectedPath + "\\ModernNotify\\";
            }
        }



        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("https://unesell.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void AddShortcut()
        {
            string pathToExe = path_exe + "ModernNotify.exe";

            // For Start
            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs");

            if (!Directory.Exists(appStartMenuPath))
                Directory.CreateDirectory(appStartMenuPath);

            string shortcutLocation = Path.Combine(appStartMenuPath, "ModernNotify" + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "ModernNotify";
            shortcut.TargetPath = pathToExe;
            shortcut.Save();

            //For Desktop
            string appDesktopMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (!Directory.Exists(appDesktopMenuPath))
                Directory.CreateDirectory(appDesktopMenuPath);

            shortcutLocation = Path.Combine(appDesktopMenuPath, "ModernNotify" + ".lnk");
            IWshShortcut shortcut_desktop = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut_desktop.Description = "ModernNotify";
            shortcut_desktop.TargetPath = pathToExe;
            shortcut_desktop.Save();
        }
    }
}
