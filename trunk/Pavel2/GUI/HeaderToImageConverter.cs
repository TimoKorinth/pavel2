using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Pavel2.GUI {
    public class HeaderToImageConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if ((value as string).Contains(@"\")) {
                Uri uri = new Uri("pack://application:,,,/GUI/Icons/HDD.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else {
                Uri uri = new Uri("pack://application:,,,/GUI/Icons/Folder.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
