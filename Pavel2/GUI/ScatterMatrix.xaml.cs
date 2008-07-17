using System;
using System.Windows;
using System.Windows.Controls;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Windows.Forms.Integration;
using Pavel2.Framework;
using System.Windows.Media;

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

            protected override void RenderScene() {
                this.Invalidate();
            }
        }

        System.Windows.Forms.Control wfPA;
        DataGrid dataGrid;
        double step;

        public ScatterMatrix() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            wfPA.Paint += DrawData;
            host.Child = wfPA;
            SetViewPort();
        }

        private void DrawPoints() {
            Gl.glColor4f(0.043f, 0.729f, 0.878f, 0.5f);
            Gl.glEnable(Gl.GL_POINT_SMOOTH);
            Gl.glPointSize(5f);
            if (dataGrid == null) return;
            double step = (double)1 / dataGrid.Columns.Length;
            for (int x = 0; x < dataGrid.Columns.Length; x++) {
                for (int y = 0; y < dataGrid.Columns.Length; y++) {
                    if (x == y) continue;
                    Gl.glBegin(Gl.GL_POINTS);
                    for (int row = 0; row < dataGrid.Columns[dataGrid.MaxColumn].Points.Length; row++) {
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
            }
        }

        private void DrawData(object sender, System.Windows.Forms.PaintEventArgs e) {
            RenderScene();
        }

        private void RenderScene() {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            DrawPoints();
            DrawAxes();

            Gl.glFlush();
        }


        private void SetViewPort() {
            Gl.glViewport(0, 0, (int)host.ActualWidth, (int)host.ActualHeight);
        }

        #region Visualization Member

        public void Render(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            if (dataGrid == null) return;
            step = (double)1 / (dataGrid.Columns.Length - 1);
            SetViewPort();
            RenderScene();
        }

        public void RenderAfterResize() {
            SetViewPort();
        }

        #endregion

        #region Visualization Member


        public bool OwnScreenshot() {
            return true;
        }

        public ImageSource GetScreenshot() {
            return null; ;
        }

        #endregion
    }
}
