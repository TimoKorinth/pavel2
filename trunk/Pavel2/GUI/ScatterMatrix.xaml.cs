﻿using System;
using System.Windows;
using System.Windows.Controls;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Windows.Forms.Integration;
using Pavel2.Framework;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für ParallelPlot.xaml
    /// </summary>
    public partial class ScatterMatrix : UserControl, Visualization {

        internal class OpenGLRenderWind : OpenGLControl {

            public OpenGLRenderWind()
                : base() {
            }

            protected override void InitOpenGL() {
                Gl.glClearColor(1.0f, 1.0f, 1.0f, 0.0f);
                Gl.glShadeModel(Gl.GL_FLAT);
                Gl.glEnable(Gl.GL_LINE_SMOOTH);
                Gl.glEnable(Gl.GL_BLEND);
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                Gl.glDisable(Gl.GL_CULL_FACE);
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glOrtho(0.0, 1.0, 0.0, 1.0, -1.0, 1.0);
            }

            protected override void SetupModelViewMatrixOperations() {
                Gl.glLoadIdentity();
            }
        }

        OpenGLRenderWind wfPA;
        DataGrid dataGrid;
        double step;
        CombinedDataItem comp;
        WindowsFormsHost host = new WindowsFormsHost();
        Button lastBtn;

        public ScatterMatrix() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            host.Child = wfPA; 
            wfPA.Width = 1000;
            wfPA.Height = 800;
            wfPA.SetupViewPort();
        }

        private void DrawPoints() {
            Gl.glEnable(Gl.GL_POINT_SMOOTH);
            if (dataGrid.MaxPoints < 100) {
                Gl.glPointSize(6f);
                Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.9f));
            }
            if (dataGrid.MaxPoints > 100) {
                Gl.glPointSize(3f);
                Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.6f));
            }
            if (dataGrid.MaxPoints > 1000) {
                Gl.glPointSize(1f);
                Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.3f));
            }
            if (dataGrid == null) return;
            int index = -1;
            for (int x = 0; x < dataGrid.Columns.Length; x++) {
                for (int y = 0; y < dataGrid.Columns.Length; y++) {
                    if (x == y) continue;
                    Gl.glBegin(Gl.GL_POINTS);
                    for (int row = 0; row < dataGrid.MaxPoints; row++) {
                        if (comp != null) {
                            if (comp.GetDataItemIndex(row) != index) {
                                index = comp.GetDataItemIndex(row);
                                Gl.glColor4fv(ColorManagement.GetColor(index+2).RGBwithA(0.7f));
                            }
                        }
                        double xCo = Normalize(dataGrid.DoubleDataField[row][x], dataGrid.Columns[x]);
                        double yCo = Normalize(dataGrid.DoubleDataField[row][y], dataGrid.Columns[y]);
                        if (xCo > 1 || xCo < 0 || yCo > 1 || yCo < 0) {
                            continue;
                        }
                        xCo = (xCo * step) + step * x;
                        yCo = (yCo * step) + step * y;
                        Gl.glVertex2d(xCo, yCo);
                    }
                    Gl.glEnd();
                }
            }
        }

        private double Normalize(double value, Column col) {
            if ((col.Max - col.Min) == 0) {
                return 0.5;
            }
            return ((value - col.Min) / (col.Max - col.Min));
        }

        private void DrawAxes() {
            Gl.glColor3f(0.5f, 0.5f, 0.5f);
            Gl.glDisable(Gl.GL_LINE_SMOOTH);
            Gl.glLineWidth(2f);
            if (dataGrid == null) return;
            for (int x = 0; x < dataGrid.Columns.Length; x++) {
                for (int y = 0; y < dataGrid.Columns.Length; y++) {
                    Gl.glBegin(Gl.GL_LINES);
                    Gl.glVertex2d(step * x, step * y);
                    Gl.glVertex2d(step * x, step * (y + 1));
                    Gl.glEnd();
                    Gl.glBegin(Gl.GL_LINES);
                    Gl.glVertex2d(step * x, step * y);
                    Gl.glVertex2d(step * (x + 1), step * y);
                    Gl.glEnd();
                }
            }
        }

        private void SetLabels() {
            if (dataGrid == null) return;
            labels.Children.Clear();
            labels.ColumnDefinitions.Clear();
            labels.RowDefinitions.Clear();
            for (int x = 0; x < dataGrid.Columns.Length; x++) {
                labels.RowDefinitions.Add(new RowDefinition());
                labels.ColumnDefinitions.Add(new ColumnDefinition());

                Canvas rect = new Canvas();
                Grid.SetColumn(rect, x);
                Grid.SetRow(rect, dataGrid.Columns.Length - 1 - x);
                Button btn = new Button();
                Canvas.SetTop(btn, 2);
                Canvas.SetRight(btn, 2);
                btn.Tag = dataGrid.Columns[x];
                btn.Click += btn_Click;
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                img.Source = new BitmapImage(new Uri("Icons/cross.png", UriKind.Relative));
                btn.Content = img;
                rect.Children.Add(btn);
                labels.Children.Add(rect);
                rect.IsMouseDirectlyOverChanged += rect_IsMouseDirectlyOverChanged;
                btn.Visibility = Visibility.Collapsed;
                rect.Tag = btn;
                rect.Background = System.Windows.Media.Brushes.Transparent;

                Label l = new Label();
                l.Content = dataGrid.Columns[x].Header;
                l.ToolTip = dataGrid.Columns[x].Header;
                labels.Children.Add(l);
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(l, x);
                Grid.SetRow(l, dataGrid.Columns.Length - 1 - x);
            }
            for (int col = 0; col < dataGrid.Columns.Length; col++) {
                for (int row = 0; row < dataGrid.Columns.Length; row++) {
                    if (row != col) {
                        Canvas rect = new Canvas();
                        Grid.SetColumn(rect, col);
                        Grid.SetRow(rect, dataGrid.Columns.Length - 1 - row);
                        Button btn = new Button();
                        Canvas.SetTop(btn, 2);
                        Canvas.SetRight(btn, 2);
                        Column[] cols = new Column[2];
                        cols[0] = dataGrid.Columns[col];
                        cols[1] = dataGrid.Columns[row];
                        btn.Tag = cols;
                        btn.Click += btnZoom_Click;
                        System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                        img.Source = new BitmapImage(new Uri("Icons/zoom_in.png", UriKind.Relative));
                        btn.Content = img;
                        rect.Children.Add(btn);
                        labels.Children.Add(rect);
                        rect.IsMouseDirectlyOverChanged += rect_IsMouseDirectlyOverChanged;
                        btn.Visibility = Visibility.Collapsed;
                        rect.Tag = btn;
                        rect.Background = System.Windows.Media.Brushes.Transparent;
                    }
                }                
            }
        }

        void btnZoom_Click(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn == null) return;
            Column[] cols = btn.Tag as Column[];
            if (cols == null) return;
            foreach (Column col in dataGrid.Columns) {
                if (!col.Equals(cols[0]) && !col.Equals(cols[1])) {
                    dataGrid.ColIsVisible(col, false);
                }
            }
            step = (double)1 / dataGrid.Columns.Length;
            SetLabels();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void btn_Click(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn == null) return;
            Column col = btn.Tag as Column;
            if (col == null) return;
            dataGrid.ColIsVisible(col, false);
            step = (double)1 / dataGrid.Columns.Length;
            SetLabels();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void rect_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Canvas cGrid = sender as Canvas;
            if (cGrid == null) return;
            Button btn = cGrid.Tag as Button;
            if (btn == null) return;
            if ((bool)e.NewValue) {
                if (lastBtn != null) lastBtn.Visibility = Visibility.Collapsed;
                btn.Visibility = Visibility.Visible;
            } else {
                lastBtn = btn;
                if (!btn.IsMouseOver) btn.Visibility = Visibility.Collapsed;
            }
        }

        private void RenderScene() {
            wfPA.MakeCurrentContext();
            wfPA.SetupViewPort();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            DrawPoints();
            DrawAxes();

            Gl.glFlush();
        }

        #region Visualization Member

        public void Render(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            comp = MainData.MainWindow.visualizationLayer.VisualizationData as CombinedDataItem;
            if (dataGrid == null) return;
            step = (double)1 / dataGrid.Columns.Length;
            SetLabels();
            //Abfrage ob sich was geändert hat, sonst einfach den evtl. schon
            //vorhandenen Screenshot nehmen:
            if (!dataGrid.Cache.ContainsKey(this.GetType()) || this.dataGrid.Changed[this.GetType()]) {
                this.dataGrid.Changed[this.GetType()] = false;
                RenderScene();
                visImage.Source = TakeScreenshot();
                dataGrid.Cache[this.GetType()] = visImage.Source;
            } else {
                visImage.Source = dataGrid.Cache[this.GetType()];
            }
        }

        public void RenderAfterResize() {
            wfPA.Height = (int)this.ActualHeight;
            wfPA.Width = (int)this.ActualWidth;
            RenderScene();
            visImage.Source = TakeScreenshot();
        }

        #endregion

        #region Visualization Member


        public bool OwnScreenshot() {
            return true;
        }

        public ImageSource GetScreenshot() {
            if (this.dataGrid != null && this.dataGrid.Cache.ContainsKey(this.GetType())) {
                return this.dataGrid.Cache[this.GetType()];
            }
            Bitmap bmp = wfPA.Screenshot();
            if (bmp == null) return null;
            return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        public ImageSource TakeScreenshot() {
            Bitmap bmp = wfPA.Screenshot();
            if (bmp == null) return null;
            return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        #endregion
    }
}
