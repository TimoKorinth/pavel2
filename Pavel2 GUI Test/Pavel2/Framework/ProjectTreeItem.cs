using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pavel2.GUI;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Pavel2.Framework {
    public abstract class ProjectTreeItem {

        private String header;
        private Visualization lastVisualization;
        RenderTargetBitmap screenshot;

        public RenderTargetBitmap Screenshot {
            get { return screenshot; }
            set { screenshot = value; }
        }

        public void TakeScreenShot() {
            if (lastVisualization == null) return;
            FrameworkElement vis = (FrameworkElement)lastVisualization;
            screenshot = new RenderTargetBitmap((int)vis.ActualWidth, (int)vis.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            screenshot.Render(vis);
        }

        public Visualization LastVisualization {
            get { return lastVisualization; }
            set { lastVisualization = value; }
        }

        public String Header {
            get { return header; }
            set { header = value; }
        }

        public abstract DataGrid DataGrid { get; set; }

    }
}
