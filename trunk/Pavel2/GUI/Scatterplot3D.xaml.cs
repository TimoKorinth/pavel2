using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tao.OpenGl;
using Pavel2.Framework;
using System.Windows.Forms.Integration;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Interop;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für Scatterplot3D.xaml
    /// </summary>
    public partial class Scatterplot3D : UserControl, Visualization {

        internal class OpenGLRenderWind : OpenGLControl {
            public OpenGLRenderWind()
                : base() {
            }

            protected override void InitOpenGL() {
                Gl.glClearColor(1.0f, 1.0f, 1.0f, 0.0f);
                Gl.glShadeModel(Gl.GL_FLAT);
                Gl.glDisable(Gl.GL_CULL_FACE);
                Gl.glEnable(Gl.GL_LINE_SMOOTH);
                Gl.glEnable(Gl.GL_POINT_SMOOTH);
                Gl.glEnable(Gl.GL_BLEND);
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
                Gl.glEnableClientState(Gl.GL_COLOR_ARRAY);
            }

            protected override void SetupModelViewMatrixOperations() {
                Gl.glLoadIdentity();
            }
        }

        OpenGLRenderWind wfPA;
        DataGrid dataGrid;
        CombinedDataItem comp;
        //WindowsFormsHost host = new WindowsFormsHost();
        private double[] vertexArray;
        private float[] colorArray;
        private System.Windows.Point mouseDragStartPoint;
        private float lrAngle;
        private float udAngle;
        private float lrAngleCurrent;
        private float udAngleCurrent;
        private float lrAngleTemp;
        private float udAngleTemp;
        private float zoom = 0f;

        public float LRAngleCurrent {
            get { return lrAngleCurrent; }
            set {
                lrAngleCurrent = value % 360;
            }
        }

        private void CreateArrays() {
            this.vertexArray = new double[dataGrid.MaxPoints * 3];
            this.colorArray = new float[dataGrid.MaxPoints * 4];
            for (int pointIndex = 0; pointIndex < dataGrid.MaxPoints; pointIndex++) {
                vertexArray[pointIndex * 3 + 0] = (float)Normalize(dataGrid.DoubleDataField[pointIndex][0], dataGrid.Columns[0]);
                vertexArray[pointIndex * 3 + 1] = (float)Normalize(dataGrid.DoubleDataField[pointIndex][1], dataGrid.Columns[1]);
                vertexArray[pointIndex * 3 + 2] = (float)Normalize(dataGrid.DoubleDataField[pointIndex][2], dataGrid.Columns[2]);
                colorArray[pointIndex * 4 + 0] = ColorManagement.UnselectedColor.R;
                colorArray[pointIndex * 4 + 1] = ColorManagement.UnselectedColor.G;
                colorArray[pointIndex * 4 + 2] = ColorManagement.UnselectedColor.B;
                colorArray[pointIndex * 4 + 3] = 0.8f;
            }
            //this.colorArrayBase = (float[])this.colorArray.Clone();
        }

        public float UDAngleCurrent {
            get { return udAngleCurrent; }
            set {
                float newVal = value % 360;

                if (newVal > 270) udAngleCurrent = 270;
                else if (newVal > 90) udAngleCurrent = 90;
                else if (newVal < -270) udAngleCurrent = -270;
                else if (newVal < -90) udAngleCurrent = -90;
                else udAngleCurrent = newVal;
            }
        }

        public Scatterplot3D() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            host.Child = wfPA;
            wfPA.MouseDown += wfPA_MouseDown;
            wfPA.MouseMove += wfPA_MouseMove;
            wfPA.MouseWheel += wfPA_MouseWheel;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state) {
                wfPA.Width = (int)MainData.MainWindow.visualizationLayer.ActualWidth; ;
                wfPA.Height = (int)MainData.MainWindow.visualizationLayer.ActualHeight;
                wfPA.SetupViewPort();
                return null;
            }), null);
        }

        void wfPA_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            zoom += (float)e.Delta / 1000;
            RenderScene();
        }

        void wfPA_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && mouseDragStartPoint != null) {
                lrAngle = lrAngleTemp + (float)(mouseDragStartPoint.X - e.X);
                udAngle = udAngleTemp + (float)(mouseDragStartPoint.Y - e.Y);
                LRAngleCurrent = lrAngle;
                UDAngleCurrent = udAngle;
                RenderScene();
                //visImage.Source = TakeScreenshot();
                //dataGrid.Cache[this.GetType()] = visImage.Source;
            }
        }

        void wfPA_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            wfPA.MakeCurrentContext();
            mouseDragStartPoint = new System.Windows.Point(e.X, e.Y);
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                udAngleTemp = udAngle;
                lrAngleTemp = lrAngle;
            }
        }

        void host_MouseUp(object sender, MouseButtonEventArgs e) {
            //if (ev.Button == MouseButtons.Left) {
            //    switch (vis.LeftMouseButtonMode) {
            //        case ScatterPlot.LeftMouseButtonModes.Picking:
            //            PickingEnd(new Vector(ev.X, ev.Y)); break;
            //        case ScatterPlot.LeftMouseButtonModes.ScatterPlanesAdd:
            //            if (Control.ModifierKeys != Keys.Control)
            //                ScatterPlaneAddPicking(ev.X, ev.Y);
            //            else
            //                ScatterPlaneRemovePicking(ev.X, ev.Y);
            //            break;
            //        default:
            //            break;
            //    }
            //} else if (ev.Button == MouseButtons.Right
            //    && ev.X == mouseDragStartPoint.X
            //    && ev.Y == mouseDragStartPoint.Y) {
            //    flipAxisMenu.Show(this, ev.X, ev.Y);
            //}

            //mouseDragStartPoint = null;
        }

        private void DrawAxis() {
            //float step = 1 / ((float)vis.ScaleCount - 1);

            //// Axis
            //Gl.glColor3fv(color.RGB);
            //Gl.glBegin(Gl.GL_LINES);
            //Gl.glVertex3f(0, 0, 0);
            //Gl.glVertex3f(axisBase.X, axisBase.Y, axisBase.Z);
            //Gl.glEnd();

            ////Scale
            //float pos;
            //VectorF PointValue;
            //Gl.glColor3fv(ColorManagement.DescriptionColor.RGB);
            //for (int i = 0; i < vis.ScaleCount; i++) {
            //    pos = 0 + i * step;
            //    Gl.glRasterPos3fv((scaleDisp + pos * axisBase).XYZ);
            //    PointValue = ScaleToPoint(scaleDisp + axisBase * pos);
            //    Glut.glutBitmapString(Glut.GLUT_BITMAP_HELVETICA_10, (axisBase.X * PointValue.X + axisBase.Y * PointValue.Y + axisBase.Z * PointValue.Z).ToString("F" + vis.DecimalDigits));
            //}
            ////Label
            //Gl.glRasterPos3f(labelPos.X, labelPos.Y, labelPos.Z);
            //Glut.glutBitmapString(Glut.GLUT_BITMAP_HELVETICA_10, axisCp.Label);
        }

        private void DrawPoints() {
            if (vertexArray == null || colorArray == null) return;
            Gl.glPointSize(5);
            Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, this.vertexArray);
            Gl.glColorPointer(4, Gl.GL_FLOAT, 0, this.colorArray);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glDrawArrays(Gl.GL_POINTS, 0, dataGrid.MaxPoints);
            Gl.glPopMatrix();
            //Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.8f));
            //for (int row = 0; row < dataGrid.MaxPoints; row++) {
            //    Gl.glBegin(Gl.GL_POINTS);
            //    Gl.glVertex3d(Normalize(dataGrid.DoubleDataField[row][0], dataGrid.Columns[0]), Normalize(dataGrid.DoubleDataField[row][1], dataGrid.Columns[1]), Normalize(dataGrid.DoubleDataField[row][2], dataGrid.Columns[2]));
            //    Gl.glEnd();
            //}
        }

        private double Normalize(double value, Column col) {
            if ((col.Max - col.Min) == 0) {
                return 0.5;
            }
            return ((value - col.Min) / (col.Max - col.Min));
        }

        private void RenderScene() {
            wfPA.MakeCurrentContext();
            Gl.glDrawBuffer(Gl.GL_BACK);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            wfPA.SetupViewPort();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glRotatef((udAngleCurrent), 1.0f, 0.0f, 0.0f);
            Gl.glRotatef(-(lrAngleCurrent), 0.0f, 1.0f, 0.0f);
            //Shift the OGL Coordinate System, so that 0.5, 0.5, 0.5 is the center of rotation
            Gl.glTranslatef(-0.5f, -0.5f, -0.5f);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glOrtho(-1 + zoom,
                        1 - zoom,
                       -1 + zoom,
                        1 - zoom,
                       -1000,
                        1000);

            DrawAxis();
            DrawPoints();

            Gl.glFlush();
            wfPA.SwapBuffers();
        }

        #region Visualization Member

        public void Render(Pavel2.Framework.DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            comp = MainData.MainWindow.visualizationLayer.VisualizationData as CombinedDataItem;
            if (dataGrid == null) return;
            if (!dataGrid.Cache.ContainsKey(this.GetType()) || this.dataGrid.Changed[this.GetType()]) {
                this.dataGrid.Changed[this.GetType()] = false;
                CreateArrays();
                RenderScene();
                //visImage.Source = TakeScreenshot();
                //dataGrid.Cache[this.GetType()] = visImage.Source;
            } else {
                //visImage.Source = dataGrid.Cache[this.GetType()];
            }
        }

        public void RenderAfterResize() {
            //wfPA.Height = (int)visImage.ActualHeight;
            //wfPA.Width = (int)visImage.ActualWidth;
            RenderScene();
            //visImage.Source = TakeScreenshot();
        }

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
