using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class DataProjectTreeItem : ProjectTreeItem {

        private DataGrid dataGrid;

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
    }
}
