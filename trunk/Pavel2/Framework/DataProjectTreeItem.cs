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
        private DataProjectTreeItem originalData;

        public DataProjectTreeItem OriginalData {
            get { return originalData; }
            set { originalData = value; }
        }

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

        public DataProjectTreeItem Clone() {
            Column[] cols = new Column[this.dataGrid.RealColumns.Length];
            for (int i = 0; i < this.dataGrid.RealColumns.Length; i++) {
                Column c = new Column(this.dataGrid.RealColumns[i].Header);
                c.Points = (IPoint[])this.dataGrid.RealColumns[i].Points.Clone();
                cols[i] = c;
            }
            DataGrid d = new DataGrid(cols);
            DataProjectTreeItem data = new DataProjectTreeItem(d);
            data.OriginalData = this;
            data.filename = this.filename;
            data.Header = this.Header;
            data.LastVisualization = this.LastVisualization;
            data.parser = this.parser;
            data.Screenshot = this.Screenshot;
            return data;
        }

        public DataProjectTreeItem(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
        }

        public DataProjectTreeItem(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt) {
        }
    }
}
