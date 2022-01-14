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

namespace ModernNotyfi
{
    /// <summary>
    /// Логика взаимодействия для ConnectQR.xaml
    /// </summary>
    public partial class ConnectQR : Window
    {
        String CONNECT_IDENTIFY = "MODERNCONNECT:";

        public ConnectQR()
        {
            InitializeComponent();
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
            var timer_minute = new System.Windows.Threading.DispatcherTimer();
            timer_minute.Interval = new TimeSpan(0, 0, 1);
            timer_minute.IsEnabled = true;
            timer_minute.Tick += (o, t) =>
            {
                string responseString = string.Empty;
                using (var webClient = new WebClient())
                {
                    responseString = webClient.DownloadString("https://unesell.000webhostapp.com/check_connect.php?id=" + GetMotherBoardID());
                }

                if (responseString != "null")
                {
                    info.Content = "Подкючение...";
                    var mobile = JObject.Parse(responseString).SelectToken("MOBILE");
                    info.Content = "Соединение с " + Convert.ToString(mobile);

                    Properties.Settings.Default.ConnectMobile = Convert.ToString(mobile);
                    Properties.Settings.Default.Save();

                    MyDevice myDevice = new MyDevice();
                    myDevice.Show();
                    timer_minute.Stop();
                    Close();
                }
            };
            timer_minute.Start();
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

            return mbInfo;
        }
    }
}
