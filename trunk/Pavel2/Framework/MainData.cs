using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public static class MainData {

        private static List<Column> columns = new List<Column>();

        public static List<Column> Columns {
            get { return columns; }
            set { columns = value; }
        }

        //public static void AddColumn(Column column) {
        //    columns.Add(column);
        //}

        public static DataGrid AddColumns(Column[] columns) {
            Column[] cols = columns.Clone() as Column[];
            MainData.columns.AddRange(cols);
            return new DataGrid(cols, "");
        }

        public static void RemoveColumns(DataGrid dataGrid) {
            for (int i = 0; i < dataGrid.Columns.Length; i++) {
                columns.Remove(dataGrid.Columns[i]);
            }
        }

        //public static void AddColumn(IPoint[] points) {
        //    AddColumn(points, "");
        //}

        //public static void AddColumn(IPoint[] points, String header) {
        //    Column col = new Column();
        //    col.Points = points;
        //    col.Header = header;
        //    columns.Add(col);
        //}

    }
}
