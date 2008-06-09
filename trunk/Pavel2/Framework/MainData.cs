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

        public static DataGrid CurrentDataGrid {
            get {
                Pavel2.GUI.MainWindow window = (Pavel2.GUI.MainWindow)Application.Current.MainWindow;
                return window.CurrentDataGrid; 
            }
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
