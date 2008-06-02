using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pavel2.GUI;

namespace Pavel2.Framework {
    public abstract class ProjectTreeItem {

        private String header;
        private Visualization lastVisualization;

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
