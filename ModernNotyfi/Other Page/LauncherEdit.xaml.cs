using IWshRuntimeLibrary;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.UI.WindowManagement;
using Woof.SystemEx;

namespace ModernNotyfi.Other_Page
{
    /// <summary>
    /// Логика взаимодействия для DebugWindow.xaml
    /// </summary>
    public partial class LauncherEdit : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<ItemModel> items;
        public ObservableCollection<ItemModel> Items
        {
            get { return items; }
            set
            {
                if (items != value)
                {
                    items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }

        public ObservableCollection<ItemModel> ItemsApp { get; set; } = new ObservableCollection<ItemModel>();

        public LauncherEdit()
        {
            InitializeComponent();
            DataContext = this;

            WPFUI.Appearance.Background.Apply(this, WPFUI.Appearance.BackgroundType.Mica);
            UpdateList();
        }

        public void UpdateList()
        {
            string shortcutsFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shortcuts");
            if (!Directory.Exists(shortcutsFolderPath))
            {
                Directory.CreateDirectory(shortcutsFolderPath);
            }

            string[] shortcutFiles = Directory.GetFiles(shortcutsFolderPath);
            foreach (string shortcutFilePath in shortcutFiles)
            {
                ImageSource imageSource = null;

                FileInfo fileInfo = new FileInfo(shortcutFilePath);

                if (fileInfo.Extension.ToLower() == ".lnk")
                {
                    string targetPath = GetShortcutTarget(shortcutFilePath);
                    BitmapSource iconSource = GetFileIcon(targetPath);

                    if (iconSource != null)
                    {
                        imageSource = iconSource;
                    }
                }

                ItemsApp.Add(new ItemModel(fileInfo.Name.Replace(".lnk", ""), imageSource, fileInfo.FullName));
            }

            if (ItemsApp.Count > 0){
                NoApps.Visibility = Visibility.Collapsed;
            }
            else{ 
                NoApps.Visibility = Visibility.Visible;
            }
        }

        private string GetShortcutTarget(string shortcutFilePath)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath);
            string targetPath = shortcut.TargetPath;
            Marshal.ReleaseComObject(shortcut);
            Marshal.ReleaseComObject(shell);
            return targetPath;
        }

        private BitmapSource GetFileIcon(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            return null;
        }

        private void App_Delete(object sender, SelectionChangedEventArgs e)
        {
            if (Apps_List.SelectedIndex != -1)
            {
                var selectedItem = (ItemModel)Apps_List.SelectedItem;
                string message = $"Вы уверены, что хотите удалить ярлык для приложения '{selectedItem.Name}'?";
                MessageBoxResult result = MessageBox.Show(message, "Подтвердите удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        System.IO.File.Delete(selectedItem.Sourse);
                        ItemsApp.Remove(selectedItem);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            Apps_List.SelectedIndex = -1;
            ItemsApp.Clear();
            UpdateList();

            try
            {
                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.QuickLaunchItems.Clear();
                my.LauncherUpdate();
            }catch { }
        }

        private void AddApp(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string shortcutsFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shortcuts");
                if (!Directory.Exists(shortcutsFolderPath))
                {
                    Directory.CreateDirectory(shortcutsFolderPath);
                }

                string appFilePath = openFileDialog.FileName;
                string shortcutFilePath = System.IO.Path.Combine(shortcutsFolderPath, System.IO.Path.GetFileNameWithoutExtension(appFilePath) + ".lnk");

                WshShell wshShell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(shortcutFilePath);
                shortcut.TargetPath = appFilePath;
                shortcut.Save();

                // Освобождаем ресурсы
                Marshal.ReleaseComObject(shortcut);
                Marshal.ReleaseComObject(wshShell);

                // Обновляем листбокс или выполните другие действия, если необходимо
                ItemsApp.Clear();
                UpdateList();

                MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                my.QuickLaunchItems.Clear();
                my.LauncherUpdate();
            }
        }

    }

    public class ItemModel
    {
        public ItemModel(string name, ImageSource image, string sourse)
        {
            Name = name;
            Image = image;
            Sourse = sourse;
        }

        public string Name { get; set; }
        public string Sourse { get; set; }
        public ImageSource Image { get; set; }
    }
}
