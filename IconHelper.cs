using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

public static class IconHelper
{
    public static BitmapSource GetFileIcon(string filePath)
    {
        Icon icon = Icon.ExtractAssociatedIcon(filePath);
        BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        icon.Dispose();

        return bitmapSource;
    }
}