using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public abstract class ProjectTreeItem {

        private String header;

        public String Header {
            get { return header; }
            set { header = value; }
        }

        public abstract DataGrid DataGrid { get; set; }

    }
}
