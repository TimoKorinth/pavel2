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
            if (value is FolderProjectTreeItem || value is LinkItem) {
                Uri uri = new Uri("Icons/Folder2.png", UriKind.Relative);
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is DataProjectTreeItem) {
                Uri uri = new Uri("Icons/DataTable.png", UriKind.Relative);
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is Column) {
                Uri uri = new Uri("Icons/Column.png", UriKind.Relative);
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
