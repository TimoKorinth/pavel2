using System;
using System.Windows;
using System.Windows.Controls;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Windows.Forms.Integration;
using Pavel2.Framework;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Interop;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für ParallelPlot.xaml
    /// </summary>
    public partial class ParallelPlot : UserControl, Visualization {

        internal class OpenGLRenderWind : OpenGLControl {
            public OpenGLRenderWind() : base() {
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

            protected override void RenderScene() {
                this.MakeCurrentContext();
                this.Invalidate();
            }
        }

        OpenGLRenderWind wfPA;
        DataGrid dataGrid;
        CombinedDataItem comp;
        double step;

        public ParallelPlot() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            wfPA.Paint += DrawData;
            host.Child = wfPA;
        }

        private void DrawLines() {
            Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.2f));
            Gl.glEnable(Gl.GL_LINE_SMOOTH);
            Gl.glLineWidth(1f);
            bool breakLine = false;
            if (dataGrid == null) return;
            int index = -1;
            for (int row = 0; row < dataGrid.Columns[dataGrid.MaxColumn].Points.Length; row++) {
                if (comp != null) {
                    if (comp.GetDataItemIndex(row) != index) {
                        index = comp.GetDataItemIndex(row);
                        Gl.glColor4fv(ColorManagement.GetColor(index).RGBwithA(0.2f));
                    }
                }
                Gl.glBegin(Gl.GL_LINE_STRIP);
                for (int col = 0; col < dataGrid.Columns.Length; col++) {
                    if (dataGrid.DoubleDataField[row][col] == double.NaN) {
                        Gl.glEnd();
                        breakLine = true;
                    } else {
                        if (breakLine) Gl.glBegin(Gl.GL_LINE_STRIP);
                        double nValue = Normalize(dataGrid.DoubleDataField[row][col], dataGrid.Columns[col]);
                        Gl.glVertex2d(step * col, nValue);
                    }
                }
                if (!breakLine) Gl.glEnd();
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
            for (int col = 0; col < dataGrid.Columns.Length; col++) {
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2d(step * col, 0);
                Gl.glVertex2d(step * col, 1);
                Gl.glEnd();
            }
        }

        private void DrawData(object sender, System.Windows.Forms.PaintEventArgs e) {
            RenderScene();
        }

        private void RenderScene() {
            wfPA.MakeCurrentContext();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            DrawLines();
            DrawAxes();

            Gl.glFlush();
        }

        private void SetLabelPanel() {
            labelGrid.Children.Clear();
            labelGrid.ColumnDefinitions.Clear();
            if (dataGrid == null) return;
            for (int col = 0; col < dataGrid.Columns.Length-1; col++) {
                labelGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int col = 0; col < dataGrid.Columns.Length-1; col++) {
                Label lab = new Label();
                lab.HorizontalAlignment = HorizontalAlignment.Left;
                lab.Content = dataGrid.Columns[col].Header;
                labelGrid.Children.Add(lab);
                Grid.SetColumn(lab, col);
            }
            Label lastLab = new Label();
            lastLab.HorizontalAlignment = HorizontalAlignment.Right;
            lastLab.Content = dataGrid.Columns[dataGrid.Columns.Length - 1].Header;
            labelGrid.Children.Add(lastLab);
            Grid.SetColumn(lastLab, dataGrid.Columns.Length - 1);
        }

        #region Visualization Member

        public void Render(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            comp = MainData.MainWindow.visualizationLayer.VisualizationData as CombinedDataItem;
            if (dataGrid == null) return;
            SetLabelPanel();
            step = (double)1 / (dataGrid.Columns.Length - 1);
            RenderScene();
        }

        public void RenderAfterResize() {
            wfPA.Refresh();
        }

        #endregion

        #region Visualization Member


        public bool OwnScreenshot() {
            return true;
        }

        public ImageSource GetScreenshot() {
            Bitmap bmp = wfPA.Screenshot();
            return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        #endregion
    }
}
