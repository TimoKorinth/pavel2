using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class DataGrid {

        private Column[] columns;
        private String name;
        private Parser parser;
        private String[][] data;

        public String[][] Data {
            get { return data; }
            set { data = value; }
        }

        public Parser Parser {
            get { return parser; }
            set { parser = value; }
        }

        public String Name {
            get { return name; }
            set { name = value; }
        }

        public Column[] Columns {
            get { return columns; }
            set { columns = value; }
        }

        public DataGrid(Column[] columns, String name) {
            this.columns = columns;
            this.name = name;
            this.data = GetRows();
        }

        private String[][] GetRows() {
            int iMax = 0;
            for (int i = 0; i < columns.Length; i++) {
                if (columns[i].Points.Length > columns[iMax].Points.Length) {
                    iMax = i;
                }
            }
            String[][] tmp = new String[columns[iMax].Points.Length][];
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
