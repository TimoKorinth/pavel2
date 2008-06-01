using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class DataGrid {

        private Column[] columns;
        private String[][] data;
        private int maxColumn;

        public int MaxColumn {
            get { return maxColumn; }
            set { maxColumn = value; }
        }

        public String[][] Data {
            get { return data; }
            set { data = value; }
        }

        public Column[] Columns {
            get { return columns; }
            set { columns = value; }
        }

        public DataGrid() {
            this.columns = new Column[0];
            this.data = new String[0][];
        }

        public DataGrid(Column[] columns) {
            this.columns = columns;
            this.data = GetRows();
        }

        public void AddColumn(Column column) {
            List<Column> listTmp = new List<Column>();
            listTmp.AddRange(columns);
            listTmp.Add(column);
            this.columns = listTmp.ToArray();
            this.data = GetRows();
        }

        private String[][] GetRows() {
            this.maxColumn = 0;
            for (int i = 0; i < columns.Length; i++) {
                if (columns[i].Points.Length > columns[this.maxColumn].Points.Length) {
                    this.maxColumn = i;
                }
            }
            String[][] tmp = new String[columns[this.maxColumn].Points.Length][];
            for (int i = 0; i < tmp.Length; i++) {
                tmp[i] = new String[columns.Length];
                for (int j = 0; j < columns.Length; j++) {
                    if (columns[j].Points.Length > i) {
                        tmp[i][j] = columns[j].Points[i].Data;
                    } else {
                        tmp[i][j] = "";
                    }
                }
            }
            return tmp;
        }

    }
}
