using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Runtime.Serialization;

namespace Pavel2.Framework {
    [Serializable()]
    public class FolderProjectTreeItem : ProjectTreeItem {

        private DataGrid dataGrid;
        private TreeViewItem parentItem;
        private List<Column> columns = new List<Column>();

        public override DataGrid DataGrid {
            get {
                if (SomethingChanged(parentItem) || dataGrid == null) {
                    columns.Clear();
                    GetColumns(parentItem);
                    if (columns.Count > 0) dataGrid = new DataGrid(columns.ToArray());
                }
                return dataGrid;
            }
            set {
                this.dataGrid = value;
            }
        }

        public TreeViewItem ParentItem {
            get { return parentItem; }
            set { parentItem = value; }
        }

        public FolderProjectTreeItem(TreeViewItem parentItem) {
            this.parentItem = parentItem;
        }

        public FolderProjectTreeItem(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt) {
        }

        private void GetColumns(TreeViewItem tvItem) {
            foreach (TreeViewItem item in tvItem.Items) {
                if (item.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                    if (dPTI.DataGrid != null) columns.AddRange(dPTI.DataGrid.Columns);
                    dPTI.DataGrid.Changed = false;
                } else if (item.Tag is FolderProjectTreeItem) {
                    GetColumns(item);
                }
            }
        }

        private bool SomethingChanged(TreeViewItem tvItem) {
            foreach (TreeViewItem item in tvItem.Items) {
                if (item.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                    if (dPTI.DataGrid.Changed) return true;
                } else if (item.Tag is FolderProjectTreeItem) {
                    if (SomethingChanged(item)) return true;
                }
            }
            return false;
        }
    }
}
