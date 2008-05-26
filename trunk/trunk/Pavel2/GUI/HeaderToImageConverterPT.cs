using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Pavel2.GUI {
    public class HeaderToImageConverterPT : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is String) {
                String header = (String)value;
                if (header.StartsWith("F#")) {
                    Uri uri = new Uri("pack://application:,,,/GUI/Icons/Folder2.png");
                    BitmapImage source = new BitmapImage(uri);
                    return source;
                } else if (header.StartsWith("D#")) {
                    Uri uri = new Uri("pack://application:,,,/GUI/Icons/DataTable.png");
                    BitmapImage source = new BitmapImage(uri);
                    return source;
                } else if (header.StartsWith("V#")) {
                    Uri uri = new Uri("pack://application:,,,/GUI/Icons/Comp.png");
                    BitmapImage source = new BitmapImage(uri);
                    return source;
                } else if (header.StartsWith("C#")) {
                    Uri uri = new Uri("pack://application:,,,/GUI/Icons/Column.png");
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
