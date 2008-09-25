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
using Tao.FreeGlut;

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
        private int scaleNumber = 5;
        private float[] vertexArray;
        private float[] colorArray;
        private System.Windows.Point mouseDragStartPoint;
        private System.Windows.Point startPoint;
        private System.Windows.Point endPoint;
        private float lrAngle = 20;
        private float udAngle = 20;
        private float lrAngleCurrent = 20;
        private float udAngleCurrent = 20;
        private float lrAngleTemp;
        private float udAngleTemp;
        private float zoom = 0f;
        private Options properties = new Options();

        public float LRAngleCurrent {
            get { return lrAngleCurrent; }
            set { lrAngleCurrent = value % 360; }
        }

        private void CreateArrays() {
            this.vertexArray = new float[dataGrid.MaxPoints * 3];
            this.colorArray = new float[dataGrid.MaxPoints * 4];
            int index = -1;
            for (int pointIndex = 0; pointIndex < dataGrid.MaxPoints; pointIndex++) {
                vertexArray[pointIndex * 3 + 0] = (float)Normalize(dataGrid.DoubleDataField[pointIndex][properties.Col1], dataGrid.Columns[properties.Col1]);
                vertexArray[pointIndex * 3 + 1] = (float)Normalize(dataGrid.DoubleDataField[pointIndex][properties.Col2], dataGrid.Columns[properties.Col2]);
                vertexArray[pointIndex * 3 + 2] = (float)Normalize(dataGrid.DoubleDataField[pointIndex][properties.Col3], dataGrid.Columns[properties.Col3]);
                if (comp != null) {
                    if (comp.GetDataItemIndex(pointIndex) != index) {
                        index = comp.GetDataItemIndex(pointIndex);
                    }
                }
                if (index != -1) {
                    colorArray[pointIndex * 4 + 0] = ColorManagement.GetColor(index + 2).R;
                    colorArray[pointIndex * 4 + 1] = ColorManagement.GetColor(index + 2).G;
                    colorArray[pointIndex * 4 + 2] = ColorManagement.GetColor(index + 2).B;
                    colorArray[pointIndex * 4 + 3] = 0.8f;
                } else {
                    colorArray[pointIndex * 4 + 0] = ColorManagement.UnselectedColor.R;
                    colorArray[pointIndex * 4 + 1] = ColorManagement.UnselectedColor.G;
                    colorArray[pointIndex * 4 + 2] = ColorManagement.UnselectedColor.B;
                    colorArray[pointIndex * 4 + 3] = 0.8f;
                }
            }
            //this.colorArrayBase = (float[])this.colorArray.Clone();
        }

        public float UDAngleCurrent {
            get { return udAngleCurrent; }
            set { udAngleCurrent = value % 360; }
        }

        public Scatterplot3D() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            host.Child = wfPA;
            wfPA.MouseDown += wfPA_MouseDown;
            wfPA.MouseMove += wfPA_MouseMove;
            wfPA.MouseUp += wfPA_MouseUp;
            wfPA.MouseWheel += wfPA_MouseWheel;
            wfPA.SizeChanged += wfPA_SizeChanged;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state) {
                wfPA.Width = (int)MainData.MainWindow.visualizationLayer.ActualWidth; ;
                wfPA.Height = (int)MainData.MainWindow.visualizationLayer.ActualHeight;
                wfPA.SetupViewPort();
                return null;
            }), null);
        }

        void wfPA_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
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
            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                overlay.Visibility = Visibility.Hidden;
                host.Visibility = Visibility.Visible;
                int sx = (int)((startPoint.X) * wfPA.Width / host.ActualWidth);
                int sy = (int)((startPoint.Y) * wfPA.Height / host.ActualHeight);
                int ex = (int)((endPoint.X) * wfPA.Width / host.ActualWidth);
                int ey = (int)((endPoint.Y) * wfPA.Height / host.ActualHeight);
                int w = (int)Math.Abs(ex - sx);
                int h = (int)Math.Abs(ey - sy);
                if (w < 5) w = 5;
                if (h < 5) h = 5;
                int mx = (ex - sx) / 2 + sx;
                int my = (ey - sy) / 2 + sy;
                PerformPicking(mx, my, w, h);
                startPoint = new System.Windows.Point(e.X, e.Y);
                endPoint = startPoint;
                RemoveAdornerArray();
                SelectPoints();
                RenderScene();
                dataGrid.ShowNumberSelPoints();
            }
        }

        void wfPA_SizeChanged(object sender, EventArgs e) {
            if (dataGrid != null) RenderScene();
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
            } else if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                endPoint = new System.Windows.Point(e.X, e.Y);
                DrawRubberBand();
            }
        }

        void wfPA_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                wfPA.MakeCurrentContext();
                mouseDragStartPoint = new System.Windows.Point(e.X, e.Y);
                udAngleTemp = udAngle;
                lrAngleTemp = lrAngle;
            } else if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                overlay.Source = TakeScreenshot();
                overlay.Visibility = Visibility.Visible;
                host.Visibility = Visibility.Hidden;
                startPoint = new System.Windows.Point(e.X, e.Y);
                endPoint = startPoint;
            }
        }

        private void DrawAxis() {
            float step = 1 / ((float)scaleNumber - 1);
            
            //Axis
            Gl.glColor3fv(ColorManagement.AxesColor.RGB);
            Gl.glLineWidth(3f);

            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(1, 0, 0);
            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 1, 0);
            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 0, 1);
            Gl.glEnd();

            //Scales
            float pos;
            Gl.glColor3fv(ColorManagement.DescriptionColor.RGB);
            for (int i = 0; i < scaleNumber; i++) {
                pos = 0 + i * step;
                Gl.glRasterPos3f(pos , 0, 0);
                Glut.glutBitmapString(Glut.GLUT_BITMAP_HELVETICA_10, (dataGrid.Columns[properties.Col1].Min + (dataGrid.Columns[properties.Col1].Max - dataGrid.Columns[properties.Col1].Min) * step * i).ToString("F5"));
                Gl.glRasterPos3f(0, pos, 0);
                Glut.glutBitmapString(Glut.GLUT_BITMAP_HELVETICA_10, (dataGrid.Columns[properties.Col2].Min + (dataGrid.Columns[properties.Col2].Max - dataGrid.Columns[properties.Col2].Min) * step * i).ToString("F5"));
                Gl.glRasterPos3f(0, 0, pos);
                Glut.glutBitmapString(Glut.GLUT_BITMAP_HELVETICA_10, (dataGrid.Columns[properties.Col3].Min + (dataGrid.Columns[properties.Col3].Max - dataGrid.Columns[properties.Col3].Min) * step * i).ToString("F5"));
            }
            //Labels
            Gl.glRasterPos3f(1.1f, 0, 0);
            Glut.glutBitmapString(Glut.GLUT_BITMAP_HELVETICA_10, dataGrid.Columns[properties.Col1].Header);
            Gl.glRasterPos3f(0, 1.1f, 0);
            Glut.glutBitmapString(Glut.GLUT_BITMAP_HELVETICA_10, dataGrid.Columns[properties.Col2].Header);
            Gl.glRasterPos3f(0, 0, 1.1f);
            Glut.glutBitmapString(Glut.GLUT_BITMAP_HELVETICA_10, dataGrid.Columns[properties.Col3].Header);
        }

        private void DrawPoints() {
            if (vertexArray == null || colorArray == null) return;
            int[] mode = new int[1];
            Gl.glGetIntegerv(Gl.GL_RENDER_MODE, mode);
            Gl.glPointSize(5);
            Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, this.vertexArray);
            Gl.glColorPointer(4, Gl.GL_FLOAT, 0, this.colorArray);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            if (mode[0] == Gl.GL_SELECT) {
                for (int i = 0; i < dataGrid.MaxPoints; i++) {
                    Gl.glLoadName(i);
                    Gl.glBegin(Gl.GL_POINTS);
                    Gl.glArrayElement(i);
                    Gl.glEnd();
                }
            } else {
                Gl.glDrawArrays(Gl.GL_POINTS, 0, dataGrid.MaxPoints);
            }
            Gl.glPopMatrix();
        }

        private void SelectPoints() {
            int index = -1;
            for (int row = 0; row < dataGrid.MaxPoints; row++) {
                if (comp != null) {
                    if (comp.GetDataItemIndex(row) != index) {
                        index = comp.GetDataItemIndex(row);
                    }
                }
                if (index != -1) {
                    colorArray[row * 4 + 0] = ColorManagement.GetColor(index + 2).R;
                    colorArray[row * 4 + 1] = ColorManagement.GetColor(index + 2).G;
                    colorArray[row * 4 + 2] = ColorManagement.GetColor(index + 2).B;
                    colorArray[row * 4 + 3] = 0.8f;
                } else {
                    colorArray[row * 4 + 0] = ColorManagement.UnselectedColor.R;
                    colorArray[row * 4 + 1] = ColorManagement.UnselectedColor.G;
                    colorArray[row * 4 + 2] = ColorManagement.UnselectedColor.B;
                    colorArray[row * 4 + 3] = 0.8f;
                }
                if (dataGrid.SelectedPoints[row]) {
                    colorArray[row * 4 + 0] = ColorManagement.CurrentSelectionColor.R;
                    colorArray[row * 4 + 1] = ColorManagement.CurrentSelectionColor.G;
                    colorArray[row * 4 + 2] = ColorManagement.CurrentSelectionColor.B;
                    colorArray[row * 4 + 3] = 0.8f;
                }
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
            Gl.glDrawBuffer(Gl.GL_BACK);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            wfPA.SetupViewPort();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
            if (dataGrid.Columns.Length < 3) return;
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
            if (MainData.MainWindow.visualizationLayer.VisualizationData is LinkItem) {
                LinkItem link = MainData.MainWindow.visualizationLayer.VisualizationData as LinkItem;
                if (link.IsCombined) comp = link.CombItem;
            }
            if (dataGrid == null) return;
            CreateArrays();
            if (dataGrid.Changed[this.GetType()]) SelectPoints();
            this.dataGrid.Changed[this.GetType()] = false;
            RenderScene();
            dataGrid.Cache[this.GetType()] = TakeScreenshot();
        }

        public void RenderAfterResize() {
            RenderScene();
        }

        public bool OwnScreenshot() {
            return true;
        }

        public ImageSource GetScreenshot() {
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

        private void DrawRubberBand() {
            RemoveAdornerArray();
            AdornerLayer.GetAdornerLayer(this).Add(new RubberBandAdorner(this, startPoint, endPoint));
        }

        private void RemoveAdornerArray() {
            Adorner[] toRemoveArray = AdornerLayer.GetAdornerLayer(this).GetAdorners(this);
            if (toRemoveArray != null) {
                for (int x = 0; x < toRemoveArray.Length; x++) {
                    AdornerLayer.GetAdornerLayer(this).Remove(toRemoveArray[x]);
                }
            }
        }

        /// <summary>
        /// Picks the Points in the given Rectangle
        /// </summary>
        /// <param name="x">x-Coordinate (Window based, left is 0) of the picking region's center</param>
        /// <param name="y">y-Coordinate (Window based, top is 0) of the picking region's center</param>
        /// <param name="w">Width of the Picking Rectangle</param>
        /// <param name="h">Height of the Picking Rectangle</param>
        /// <returns>An array containing the picked Points</returns>
        private void PerformPicking(int x, int y, int w, int h) {
            wfPA.PushMatrices();
            int[] selectBuffer = new int[dataGrid.MaxPoints * 4];
            int[] viewport = new int[4];
            int hits;

            //Extract viewport
            wfPA.SetupViewPort();
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            //Designate SelectBuffer and switch to Select mode
            Gl.glSelectBuffer(selectBuffer.Length, selectBuffer);
            Gl.glRenderMode(Gl.GL_SELECT);

            Gl.glInitNames();
            Gl.glPushName(0);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glRotatef((udAngleCurrent), 1.0f, 0.0f, 0.0f);
            Gl.glRotatef(-(lrAngleCurrent), 0.0f, 1.0f, 0.0f);
            //Shift the OGL Coordinate System, so that 0.5, 0.5, 0.5 is the center of rotation
            Gl.glTranslatef(-0.5f, -0.5f, -0.5f);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPickMatrix(x, (viewport[3] - y), w, h, viewport);
            Gl.glOrtho(-1 + zoom,
                        1 - zoom,
                       -1 + zoom,
                        1 - zoom,
                       -1000,
                        1000);
            DrawPoints();

            //Switch Back to Render Mode
            hits = Gl.glRenderMode(Gl.GL_RENDER);

            wfPA.PopMatrices();
            if (!System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl)) dataGrid.ClearSelectedPoints();
            for (int i = 0; i < hits; i++) {
                dataGrid.SelectedPoints[selectBuffer[i * 4 + 3]] = true;
            }
        }

        private class RubberBandAdorner : Adorner {

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Red);
            System.Windows.Media.Pen renderPen = new System.Windows.Media.Pen(new SolidColorBrush(Colors.Red), 1);
            System.Windows.Point start;
            System.Windows.Point end;

            public RubberBandAdorner(UIElement adornedElement, System.Windows.Point start, System.Windows.Point end)
                : base(adornedElement) {
                this.start = start;
                this.end = end;
            }

            protected override void OnRender(DrawingContext dc) {
                this.IsHitTestVisible = false;
                Rect e = new Rect(start, end);
                renderBrush.Opacity = 0.2;
                dc.DrawRectangle(renderBrush, renderPen, e);
            }
        }

        private class Options {
            private int col1 = 0;
            public int Col1 {
                get { return col1; }
                set { col1 = value; }
            }
            private int col2 = 1;
            public int Col2 {
                get { return col2; }
                set { col2 = value; }
            }
            private int col3 = 2;
            public int Col3 {
                get { return col3; }
                set { col3 = value; }
            }
        }


        #region Visualization Member


        public object GetProperties() {
            return null;
        }

        #endregion

        #region Visualization Member


        public UIElement GetUIElement() {
            if (dataGrid == null) return null;
            ItemsControl itemsControl = new ItemsControl();

            ComboBox col1combo = new ComboBox();
            col1combo.ItemsSource = dataGrid.Columns;
            col1combo.SelectedIndex = properties.Col1;
            col1combo.DisplayMemberPath = "Header";
            col1combo.SelectionChanged += col1combo_SelectionChanged;
            TextBlock col1txt = new TextBlock();
            col1txt.Text = "Column 1";
            col1txt.Width = 120;
            col1txt.Margin = new Thickness(0,0,8,0);
            col1txt.TextAlignment = TextAlignment.Right;
            col1txt.VerticalAlignment = VerticalAlignment.Center;
            StackPanel col1stack = new StackPanel();
            col1stack.Margin = new Thickness(5);
            col1stack.Orientation = Orientation.Horizontal;
            col1stack.Children.Add(col1txt);
            col1stack.Children.Add(col1combo);

            ComboBox col2combo = new ComboBox();
            col2combo.ItemsSource = dataGrid.Columns;
            col2combo.SelectedIndex = properties.Col2;
            col2combo.DisplayMemberPath = "Header";
            col2combo.SelectionChanged += col2combo_SelectionChanged;
            TextBlock col2txt = new TextBlock();
            col2txt.Text = "Column 2";
            col2txt.Width = 120;
            col2txt.Margin = new Thickness(0, 0, 8, 0);
            col2txt.TextAlignment = TextAlignment.Right;
            col2txt.VerticalAlignment = VerticalAlignment.Center;
            StackPanel col2stack = new StackPanel();
            col2stack.Margin = new Thickness(5);
            col2stack.Orientation = Orientation.Horizontal;
            col2stack.Children.Add(col2txt);
            col2stack.Children.Add(col2combo);

            ComboBox col3combo = new ComboBox();
            col3combo.ItemsSource = dataGrid.Columns;
            col3combo.SelectedIndex = properties.Col3;
            col3combo.SelectionChanged += col3combo_SelectionChanged;
            col3combo.DisplayMemberPath = "Header";
            TextBlock col3txt = new TextBlock();
            col3txt.Text = "Column 3";
            col3txt.Width = 120;
            col3txt.Margin = new Thickness(0, 0, 8, 0);
            col3txt.TextAlignment = TextAlignment.Right;
            col3txt.VerticalAlignment = VerticalAlignment.Center;
            StackPanel col3stack = new StackPanel();
            col3stack.Margin = new Thickness(5);
            col3stack.Orientation = Orientation.Horizontal;
            col3stack.Children.Add(col3txt);
            col3stack.Children.Add(col3combo);

            itemsControl.Items.Add(col1stack);
            itemsControl.Items.Add(col2stack);
            itemsControl.Items.Add(col3stack);
            return itemsControl;
        }

        void col1combo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cBox = (ComboBox)sender;
            properties.Col1 = cBox.SelectedIndex;
            CreateArrays();
            if (dataGrid.Changed[this.GetType()]) SelectPoints();
            this.dataGrid.Changed[this.GetType()] = false;
            RenderScene();
            dataGrid.Cache[this.GetType()] = TakeScreenshot();
        }

        void col2combo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cBox = (ComboBox)sender;
            properties.Col2 = cBox.SelectedIndex;
            CreateArrays();
            if (dataGrid.Changed[this.GetType()]) SelectPoints();
            this.dataGrid.Changed[this.GetType()] = false;
            RenderScene();
            dataGrid.Cache[this.GetType()] = TakeScreenshot();
        }

        void col3combo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cBox = (ComboBox)sender;
            properties.Col3 = cBox.SelectedIndex;
            CreateArrays();
            if (dataGrid.Changed[this.GetType()]) SelectPoints();
            this.dataGrid.Changed[this.GetType()] = false;
            RenderScene();
            dataGrid.Cache[this.GetType()] = TakeScreenshot();
        }

        #endregion
    }
}
