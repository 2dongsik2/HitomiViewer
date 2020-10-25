using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Scripts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebPWrapper;

namespace HitomiViewer.Processor
{
    class ImageProcessor
    {
        public static async Task<byte[]> ProcessAsync(string url)
        {
            if (url.isUrl())
            {
                if (url.EndsWith(".webp"))
                {
                    return await LoadUrlWebPImageAsync(url);
                }
                else
                {
                    return await LoadUrlImageAsync(url);
                }
            }
            else if (Global.config.file_encrypt.Get<bool>())
            {
                try
                {
                    byte[] org = File.ReadAllBytes(url);
                    byte[] dec = FileDecrypt.Default(org);
                    return dec;
                }
                catch { }
            }
            try
            {
                return LoadFile(url);
            }
            catch (FileNotFoundException)
            {
                return FromResource("NoImage.jpg");
            }
            catch (NotSupportedException)
            {
                return FromResource("ErrEncrypted.jpg");
            }
        }

        public static byte[] LoadFile(string url)
        {
            return File.ReadAllBytes(url);
        }
        public static async Task<byte[]> LoadUrlImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Headers.Add("Referer", "https://" + new Uri(url).Host);
            Byte[] Data = await wc.DownloadDataTaskAsync(url);
            wc.Dispose();
            return Data;
        }
        public static async Task<byte[]> LoadUrlWebPImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Headers.Add("Referer", "https://" + new Uri(url).Host);
            Byte[] MyData = await wc.DownloadDataTaskAsync(url);
            wc.Dispose();
            WebP webP = new WebP();
            Bitmap bitmap = webP.Decode(MyData);
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] data = ms.ToArray();
            ms.Dispose();
            return data;
        }

        public static byte[] FromResource(string psResourceName)
        {
            Uri oUri = new Uri($"/Resources/{psResourceName}", UriKind.RelativeOrAbsolute);
            var info = Application.GetResourceStream(oUri);
            var memoryStream = new MemoryStream();
            byte[] data = memoryStream.ToArray();
            memoryStream.Dispose();
            return data;
        }

        public static BitmapImage Bytes2Image(byte[] array)
        {
            if (array == null || array.Length < 1) return null;
            using (MemoryStream ms = new MemoryStream((byte[])array.Clone()))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        public static BitmapImage Bytes2Image2(byte[] array)
        {
            if (array == null || array.Length < 1) return null;
            BitmapImage bimgTemp = new BitmapImage();
            bimgTemp.BeginInit();
            bimgTemp.StreamSource = new MemoryStream(array);
            bimgTemp.EndInit();
            return bimgTemp;
        }
        public static Byte[] Image2Bytes(BitmapImage image)
        {
            try
            {
                byte[] bytes = null;
                var bitmapSource = image as BitmapSource;
                var encoder = new BmpBitmapEncoder();
                if (bitmapSource != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using (var stream = new System.IO.MemoryStream())
                    {
                        encoder.Save(stream);
                        bytes = stream.ToArray();
                    }
                }
                return bytes;
            }
            catch
            {
                return new byte[0];
            }
        }
        public static async Task<byte[]> PixivImage(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Referer", "https://www.pixiv.net/");
            HttpResponseMessage response = await client.GetAsync(url);
            byte[] data = await response.Content.ReadAsByteArrayAsync();
            return data;
        }
    }
    public static partial class ExtensionMethods
    {
        public static BitmapImage ToImage(this byte[] array) => ImageProcessor.Bytes2Image2(array);
        public static byte[] ToByteArray(this BitmapImage bitmapImage) => ImageProcessor.Image2Bytes(bitmapImage);
    }
}
