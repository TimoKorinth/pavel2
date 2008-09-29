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
                return dataGrid;
            }
            set {
                this.dataGrid = value;
            }
        }

        public void UpdateDataGrid() {
            columns.Clear();
            GetColumns(parentItem);
            if (dataGrid != null) {
                dataGrid.ColumnChanged -= MainData.MainWindow.projectTreeView.dataGrid_ColumnChanged;
                dataGrid.ColumnVisChanged -= MainData.MainWindow.projectTreeView.dataGrid_ColumnVisChanged;
            }
            if (columns.Count > 0) dataGrid = new DataGrid(columns.ToArray());
            else dataGrid = new DataGrid();
            dataGrid.ColumnChanged += MainData.MainWindow.projectTreeView.dataGrid_ColumnChanged;
            dataGrid.ColumnVisChanged += MainData.MainWindow.projectTreeView.dataGrid_ColumnVisChanged;
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
                    if (dPTI.DataGrid != null) columns.AddRange(dPTI.DataGrid.RealColumns);
                } else if (item.Tag is FolderProjectTreeItem) {
                    GetColumns(item);
                }
            }
        }

        private bool SomethingChanged(TreeViewItem tvItem) {
            foreach (TreeViewItem item in tvItem.Items) {
                if (item.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                    if (dPTI.DataGrid.Changed[this.GetType()]) return true;
                } else if (item.Tag is FolderProjectTreeItem) {
                    if (SomethingChanged(item)) return true;
                }
            }
            return false;
        }
    }
}
