using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;

namespace Pavel2.GUI {
    public class HeaderToImageConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is DriveInfo) {
                Uri uri = new Uri("pack://application:,,,/GUI/Icons/HDD.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is DirectoryInfo) {
                Uri uri = new Uri("pack://application:,,,/GUI/Icons/Folder.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is FileInfo) {
                Uri uri = new Uri("pack://application:,,,/GUI/Icons/Document.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
