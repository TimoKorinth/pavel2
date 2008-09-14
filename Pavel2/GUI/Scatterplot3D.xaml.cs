﻿using System;
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
                Gl.glShadeModel(Gl.GL_FLAT);
                Gl.glDisable(Gl.GL_CULL_FACE);
                Gl.glEnable(Gl.GL_LINE_SMOOTH);
                Gl.glEnable(Gl.GL_POINT_SMOOTH);
                Gl.glEnable(Gl.GL_DEPTH_TEST);
                Gl.glDepthFunc(Gl.GL_ALWAYS);
                Gl.glEnable(Gl.GL_BLEND);
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            }

            protected override void SetupModelViewMatrixOperations() {
                Gl.glLoadIdentity();
            }
        }

        OpenGLRenderWind wfPA;
        DataGrid dataGrid;
        CombinedDataItem comp;
        private int scaleNumber = 5;
        WindowsFormsHost host = new WindowsFormsHost();
        private double[] vertexArray;

        public Scatterplot3D() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            host.Child = wfPA;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state) {
                wfPA.Width = (int)MainData.MainWindow.visualizationLayer.ActualWidth; ;
                wfPA.Height = (int)MainData.MainWindow.visualizationLayer.ActualHeight;
                wfPA.SetupViewPort();
                return null;
            }), null);
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
            Gl.glPointSize(5);
            for (int row = 0; row < dataGrid.MaxPoints; row++) {
                Gl.glBegin(Gl.GL_POINTS);
                Gl.glVertex3d(Normalize(dataGrid.DoubleDataField[row][0], dataGrid.Columns[0]), Normalize(dataGrid.DoubleDataField[row][1], dataGrid.Columns[1]), Normalize(dataGrid.DoubleDataField[row][2], dataGrid.Columns[2]));
                Gl.glEnd();
            }
        }

        private double Normalize(double value, Column col) {
            if ((col.Max - col.Min) == 0) {
                return 0.5;
            }
            return ((value - col.Min) / (col.Max - col.Min));
        }

        private void RenderScene() {
            wfPA.MakeCurrentContext();
            wfPA.SetupViewPort();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            DrawAxis();
            DrawPoints();

            Gl.glFlush();
        }

        #region Visualization Member

        public void Render(Pavel2.Framework.DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            comp = MainData.MainWindow.visualizationLayer.VisualizationData as CombinedDataItem;
            if (dataGrid == null) return;
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
            wfPA.Height = (int)visImage.ActualHeight;
            wfPA.Width = (int)visImage.ActualWidth;
            RenderScene();
            visImage.Source = TakeScreenshot();
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
