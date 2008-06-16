using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class CombinedDataItem : ProjectTreeItem {

        private List<DataProjectTreeItem> dataItems;
        private DataGrid dataGrid;

        public CombinedDataItem(List<DataProjectTreeItem> dataItems) {
            this.dataItems = dataItems;
            CreateDataGrid();
        }

        public override DataGrid DataGrid {
            get { return dataGrid; }
            set { }
        }

        //TODO: Prüfung auf gleiche Spalten auch hier
        private void CreateDataGrid() {
            if (dataItems.Count <= 0) return;
            List<IPoint>[] points = new List<IPoint>[dataItems[0].DataGrid.Columns.Length];
            for (int i = 0; i < dataItems.Count; i++) {
                for (int j = 0; j < points.Length; j++) {
                    if (points[j] == null) points[j] = new List<IPoint>();
                    points[j].AddRange(dataItems[i].DataGrid.Columns[j].Points);
                }
            }
            Column[] cols = new Column[points.Length];
            for (int i = 0; i < cols.Length; i++) {
                cols[i] = new Column(dataItems[0].DataGrid.Columns[i].Header);
                cols[i].Points = points[i].ToArray();
                cols[i].CalcMinMax();
            }
            this.dataGrid = new DataGrid(cols);
        }
    }
}
