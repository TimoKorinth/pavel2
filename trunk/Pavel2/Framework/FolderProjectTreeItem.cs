using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Pavel2.Framework {
    public class FolderProjectTreeItem : ProjectTreeItem {

        private DataGrid dataGrid;
        private TreeViewItem parentItem;
        private List<Column> columns = new List<Column>();

        public override DataGrid DataGrid {
            get {
                columns.Clear();
                GetColumns(parentItem);
                if (columns.Count > 0) dataGrid = new DataGrid(columns.ToArray(), "");
                return dataGrid;
            }
            set {
                this.dataGrid = value;
            }
        }

        public FolderProjectTreeItem(TreeViewItem parentItem) {
            this.parentItem = parentItem;
        }

        private void GetColumns(TreeViewItem tvItem) {
            foreach (TreeViewItem item in tvItem.Items) {
                if (item.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                    columns.AddRange(dPTI.DataGrid.Columns);
                } else if (item.Tag is FolderProjectTreeItem) {
                    GetColumns(item);
                }
            }
        }
    }
}
