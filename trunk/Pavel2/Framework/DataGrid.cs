using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Pavel2.GUI;
using System.Windows;
using System.ComponentModel;

namespace Pavel2.Framework {
    [Serializable()]
    public class DataGrid {

        private Column[] columns;
        private String[][] data;
        private double[][] dData;
        private int maxColumn;
        private int maxPoints;
        private bool changed;
        private Dictionary<Type, ImageSource> cache = new Dictionary<Type,ImageSource>();
        private bool showAll = true;

        public event EventHandler ColumnChanged;
        public event EventHandler ColumnVisChanged;

        [Description("Show filtered points")]
        public bool ShowAll {
            get { return showAll; }
            set { 
                if (showAll == value) return;
                showAll = value;
                changed = true;
                if (showAll) {
                    SetDataFields();
                } else {
                    SetDataFieldsAfterZoom();
                }
            }
        }

        [Browsable(false)]
        public int MaxPoints {
            get { return maxPoints; }
            set { maxPoints = value; }
        }

        [Browsable(false)]
        public Dictionary<Type, ImageSource> Cache {
            get { return cache; }
            set { cache = value; }
        }

        [Browsable(false)]
        public bool Changed {
            get { return changed; }
            set { changed = value; }
        }

        [Browsable(false)]
        public int MaxColumn {
            get { return maxColumn; }
            set { maxColumn = value; }
        }

        [Browsable(false)]
        public String[][] DataField {
            get { return data; }
            set { data = value; }
        }

        [Browsable(false)]
        public double[][] DoubleDataField {
            get { return dData; }
            set { dData = value; }
        }

        public int IndexOf(Column col) {
            List<Column> colList = new List<Column>(columns);
            return colList.IndexOf(col);
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

        public void ColIsVisible(Column col, bool isVis) {
            col.Visible = isVis;
            SetDataFields();
            changed = true;
            if (ColumnVisChanged != null) {
                ColumnVisChanged(this, new EventArgs());
            }
        }

        public void ChangeColZoom(Column col, double min, double max) {
            col.Max = max;
            col.Min = min;
            changed = true;
            if (!showAll) SetDataFieldsAfterZoom();
        }

        [Browsable(false)]
        public Column[] Columns {
            get { 
                List<Column> tmp = new List<Column>();
                foreach (Column col in columns) {
                    if (col.Visible) tmp.Add(col);
                }
                return tmp.ToArray(); 
            }
            set { columns = value; }
        }

        [Browsable(false)]
        public Column[] RealColumns {
            get { return columns; }
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

        public void SetDataFieldsAfterZoom() {
            List<double[]> dData = new List<double[]>();
            List<String[]> data = new List<String[]>();
            int visCols = VisibleColumns();
            for (int i = 0; i < maxPoints; i++) {
                double[] dTmp = new double[visCols];
                String[] sTmp = new String[visCols];
                int index = 0;
                bool isInInterval = true;
                for (int j = 0; j < columns.Length; j++) {
                    if (columns[j].Visible) {
                        if (columns[j].Points[i].DoubleData < columns[j].Min || columns[j].Points[i].DoubleData > columns[j].Max) isInInterval = false;
                        if (columns[j].Points.Length > i) {
                            dTmp[index] = columns[j].Points[i].DoubleData;
                            sTmp[index] = columns[j].Points[i].Data;
                        } else {
                            dTmp[index] = double.NaN;
                            sTmp[index] = "";
                        }
                        index++;
                    }
                }
                if (isInInterval) {
                    dData.Add(dTmp);
                    data.Add(sTmp);
                }
            }
            maxPoints = dData.Count;
            this.dData = dData.ToArray();
            this.data = data.ToArray();
        }

        private void SetMaxColumn() {
            this.maxColumn = 0;
            this.maxPoints = columns[0].Points.Length;
            for (int i = 0; i < columns.Length; i++) {
                if (columns[i].Points.Length > columns[this.maxColumn].Points.Length) {
                    if (columns[i].Visible) {
                        this.maxColumn = i;
                        maxPoints = columns[i].Points.Length;
                    }
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
