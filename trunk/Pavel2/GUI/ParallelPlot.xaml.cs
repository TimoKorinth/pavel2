using System;
using System.Windows;
using System.Windows.Controls;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Windows.Forms.Integration;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für ParallelPlot.xaml
    /// </summary>
    public partial class ParallelPlot : UserControl, Visualization {

        internal class OpenGLRenderWind : SimpleOpenGlControl {
            public OpenGLRenderWind() {
                this.InitializeContexts();
                // Should I explicitly set this.Size here?
            }
        }

        WindowsFormsHost host;
        System.Windows.Forms.Control wfPA;
        static double A, B, C, D;

        public ParallelPlot() {
            InitializeComponent();
            host = new WindowsFormsHost();
            wfPA = new OpenGLRenderWind();
            wfPA.Paint += DrawData;
            host.Child = wfPA;
            this.openGlCanvas.Children.Add(host);
            Gl.glClearColor(1.0f, 0.0f, 0.0f, 0.0f);
            Gl.glColor3f(1.0f, 1.0f, 1.0f);
            Gl.glPointSize(2.0f);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glOrtho(0.0, 1.0, 0.0, 1.0, -1.0, 1.0);
        }

        private void DrawData(object sender, System.Windows.Forms.PaintEventArgs e) {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
            Gl.glBegin(Gl.GL_LINE_STRIP);
            double x, y;
            Random rand = new Random();
            for (double i = 0; i < 100; i++) {
                y = (float)(rand.NextDouble());
                x = (float)i / (100 / 10);
                Gl.glVertex2d(x, y);
            }
            Gl.glEnd();
            Gl.glFlush();
        }

        #region Visualization Member

        public void Render() {
        }

        public void RenderAfterResize() {
        }

        #endregion
    }
}
