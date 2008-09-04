using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Pavel2.GUI;
using System.Windows;

namespace Pavel2.Framework {
    [Serializable()]
    public class DataGrid {

        private Column[] columns;
        private String[][] data;
        private double[][] dData;
        private int maxColumn;
        private bool changed;
        private Dictionary<Type, ImageSource> cache = new Dictionary<Type,ImageSource>();

        public event EventHandler ColumnChanged;

        public Dictionary<Type, ImageSource> Cache {
            get { return cache; }
            set { cache = value; }
        }

        public bool Changed {
            get { return changed; }
            set { changed = value; }
        }

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

        public void ChangeColOrder(Column col, int position) {
            List<Column> colList = new List<Column>(columns);
            colList.Remove(col);
            colList.Insert(position, col);
            columns = colList.ToArray();
            SetDataFields();
            if (ColumnChanged != null) {
                ColumnChanged(this, new EventArgs());
            }
            changed = true;
        }

        public Column[] Columns {
            get { 
                List<Column> tmp = new List<Column>();
                foreach (Column col in columns) {
                    if (col.Visible) tmp.Add(col);
                }
                return tmp.ToArray(); ; 
            }
            set { columns = value; }
        }

        public DataGrid() {
            this.columns = new Column[0];
            this.data = new String[0][];
            this.dData = new double[0][];
            changed = true;
        }

        public DataGrid(Column[] columns) {
            this.columns = columns;
            SetDataFields();
            changed = true;
        }

        public void AddColumn(Column column) {
            List<Column> listTmp = new List<Column>();
            listTmp.AddRange(columns);
            listTmp.Add(column);
            this.columns = listTmp.ToArray();
            SetDataFields();
            changed = true;
        }

        public void SetDataFields() {
            SetMaxColumn();
            double[][] dData = new double[columns[this.maxColumn].Points.Length][];
            String[][] data = new String[columns[this.maxColumn].Points.Length][];
            int visCols = VisibleColumns();
            for (int i = 0; i < dData.Length; i++) {
                dData[i] = new double[visCols];
                data[i] = new String[visCols];
                int index = 0;
                for (int j = 0; j < columns.Length; j++) {
                    if (columns[j].Visible) {
                        if (columns[j].Points.Length > i) {
                            dData[i][index] = columns[j].Points[i].DoubleData;
                            data[i][index] = columns[j].Points[i].Data;
                        } else {
                            dData[i][index] = double.NaN;
                            data[i][index] = "";
                        }
                        index++;
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
                    if (columns[i].Visible) this.maxColumn = i;
                }
            }
        }

        private int VisibleColumns() {
            int i = 0;
            for (int x = 0; x < columns.Length; x++) {
                if (columns[x].Visible) i++;
            }
            return i;
        }

    }
}
