using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Pavel2.GUI {
    public class HeaderToImageConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is String) {
                String header = (String)value;
                if (header.StartsWith("H#")) {
                    Uri uri = new Uri("pack://application:,,,/GUI/Icons/HDD.png");
                    BitmapImage source = new BitmapImage(uri);
                    return source;
                } else if (header.StartsWith("D#")) {
                    Uri uri = new Uri("pack://application:,,,/GUI/Icons/Folder.png");
                    BitmapImage source = new BitmapImage(uri);
                    return source;
                } else if (header.StartsWith("F#")) {
                    Uri uri = new Uri("pack://application:,,,/GUI/Icons/Document.png");
                    BitmapImage source = new BitmapImage(uri);
                    return source;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
