using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class DataGrid {

        private Column[] columns;
        private String[][] data;
        private double[][] dData;
        private int maxColumn;

        public int MaxColumn {
            get { return maxColumn; }
            set { maxColumn = value; }
        }

        public String[][] DataField {
            get { return data; }
            set { data = value; }
        }

        public double[][] DoubleDataField {
            get { return dData; }
            set { dData = value; }
        }

        public Column[] Columns {
            get { return columns; }
            set { columns = value; }
        }

        public DataGrid() {
            this.columns = new Column[0];
            this.data = new String[0][];
            this.dData = new double[0][];
        }

        public DataGrid(Column[] columns) {
            this.columns = columns;
            SetDataFields();
        }

        public void AddColumn(Column column) {
            List<Column> listTmp = new List<Column>();
            listTmp.AddRange(columns);
            listTmp.Add(column);
            this.columns = listTmp.ToArray();
            SetDataFields();
        }

        private void SetDataFields() {
            SetMaxColumn();
            double[][] dData = new double[columns[this.maxColumn].Points.Length][];
            String[][] data = new String[columns[this.maxColumn].Points.Length][];
            for (int i = 0; i < dData.Length; i++) {
                dData[i] = new double[columns.Length];
                data[i] = new String[columns.Length];
                for (int j = 0; j < columns.Length; j++) {
                    if (columns[j].Points.Length > i) {
                        dData[i][j] = columns[j].Points[i].DoubleData;
                        data[i][j] = columns[j].Points[i].Data;
                    } else {
                        dData[i][j] = double.NaN;
                        data[i][j] = "";
                    }
                }
            }
            this.dData = dData;
            this.data = data;
        }

        private void SetMaxColumn() {
            this.maxColumn = 0;
            for (int i = 0; i < columns.Length; i++) {
                if (columns[i].Points.Length > columns[this.maxColumn].Points.Length) {
                    this.maxColumn = i;
                }
            }
        }

    }
}
