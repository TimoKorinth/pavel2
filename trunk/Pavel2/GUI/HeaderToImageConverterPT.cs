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
                Uri uri = new Uri("Icons/table_multiple.png", UriKind.Relative);
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is DataProjectTreeItem) {
                Uri uri = new Uri("Icons/table.png", UriKind.Relative);
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is Column) {
                Column col = (Column)value;
                Uri uri = new Uri("Icons/control_stop.png", UriKind.Relative);
                if (col.Visible) {
                    uri = new Uri("Icons/control_stop_blue.png", UriKind.Relative);
                }
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is LinkItem) {
                Uri uri = new Uri("Icons/package.png", UriKind.Relative);
                BitmapImage source = new BitmapImage(uri);
                return source;
            } else if (value is ImageTreeItem) {
                Uri uri = new Uri("Icons/image.png", UriKind.Relative);
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
