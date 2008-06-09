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
    public partial class ParallelPlot : UserControl, Visualization {

        internal class OpenGLRenderWind : SimpleOpenGlControl {
            public OpenGLRenderWind() {
                this.InitializeContexts();
                Gl.glClearColor(1.0f, 1.0f, 1.0f, 0.0f);
                Gl.glShadeModel(Gl.GL_FLAT);
                Gl.glEnable(Gl.GL_LINE_SMOOTH);
                Gl.glEnable(Gl.GL_BLEND);
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                Gl.glDisable(Gl.GL_CULL_FACE);
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadIdentity();
                Gl.glOrtho(0.0, 1.0, 0.0, 1.0, -1.0, 1.0);
            }
        }

        WindowsFormsHost host;
        System.Windows.Forms.Control wfPA;
        DataGrid dataGrid;
        double step;

        public ParallelPlot() {
            InitializeComponent();
            host = new WindowsFormsHost();
            wfPA = new OpenGLRenderWind();
            wfPA.Paint += DrawData;
            host.Child = wfPA;
            openGlCanvas.Children.Add(host);
            SetViewPort();
        }

        private void DrawLines() {
            Gl.glColor3f(0.043f, 0.729f, 0.878f);
            Gl.glLineWidth(1f);
            if (dataGrid == null) return;
            for (int row = 0; row < dataGrid.Columns[dataGrid.MaxColumn].Points.Length; row++) {
                Gl.glBegin(Gl.GL_LINE_STRIP);
                for (int col = 0; col < dataGrid.Columns.Length; col++) {
                    double nValue = Normalize(dataGrid.Columns[col].Points[row].DoubleData, dataGrid.Columns[col]);
                    Gl.glVertex2d(step*col, nValue);
                }
                Gl.glEnd();
            }
        }

        private double Normalize(double value, Column col) {
            if ((col.Max - col.Min) == 0) {
                return 0.5;
            }
            return ((value - col.Min) / (col.Max - col.Min));
        }

        private void DrawAxes() {
            
        }

        private void DrawData(object sender, System.Windows.Forms.PaintEventArgs e) {
            RenderScene();
        }

        private void RenderScene() {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            //DrawAxes();
            DrawLines();

            Gl.glFlush();
        }


        private void SetViewPort() {
            Gl.glViewport(0, 0, (int)openGlCanvas.ActualWidth, (int)openGlCanvas.ActualHeight);
        }

        #region Visualization Member

        public void Render() {
            SetViewPort();
            dataGrid = MainData.CurrentDataGrid;
            if (dataGrid == null) return;
            step = (double)1 / (dataGrid.Columns.Length - 1);
            RenderScene();
            SetViewPort();
        }

        public void RenderAfterResize() {
            SetViewPort();
        }

        #endregion
    }
}
