using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pavel2.GUI;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Runtime.Serialization;
using System.Drawing;
using System.Windows.Threading;


namespace Pavel2.Framework {
    [Serializable()]
    public abstract class ProjectTreeItem : ISerializable {

        private String header;
        private Visualization lastVisualization;
        ImageSource screenshot;

        public ImageSource Screenshot {
            get { return screenshot; }
            set { screenshot = value; }
        }

        public ProjectTreeItem() { 
            
        }

        public void TakeScreenShot() {
            if (lastVisualization == null) return;
            if (lastVisualization.OwnScreenshot()) {
                ImageSource tmp = lastVisualization.GetScreenshot();
                if (tmp != null) screenshot = tmp;
            } else {
                FrameworkElement vis = (FrameworkElement)lastVisualization;
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state) {
                    if (vis.ActualWidth > 0 && vis.ActualHeight > 0) {
                        RenderTargetBitmap scr = new RenderTargetBitmap((int)vis.ActualWidth, (int)vis.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                        scr.Render(vis);
                        screenshot = scr;
                    }
                    return null;
                }), null);
            }
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

        public ProjectTreeItem(SerializationInfo info, StreamingContext ctxt) {
            this.DataGrid = (DataGrid)info.GetValue("DataGrid", typeof(DataGrid));
            this.Header = (String)info.GetValue("Header", typeof(String));
            Visualization vis = (Visualization)Activator.CreateInstance((Type)info.GetValue("Visualization", typeof(Type)));
            this.lastVisualization = vis;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("DataGrid", DataGrid);
            info.AddValue("Header", header);
            if (lastVisualization != null) info.AddValue("Visualization", lastVisualization.GetType());
            else info.AddValue("Visualization", typeof(TableView));
        }
    }
}
