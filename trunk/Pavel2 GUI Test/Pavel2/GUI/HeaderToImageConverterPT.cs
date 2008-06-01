using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Pavel2.Framework;

namespace Pavel2.GUI {
    public class HeaderToImageConverterPT : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is FolderProjectTreeItem) {
                Uri uri = new Uri("pack://application:,,,/GUI/Icons/Folder2.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is DataProjectTreeItem) {
                Uri uri = new Uri("pack://application:,,,/GUI/Icons/DataTable.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is Column) {
                Uri uri = new Uri("pack://application:,,,/GUI/Icons/Column.png");
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
