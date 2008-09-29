using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pavel2.Framework {
    public static class ImageParser {

        public static bool IsImage(FileInfo file) {
            try {
                BitmapFrame bmp = BitmapFrame.Create(new Uri(file.FullName));
            } catch (Exception) {
                return false;
            }
            return true;
        }

        public static ImageSource GetImage(FileInfo file) {
            return BitmapFrame.Create(new Uri(file.FullName));
        }

    }
}
