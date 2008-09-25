using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Pavel2.Framework {
    public static class MainData {

        private static List<Column> columns = new List<Column>();

        public static List<Column> Columns {
            get { return columns; }
            set { columns = value; }
        }

        public static Pavel2.GUI.MainWindow MainWindow {
            get { return (Pavel2.GUI.MainWindow)Application.Current.MainWindow; }
        }

        public static Column CopyColumn(Column col) {
            Column c = new Column(col.Header);
            c.Points = (IPoint[])col.Points.Clone();
            MainData.Columns.Add(c);
            return c;
        }

        public static DataGrid AddColumns(Column[] columns) {
            Column[] cols = columns.Clone() as Column[];
            MainData.columns.AddRange(cols);
            return new DataGrid(cols);
        }

        public static void RemoveColumns(DataGrid dataGrid) {
            if (dataGrid != null) {
                for (int i = 0; i < dataGrid.Columns.Length; i++) {
                    columns.Remove(dataGrid.Columns[i]);
                }
            }
        }

    }
}
