using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Pavel2.Framework {
    public class ComparableProjectTreeItem : ProjectTreeItem {

        private DataGrid dataGrid;
        private List<DataGrid> dataGrids = new List<DataGrid>();

        public override DataGrid DataGrid {
            get {
                if (dataGrid == null) return new DataGrid();
                return dataGrid;
            }
            set {
                this.dataGrid = value;
            }
        }

        public ComparableProjectTreeItem() {
        }

        public bool AddDataGrid(DataGrid dataGrid) {
            if (!IsAllowed(dataGrid)) {
                return false;
            }
            this.dataGrids.Add(dataGrid);
            ConstructDataGrid();
            return true;
        }

        private bool IsAllowed(DataGrid dataGrid) {
            if (this.dataGrid == null) return true;
            if (this.dataGrid.Columns.Length == dataGrid.Columns.Length) return true;
            return false;
        }

        private void ConstructDataGrid() {
            if (dataGrids.Count != 0) {
                int cols = dataGrids[0].Columns.Length;
                List<IPoint>[] points = new List<IPoint>[cols];
                Column[] columns = new Column[cols];
                for (int i = 0; i < cols; i++) {
                    points[i] = new List<IPoint>();
                    foreach (DataGrid dataGrid in dataGrids) {
                        points[i].AddRange(dataGrid.Columns[i].Points);
                    }
                    columns[i] = new Column();
                    columns[i].Header = dataGrids[0].Columns[i].Header;
                    columns[i].Points = points[i].ToArray();
                }
                this.dataGrid = new DataGrid(columns);
            }
        }

    }
}
