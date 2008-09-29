using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Pavel2.Framework {
    public class ImageTreeItem {

        private ImageSource imageSource;
        private String header;

        public String Header {
            get { return header; }
            set { header = value; }
        }

        public ImageSource ImageSource {
            get { return imageSource; }
            set { imageSource = value; }
        }

        public ImageSource Screenshot {
            get { return imageSource; }
            set { imageSource = value; }
        }

        public ImageTreeItem(ImageSource img) {
            this.imageSource = img;
        }

    }
}
