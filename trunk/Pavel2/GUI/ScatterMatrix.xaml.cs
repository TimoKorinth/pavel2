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

        public ScatterMatrix() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            host.Child = wfPA; 
            wfPA.Width = 1000;
            wfPA.Height = 800;
            wfPA.SetupViewPort();
        }

        private void DrawPoints() {
            Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.7f));
            Gl.glEnable(Gl.GL_POINT_SMOOTH);
            Gl.glPointSize(1f);
            if (dataGrid == null) return;
            double step = (double)1 / dataGrid.Columns.Length;
            int index = -1;
            for (int x = 0; x < dataGrid.Columns.Length; x++) {
                for (int y = 0; y < dataGrid.Columns.Length; y++) {
                    if (x == y) continue;
                    Gl.glBegin(Gl.GL_POINTS);
                    for (int row = 0; row < dataGrid.Columns[dataGrid.MaxColumn].Points.Length; row++) {
                        if (comp != null) {
                            if (comp.GetDataItemIndex(row) != index) {
                                index = comp.GetDataItemIndex(row);
                                Gl.glColor4fv(ColorManagement.GetColor(index+2).RGBwithA(0.7f));
                            }
                        }
                        double xCo = Normalize(dataGrid.DoubleDataField[row][x], dataGrid.Columns[x]);
                        double yCo = Normalize(dataGrid.DoubleDataField[row][y], dataGrid.Columns[y]);
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
            double step = (double)1 / dataGrid.Columns.Length;
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
                wfPA.PrintText((float)(x*step), (float)((x*step)+(step/2)), 0.0f, 0.03f, dataGrid.Columns[x].Header);
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
            step = (double)1 / (dataGrid.Columns.Length - 1);
            //Abfrage ob sich was geändert hat, sonst einfach den evtl. schon
            //vorhandenen Screenshot nehmen:
            if (!dataGrid.Cache.ContainsKey(this.GetType()) || this.dataGrid.Changed) {
                this.dataGrid.Changed = false;
                RenderScene();
                visImage.Source = GetScreenshot();
                dataGrid.Cache[this.GetType()] = visImage.Source;
            } else {
                visImage.Source = dataGrid.Cache[this.GetType()];
            }
        }

        public void RenderAfterResize() {
            wfPA.Height = (int)this.ActualHeight;
            wfPA.Width = (int)this.ActualWidth;
            RenderScene();
            visImage.Source = GetScreenshot();
        }

        #endregion

        #region Visualization Member


        public bool OwnScreenshot() {
            return true;
        }

        public ImageSource GetScreenshot() {
            Bitmap bmp = wfPA.Screenshot();
            if (bmp == null) return null;
            return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        #endregion
    }
}
