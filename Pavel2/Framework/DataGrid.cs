﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Pavel2.GUI;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Pavel2.Framework {
    [Serializable()]
    public class DataGrid {

        private Column[] columns;
        private Column[] visColumns;
        private String[][] data;
        private double[][] dData;
        private int maxColumn;
        private int maxPoints;
        [NonSerialized()]
        private Dictionary<Type, ImageSource> cache = new Dictionary<Type,ImageSource>();
        private Dictionary<Type, bool> changed = new Dictionary<Type, bool>();
        private bool showAll = true;
        [NonSerialized()]
        private Button undoZoom;
        [NonSerialized()]
        private Button undoColVis;
        [NonSerialized()]
        private Button createNewData;
        [NonSerialized()]
        private Button invertSelection;
        private bool[] selectedPoints;
        private String notes;
        [Browsable(false)]
        public String Notes {
            get { return notes; }
            set { notes = value; }
        }

        private bool isScatterplot = false;
        private int scatterCol1;
        private int scatterCol2;
        [Browsable(false)]
        public bool IsScatterplot {
            get { return isScatterplot; }
            set { isScatterplot = value; }
        }
        [Browsable(false)]
        public int ScatterCol1 {
            get { return scatterCol1; }
            set { scatterCol1 = value; }
        }
        [Browsable(false)]
        public int ScatterCol2 {
            get { return scatterCol2; }
            set { scatterCol2 = value; }
        }

        [field: NonSerialized]
        public event EventHandler ColumnChanged;
        [field: NonSerialized]
        public event EventHandler ColumnVisChanged;

        [Description("Show filtered points")]
        public bool ShowAll {
            get { return showAll; }
            set { 
                if (showAll == value) return;
                showAll = value;
                HasChanged();
                if (showAll) {
                    SetDataFields();
                } else {
                    SetDataFieldsAfterZoom();
                }
            }
        }

        [Browsable(false)]
        public bool[] SelectedPoints {
            get { return selectedPoints; }
        }

        public void ClearSelectedPoints() {
            selectedPoints = new bool[maxPoints];
            HasChanged();
            MainData.MainWindow.selectionStatus.Visibility = Visibility.Collapsed;
        }

        [Browsable(false)]
        public List<Button> Buttons {
            get {
                if (undoColVis == null || undoZoom == null || createNewData == null || invertSelection == null) {
                    InitButtons();
                }
                List<Button> buttons = new List<Button>();
                buttons.Add(undoZoom);
                buttons.Add(undoColVis);
                buttons.Add(createNewData);
                buttons.Add(invertSelection);
                return buttons; 
            }
        }

        [Browsable(false)]
        public int MaxPoints {
            get { return maxPoints; }
            set {
                if (value == maxPoints) return;
                maxPoints = value; 
                selectedPoints = new bool[maxPoints];
            }
        }

        public void ShowNumberSelPoints() {
            int index = 0;
            if (selectedPoints == null) return;
            for (int i = 0; i < selectedPoints.Length; i++) {
                if (selectedPoints[i]) index++;
            }
            if (index > 0) {
                MainData.MainWindow.selectionStatus.Visibility = Visibility.Visible;
                createNewData.Visibility = Visibility.Visible;
                invertSelection.Visibility = Visibility.Visible;
            } else {
                MainData.MainWindow.selectionStatus.Visibility = Visibility.Collapsed;
                createNewData.Visibility = Visibility.Collapsed;
                invertSelection.Visibility = Visibility.Collapsed;
            }
            MainData.MainWindow.selectionStatus.Content = index + " selected Points";
        }

        [Browsable(false)]
        public Dictionary<Type, ImageSource> Cache {
            get {
                if (cache == null) cache = new Dictionary<Type, ImageSource>();
                return cache; 
            }
            set { cache = value; }
        }

        [Browsable(false)]
        public Dictionary<Type, bool> Changed {
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

        public void InitChangeDict() {
            HasChanged();
        }

        public void HasChanged() {
            changed[typeof(FolderProjectTreeItem)] = true;

            Type[] types = System.Reflection.Assembly.GetAssembly(typeof(Visualization)).GetTypes();
            foreach (Type t in types) {
                Type tmp = t.GetInterface("Visualization");
                if (tmp != null) {
                    changed[t] = true;
                }
            }
        }

        public void ChangeColOrder(Column col, int position) {
            List<Column> colList = new List<Column>(columns);
            colList.Remove(col);
            colList.Insert(position, col);
            columns = colList.ToArray();
            SetVisColArray();
            SetDataFields();
            if (ColumnChanged != null) {
                ColumnChanged(this, new EventArgs());
            }
            HasChanged();
        }

        public int GetIndex(Column col) {
            for (int i = 0; i < Columns.Length; i++) {
                if (Columns[i].Equals(col)) return i;
            }
            //TODO: NaN oder so zurückgeben
            return 0;
        }

        public void ColIsVisible(Column col, bool isVis) {
            col.Visible = isVis;
            SetDataFields();
            HasChanged();
            SetVisColArray();
            SetColVisButton();
            if (ColumnVisChanged != null) {
                ColumnVisChanged(col, new EventArgs());
            }
        }

        private void SetColVisButton() {
            undoColVis.Visibility = Visibility.Collapsed;
            foreach (Column col in columns) {
                if (!col.Visible) undoColVis.Visibility = Visibility.Visible;
            }
        }

        public void ChangeColZoom(Column col, double min, double max) {
            col.Max = max;
            col.Min = min;
            col.IsZoomed = true;
            HasChanged();
            if (!showAll) SetDataFieldsAfterZoom();
            undoZoom.Visibility = Visibility.Visible;
        }

        [Browsable(false)]
        public Column[] Columns {
            get {
                if (visColumns == null) {
                    SetVisColArray();
                } 
                return visColumns;
            }
            set { columns = value; }
        }

        private void SetVisColArray() {
            List<Column> tmp = new List<Column>();
            foreach (Column col in columns) {
                if (col.Visible) tmp.Add(col);
            }
            visColumns = tmp.ToArray();
        }

        [Browsable(false)]
        public Column[] RealColumns {
            get { return columns; }
        }

        public DataGrid() {
            this.columns = new Column[0];
            this.data = new String[0][];
            this.dData = new double[0][];
            HasChanged();
            InitButtons();
            InitChangeDict();
        }

        public DataGrid(Column[] columns) {
            this.columns = columns;
            SetDataFields();
            HasChanged();
            InitButtons();
            InitChangeDict();
        }

        public void UpdateAll() {
            UpdateButtons();
            UpdateDataSets();
        }

        public void UpdateDataSets() {
            if (undoColVis.Visibility == Visibility.Visible) {
                SetDataFields();
                HasChanged();
                //if (ColumnVisChanged != null) {
                //    ColumnVisChanged(this, new EventArgs());
                //} 
            }
        }

        public void UpdateButtons() {
            for (int col = 0; col < columns.Length; col++) {
                if (columns[col].IsZoomed) {
                    undoZoom.Visibility = Visibility.Visible;
                }
                if (!columns[col].Visible) {
                    undoColVis.Visibility = Visibility.Visible;
                }
            }
        }

        private void InitButtons() {
            undoZoom = MainData.MainWindow.GetToolBarButton("Zoom", new BitmapImage(new Uri("Icons/zoom_out.png", UriKind.Relative)), "Undo Zoom");
            undoZoom.Visibility = Visibility.Collapsed;
            undoZoom.Click += undoZoom_Click;

            undoColVis = MainData.MainWindow.GetToolBarButton("Columns", new BitmapImage(new Uri("Icons/timeline_marker.png", UriKind.Relative)), "Show all columns");
            undoColVis.Visibility = Visibility.Collapsed;
            undoColVis.Click += undoColVis_Click;

            createNewData = MainData.MainWindow.GetToolBarButton("Create", new BitmapImage(new Uri("Icons/table_go.png", UriKind.Relative)), "Create new data from selected points");
            createNewData.Visibility = Visibility.Collapsed;
            createNewData.Click += createNewData_Click;

            invertSelection = MainData.MainWindow.GetToolBarButton("Invert", new BitmapImage(new Uri("Icons/arrow_switch.png", UriKind.Relative)), "Invert selection");
            invertSelection.Visibility = Visibility.Collapsed;
            invertSelection.Click += invertSelection_Click;
        }

        void invertSelection_Click(object sender, RoutedEventArgs e) {
            for (int i = 0; i < selectedPoints.Length; i++) {
                selectedPoints[i] = !selectedPoints[i];
            }
            ShowNumberSelPoints();
            HasChanged();
            MainData.MainWindow.UpdateVisualization();
        }

        void createNewData_Click(object sender, RoutedEventArgs e) {
            List<IPoint>[] newCols = new List<IPoint>[this.columns.Length];
            for (int row = 0; row < this.selectedPoints.Length; row++) {
                if (this.selectedPoints[row]) {
                    for (int col = 0; col < this.columns.Length; col++) {
                        if (newCols[col] == null) newCols[col] = new List<IPoint>();
                        newCols[col].Add(this.columns[col].Points[row]);
                    }
                }
            }
            Column[] cols = new Column[this.columns.Length];
            for (int i = 0; i < this.columns.Length; i++) {
                cols[i] = new Column(this.columns[i].Header);
                cols[i].Points = newCols[i].ToArray();
                cols[i].DirUp = this.columns[i].DirUp;
                cols[i].CalcMinMax();
                cols[i].Visible = this.columns[i].Visible;
            }
            DataGrid dGrid = new DataGrid(cols);
            DataProjectTreeItem dPTI = new DataProjectTreeItem(dGrid);
            TreeViewItem rootItem = MainData.MainWindow.projectTreeView.SelectedItem;
            dPTI.Header = "Subset";
            if (rootItem.Tag is ProjectTreeItem) dPTI.Header = (rootItem.Tag as ProjectTreeItem).Header + "_Subset";
            if (rootItem.Tag is LinkItem) dPTI.Header = (rootItem.Tag as LinkItem).Header + "_Subset";
            TreeViewItem tvItem = new TreeViewItem();
            tvItem.Tag = dPTI;
            MainData.MainWindow.projectTreeView.UpdateDataTreeViewItem(tvItem);
            MainData.MainWindow.projectTreeView.InsertToProjectTree(tvItem, true, true);
        }

        void undoColVis_Click(object sender, RoutedEventArgs e) {
            foreach (Column col in columns) {
                if (ColumnVisChanged != null && col.Visible == false) {
                    ColumnVisChanged(col, new EventArgs());
                }
                col.Visible = true;
            }
            SetDataFields();
            HasChanged();
            SetColVisButton();
            SetVisColArray();
            MainData.MainWindow.UpdateVisualization();
        }

        void undoZoom_Click(object sender, RoutedEventArgs e) {
            foreach (Column col in columns) {
                col.CalcMinMax();
                col.IsZoomed = false;
            }
            if (!showAll) {
                SetDataFieldsAfterZoom();
            } else {
                SetDataFields();
            }
            undoZoom.Visibility = Visibility.Collapsed;
            HasChanged();
            MainData.MainWindow.UpdateVisualization();
        }

        public void AddColumn(Column column) {
            List<Column> listTmp = new List<Column>();
            listTmp.AddRange(columns);
            listTmp.Add(column);
            this.columns = listTmp.ToArray();
            SetDataFields();
            HasChanged();
            SetVisColArray();
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
            MaxPoints = dData.Count;
            this.dData = dData.ToArray();
            this.data = data.ToArray();
        }

        private void SetMaxColumn() {
            this.maxColumn = 0;
            this.MaxPoints = columns[0].Points.Length;
            for (int i = 0; i < columns.Length; i++) {
                if (columns[i].Points.Length > columns[this.maxColumn].Points.Length) {
                    if (columns[i].Visible) {
                        this.maxColumn = i;
                        MaxPoints = columns[i].Points.Length;
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
