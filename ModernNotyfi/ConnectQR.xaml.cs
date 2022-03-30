using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
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
using Windows.UI.Xaml.Controls;

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для ConnectQR.xaml
    /// </summary>
    public partial class ConnectQR : Window
    {
        String CONNECT_IDENTIFY = "MODERNCONNECT:";
        public DispatcherTimer timer_sec = new DispatcherTimer();

        public ConnectQR()
        {
            InitializeComponent();

            if (Properties.Settings.Default.Language == "English")
            {
                ConnectPhoneText.Content = "Connecting a phone";
                ScanAppMobile.Content = "Scan the code with the mobile app ";
                context.Content = "Use the MN Connect App";
                info.Content = "Waiting for connection...";
                Close.Content = "Cancel connection";
            }

            code.Content = GetMotherBoardID();

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(CONNECT_IDENTIFY + GetMotherBoardID(), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            QRcode.Source = BitmapToImageSource(qrCodeImage);


            if (Properties.Settings.Default.theme == "light")
            { Light_Theme(); }
            else
            { Dark_Theme(); }

            // Проверка подключения.
            timer_sec.Interval = new TimeSpan(0, 0, 1);
            timer_sec.IsEnabled = true;
            timer_sec.Tick += (o, t) =>
            {
                ConnectQRCheck();
            };
            timer_sec.Start();
        }

        int connectOK = 0;

        public async void ConnectQRCheck()
        {
            string responseString = string.Empty;

            try
            {
                await Task.Run(() =>
                {
                    using (var webClient = new WebClient())
                    {
                        responseString = webClient.DownloadString("http://api.unesell.com/connect/check_connect.php?id=" + GetMotherBoardID());
                    }
                });


                Task.WaitAll();
                info.Content = "Ожидаем подключение...";

                if (responseString != "null")
                {
                    if (connectOK == 0)
                    {
                        info.Content = "Подкючение...";
                        var mobile = JObject.Parse(responseString).SelectToken("MOBILE");
                        info.Content = "Соединение с " + Convert.ToString(mobile);

                        Properties.Settings.Default.ConnectMobile = Convert.ToString(mobile);
                        Properties.Settings.Default.Save();

                        new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                            .AddArgument("conversationId", 9813)
                            .AddText("Связано с устройством " + Convert.ToString(mobile))
                            .AddText("Просматривайте информацию о ПК в приложении.")
                        .Show();

                        MyDevice myDevice = new MyDevice();
                        myDevice.Show();
                        timer_sec.Stop();
                        connectOK = 1;
                        this.Close();
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch
            {
                info.Content = "Сервер не отвечает, повтор...";
            }
        }

        public void Light_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#F2FFFFFF"); // Белый фон
            ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
        }
        public void Dark_Theme()
        {
            var bc = new BrushConverter();
            this.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#F2343434"); // Чёрный фон
            ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static BitmapSource ConvertBit(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        private void Cansel(object sender, RoutedEventArgs e)
        {
            MyDevice myDevice = new MyDevice();
            myDevice.Show();
            Close();
        }

        public static string GetMotherBoardID()
        {
            string mbInfo = String.Empty;
            ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            scope.Connect();
            ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

            foreach (PropertyData propData in wmiClass.Properties)
            {
                if (propData.Name == "SerialNumber")
                    mbInfo = Convert.ToString(propData.Value);
            }

            if (mbInfo == "None")
            {
                mbInfo = "virtualMachine";
            }

            return mbInfo;
        }

        private void QR_OPEN(object sender, RoutedEventArgs e)
        {
            TabsConnect.SelectedIndex = 0;
        }

        private void CODE_OPEN(object sender, RoutedEventArgs e)
        {
            TabsConnect.SelectedIndex = 1;
        }

        private void Copy_Code(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetMotherBoardID());
        }
    }
}
