using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DatabaseConnectionLib;

namespace Restaurant_Manager
{
    public class CheckValueToImgSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                ImageSource source = ImageExtention.GetImageSourceFromFile(value as string);
                return source;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
            //throw new NotImplementedException();
        }
    }


    

    public static class ImageExtention
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource GetImageSourceFromFile(string path)
        {
            Bitmap bmp;

            using (var fs = new FileStream(path, FileMode.Open))
            { bmp = new Bitmap(Image.FromStream(fs)); }


            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }
    }
}