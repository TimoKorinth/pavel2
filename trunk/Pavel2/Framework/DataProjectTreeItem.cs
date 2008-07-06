using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Pavel2.Framework {
    [Serializable()]
    public class DataProjectTreeItem : ProjectTreeItem {

        private DataGrid dataGrid;
        private String filename;
        private Parser parser;

        public Parser Parser {
            get { return parser; }
            set { parser = value; }
        }

        public String Filename {
            get { return filename; }
            set { filename = value; }
        }

        public override DataGrid DataGrid {
            get {
                return dataGrid;
            }
            set {
                this.dataGrid = value;
            }
        }

        public DataProjectTreeItem(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
        }

        public DataProjectTreeItem(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt) {
        }
    }
}
