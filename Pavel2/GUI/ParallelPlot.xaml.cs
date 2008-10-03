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
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Windows.Input;

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
        }

        OpenGLRenderWind wfPA;
        DataGrid dataGrid;
        CombinedDataItem comp;
        Canvas lastPanel;
        private double step;
        private System.Windows.Point startPoint;
        private System.Windows.Point endPoint;
        private int scaleNumber = 5;
        WindowsFormsHost host = new WindowsFormsHost();
        private bool selecting = true;

        public ParallelPlot() {
            InitializeComponent();
            this.SizeChanged += ParallelPlot_SizeChanged;
            wfPA = new OpenGLRenderWind();
            host.Child = wfPA;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state) {
                if (wfPA != null) {
                    wfPA.Width = (int)MainData.MainWindow.visualizationLayer.ActualWidth; ;
                    wfPA.Height = (int)MainData.MainWindow.visualizationLayer.ActualHeight;
                    wfPA.SetupViewPort();
                }
                return null;
            }), null);
        }

        void ParallelPlot_SizeChanged(object sender, SizeChangedEventArgs e) {
            for (int i = 0; i < thumbGrid.Children.Count; i++) {
                Thumb t = (Thumb)thumbGrid.Children[i];
                t.Margin = new Thickness(step * i * (thumbGrid.ActualWidth-10), 2, t.Margin.Right, 2);
            }
        }

        private void DrawLines() {
            Gl.glEnable(Gl.GL_LINE_SMOOTH);
            float alpha = 1;
            if (dataGrid.MaxPoints < 100) {
                Gl.glLineWidth(6f);
                alpha = 0.5f;
            }
            if (dataGrid.MaxPoints > 100) {
                Gl.glLineWidth(3f);
                alpha = 0.2f;
            }
            if (dataGrid.MaxPoints > 1000) {
                Gl.glLineWidth(1f);
                alpha = 0.02f;
            }
            if (dataGrid == null) return;
            int index = -1;
            Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(alpha));
            for (int row = 0; row < dataGrid.MaxPoints; row++) {
                if (!dataGrid.SelectedPoints[row]) {
                    if (comp != null) {
                        if (comp.GetDataItemIndex(row) != index) {
                            index = comp.GetDataItemIndex(row);
                            Gl.glColor4fv(ColorManagement.GetColor(index + 2).RGBwithA(alpha));
                        }
                    }
                    bool started = false;
                    for (int col = 0; col < dataGrid.Columns.Length; col++) {
                        if (!double.IsNaN(dataGrid.DoubleDataField[row][col])) {
                            if (col < dataGrid.Columns.Length - 1 && !double.IsNaN(dataGrid.DoubleDataField[row][col + 1])) {
                                if (!started) {
                                    Gl.glLoadName(row);
                                    Gl.glBegin(Gl.GL_LINE_STRIP);
                                    started = true;
                                }
                                double nValue = Normalize(dataGrid.DoubleDataField[row][col], dataGrid.Columns[col]);
                                if (dataGrid.Columns[col].DirUp) {
                                    Gl.glVertex2d(step * col, nValue);
                                } else {
                                    Gl.glVertex2d(step * col, 1 - nValue);
                                }
                            } else if (col == dataGrid.Columns.Length - 1 || started) {
                                double nValue = Normalize(dataGrid.DoubleDataField[row][col], dataGrid.Columns[col]);
                                if (dataGrid.Columns[col].DirUp) {
                                    Gl.glVertex2d(step * col, nValue);
                                } else {
                                    Gl.glVertex2d(step * col, 1 - nValue);
                                }
                                Gl.glEnd();
                                started = false;
                            }
                        }
                    }
                }
            }
            //Selected Points:
            Gl.glColor4fv(ColorManagement.CurrentSelectionColor.RGBwithA(0.8f));
            for (int row = 0; row < dataGrid.MaxPoints; row++) {
                if (dataGrid.SelectedPoints[row]) {
                    bool started = false;
                    for (int col = 0; col < dataGrid.Columns.Length; col++) {
                        if (!double.IsNaN(dataGrid.DoubleDataField[row][col])) {
                            if (col < dataGrid.Columns.Length - 1 && !double.IsNaN(dataGrid.DoubleDataField[row][col + 1])) {
                                if (!started) {
                                    Gl.glLoadName(row);
                                    Gl.glBegin(Gl.GL_LINE_STRIP);
                                    started = true;
                                }
                                double nValue = Normalize(dataGrid.DoubleDataField[row][col], dataGrid.Columns[col]);
                                if (dataGrid.Columns[col].DirUp) {
                                    Gl.glVertex2d(step * col, nValue);
                                } else {
                                    Gl.glVertex2d(step * col, 1 - nValue);
                                }
                            } else if (col == dataGrid.Columns.Length - 1 || started) {
                                double nValue = Normalize(dataGrid.DoubleDataField[row][col], dataGrid.Columns[col]);
                                if (dataGrid.Columns[col].DirUp) {
                                    Gl.glVertex2d(step * col, nValue);
                                } else {
                                    Gl.glVertex2d(step * col, 1 - nValue);
                                }
                                Gl.glEnd();
                                started = false;
                            }
                        }
                    }
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
            for (int col = 0; col < dataGrid.Columns.Length; col++) {
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2d(step * col, 0);
                Gl.glVertex2d(step * col, 1);
                Gl.glEnd();
            }
        }

        private void RenderScene() {
            wfPA.MakeCurrentContext();
            wfPA.SetupViewPort();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            DrawLines();
            DrawAxes();

            Gl.glFlush();
        }

        private void SetOverlayControls() {
            overlayControls.ColumnDefinitions.Clear();
            overlayControls.Children.Clear();
            for (int col = 0; col < dataGrid.Columns.Length; col++) {
                ColumnDefinition colDef = new ColumnDefinition();
                overlayControls.ColumnDefinitions.Add(colDef);
                if (col >= dataGrid.Columns.Length - 2) {
                    colDef.Width = new GridLength(0.5, GridUnitType.Star);
                }
                Grid cGrid = new Grid();
                cGrid.IsMouseDirectlyOverChanged += cGrid_IsMouseDirectlyOverChanged;
                cGrid.Background = System.Windows.Media.Brushes.Transparent;
                overlayControls.Children.Add(cGrid);
                Grid.SetColumn(cGrid, col);

                Canvas wPanel = new Canvas();
                wPanel.Width = 35;

                Thumb t = new Thumb();
                t.DragCompleted += DragCompleted;
                t.DragDelta += DragDelta;
                t.Background = System.Windows.Media.Brushes.Gray;
                t.Tag = dataGrid.Columns[col];
                t.HorizontalAlignment = HorizontalAlignment.Left;
                if (dataGrid.Columns[col].DirUp) {
                    t.Style = (Style)thumbGrid.FindResource("Up");
                } else {
                    t.Style = (Style)thumbGrid.FindResource("Down");
                }
                wPanel.Children.Add(t);

                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                Uri uri = new Uri("Icons/cross.png", UriKind.Relative);
                BitmapImage source = new BitmapImage(uri);
                img.Source = source;
                img.Width = 10;
                img.Height = 10;
                img.Tag = dataGrid.Columns[col];
                img.MouseDown += delButton_Click;
                Canvas.SetLeft(img, 15);
                wPanel.Children.Add(img);

                Thumb tDown = new Thumb();
                Thumb tUp = new Thumb();
                tDown.BorderThickness = new Thickness(1);
                tDown.BorderBrush = System.Windows.Media.Brushes.Red;
                tUp.BorderThickness = new Thickness(1);
                tUp.BorderBrush = System.Windows.Media.Brushes.Red;
                tDown.Tag = dataGrid.Columns[col];
                tUp.Tag = dataGrid.Columns[col];
                tUp.Width = 25;
                tDown.Width = 25;
                tUp.Height = 8;
                tDown.Height = 8;
                tUp.DragDelta += tZoom_DragDelta;
                tDown.DragDelta += tZoom_DragDelta;
                tUp.DragCompleted += tZoom_DragCompleted;
                tDown.DragCompleted += tZoom_DragCompleted;
                wPanel.Children.Add(tUp);
                wPanel.Children.Add(tDown);
                Canvas.SetTop(tUp, 15);
                Canvas.SetBottom(tDown, 20);

                wPanel.HorizontalAlignment = HorizontalAlignment.Left;
                if (col == dataGrid.Columns.Length - 1) {
                    wPanel.HorizontalAlignment = HorizontalAlignment.Right;
                }
                cGrid.Tag = wPanel;
                cGrid.Children.Add(wPanel);
                wPanel.Visibility = Visibility.Collapsed;
            }
        }

        void tZoom_DragCompleted(object sender, DragCompletedEventArgs e) {
            Thumb t = sender as Thumb;
            if (t == null) return;
            Column col = t.Tag as Column;
            if (col == null) return;
            double tmp = Canvas.GetTop(t);
            double pixVal = (col.Max - col.Min) / scaleGrid.ActualHeight;
            double newVal;
            if (double.IsNaN(tmp)) {
                if (col.DirUp) {
                    newVal = col.Min - pixVal * e.VerticalChange;
                    dataGrid.ChangeColZoom(col, newVal, col.Max);
                } else {
                    newVal = col.Max + pixVal * e.VerticalChange;
                    dataGrid.ChangeColZoom(col, col.Min, newVal);
                }
            } else {
                if (col.DirUp) {
                    newVal = col.Max - pixVal * e.VerticalChange;
                    dataGrid.ChangeColZoom(col, col.Min, newVal);
                } else {
                    newVal = col.Min + pixVal * e.VerticalChange;
                    dataGrid.ChangeColZoom(col, newVal, col.Max);
                }
            }
            selecting = true;
            SetLabelPanel();
            SetOverlayControls();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void tZoom_DragDelta(object sender, DragDeltaEventArgs e) {
            Thumb t = sender as Thumb;
            RemoveAdornerArray();
            selecting = false;
            if (t == null) return;
            double tmp = Canvas.GetTop(t);
            double newPos;
            if (double.IsNaN(tmp)) {
                newPos = Canvas.GetBottom(t) - e.VerticalChange;
                if ((newPos > 20) && (newPos < overlayControls.ActualHeight - 23)) Canvas.SetBottom(t, newPos);
            } else {
                newPos = Canvas.GetTop(t) + e.VerticalChange;
                if ((newPos > 15) && (newPos < overlayControls.ActualHeight - 28)) Canvas.SetTop(t, newPos);
            }
        }

        void delButton_Click(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Image btn = sender as System.Windows.Controls.Image;
            if (btn == null) return;
            Column col = btn.Tag as Column;
            if (col == null) return;
            dataGrid.ColIsVisible(col, false);
            step = (double)1 / (dataGrid.Columns.Length - 1);
            SetLabelPanel();
            SetOverlayControls();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void cGrid_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Grid cGrid = sender as Grid;
            if (cGrid == null) return;
            Canvas wPanel = cGrid.Tag as Canvas;
            if (wPanel == null) return;
            if ((bool)e.NewValue) {
                if (lastPanel != null) lastPanel.Visibility = Visibility.Collapsed;
                wPanel.Visibility = Visibility.Visible;
            } else {
                lastPanel = wPanel;
                if (!wPanel.IsMouseOver) wPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void SetLabelPanel() {
            scaleGrid.Children.Clear();
            scaleGrid.ColumnDefinitions.Clear();
            scaleGrid.RowDefinitions.Clear();
            labelGrid.Children.Clear();
            labelGrid.ColumnDefinitions.Clear();
            if (dataGrid == null) return;
            for (int col = 0; col < dataGrid.Columns.Length-1; col++) {
                labelGrid.ColumnDefinitions.Add(new ColumnDefinition());
                scaleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int row = 0; row < scaleNumber-1; row++) {
                scaleGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int col = 0; col < dataGrid.Columns.Length; col++) {
                Label lab = new Label();
                lab.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(lab, col);
                if (col == dataGrid.Columns.Length-1) {
                    lab.HorizontalAlignment = HorizontalAlignment.Right;
                    Grid.SetColumn(lab, col-1);
                }
                lab.Content = dataGrid.Columns[col].Header;
                lab.ToolTip = dataGrid.Columns[col].Header;
                labelGrid.Children.Add(lab);

                double scaleStep = (dataGrid.Columns[col].Max-dataGrid.Columns[col].Min) / (scaleNumber-1);
                double scaleText;
                if (dataGrid.Columns[col].DirUp) {
                    scaleText = dataGrid.Columns[col].Max;
                } else {
                    scaleText = dataGrid.Columns[col].Min;
                }
                for (int i = 0; i < scaleNumber; i++) {
                    TextBlock scale = new TextBlock();
                    Border border = new Border();
                    border.HorizontalAlignment = HorizontalAlignment.Left;
                    border.VerticalAlignment = VerticalAlignment.Top;
                    border.BorderBrush = System.Windows.Media.Brushes.Gray;
                    border.BorderThickness = new Thickness(0, 2, 0, 0);
                    if (i == scaleNumber-1) {
                        border.BorderThickness = new Thickness(0, 0, 0, 2);
                        border.VerticalAlignment = VerticalAlignment.Bottom;
                    }
                    border.HorizontalAlignment = HorizontalAlignment.Left;
                    if (col== dataGrid.Columns.Length-1) {
                        border.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    border.Child = scale;
                    scale.Margin = new Thickness(2, 0, 0, 0);
                    String sText = scaleText.ToString();
                    if (sText.Length > 10) {
                        border.ToolTip = sText;
                        sText = sText.Substring(0, 10)+"...";
                    }
                    scale.Text = sText;
                    if (dataGrid.Columns[col].DirUp) {
                        scaleText -= scaleStep;
                    } else {
                        scaleText += scaleStep;
                    }
                    scale.Foreground = System.Windows.Media.Brushes.Gray;
                    scaleGrid.Children.Add(border);
                    Grid.SetColumn(border, col);
                    Grid.SetRow(border, i);
                }
            }
        }

        void DragDelta(object sender, DragDeltaEventArgs e) {
            Thumb t = sender as Thumb;
            RemoveAdornerArray();
            selecting = false;
            if (t == null) return;
            if ((t.Margin.Left + ((overlayControls.ActualWidth * step) * dataGrid.IndexOf((Column)t.Tag)) + e.HorizontalChange) < -5) return;
            t.Margin = new Thickness(t.Margin.Left + e.HorizontalChange, t.Margin.Top, t.Margin.Right, t.Margin.Bottom);

            line.Visibility = Visibility.Visible;
            line.Margin = new Thickness(t.Margin.Left + ((overlayControls.ActualWidth * step) * dataGrid.IndexOf((Column)t.Tag)) + e.HorizontalChange, line.Margin.Top, line.Margin.Right, line.Margin.Bottom);
        }

        void DragCompleted(object sender, DragCompletedEventArgs e) {
            Thumb t = sender as Thumb;
            if (t == null) return;
            Column col = (Column)t.Tag;

            if (e.HorizontalChange == 0) {
                col.DirUp = !col.DirUp;
                if (col.DirUp) {
                    t.Style = (Style)thumbGrid.FindResource("Up");
                } else {
                    t.Style = (Style)thumbGrid.FindResource("Down");
                }
                SetLabelPanel();
            } else {
                int pos = 0;
                if (e.HorizontalChange < 0) {
                    pos = (int)Math.Ceiling(((t.Margin.Left + ((overlayControls.ActualWidth * step) * dataGrid.IndexOf((Column)t.Tag))) / (step * thumbGrid.ActualWidth)));
                } else {
                    pos = (int)(((t.Margin.Left + ((overlayControls.ActualWidth * step) * dataGrid.IndexOf((Column)t.Tag))) / (step * thumbGrid.ActualWidth)));
                }
                selecting = true;
                dataGrid.ChangeColOrder(col, pos);
                SetLabelPanel();
                SetOverlayControls();
                line.Visibility = Visibility.Collapsed;
            }
            wfPA.Height = (int)visImage.ActualHeight;
            wfPA.Width = (int)visImage.ActualWidth;
            RenderScene();
            visImage.Source = TakeScreenshot();
        }

        #region Visualization Member

        public void Render(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            if (MainData.MainWindow.visualizationLayer.VisualizationData is LinkItem) {
                LinkItem link = MainData.MainWindow.visualizationLayer.VisualizationData as LinkItem;
                if (link.IsCombined) comp = link.CombItem;
            }
            if (dataGrid == null) return;
            step = (double)1 / (dataGrid.Columns.Length - 1);
            SetLabelPanel();
            SetOverlayControls();
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
            wfPA.Height = (int)visImage.ActualHeight;
            wfPA.Width = (int)visImage.ActualWidth;
            RenderScene();
            visImage.Source = TakeScreenshot();
        }

        #endregion

        #region Visualization Member


        public bool OwnScreenshot() {
            return false;
        }

        public ImageSource GetScreenshot() {
            if (this.dataGrid != null && this.dataGrid.Cache.ContainsKey(this.GetType())) {
                return this.dataGrid.Cache[this.GetType()];
            }
            ImageSource imgSource = wfPA.Screenshot();
            if (imgSource == null) return null;
            return imgSource;
        }

        public ImageSource TakeScreenshot() {
            ImageSource imgSource = wfPA.Screenshot();
            if (imgSource == null) return null;
            return imgSource;
        }

        #endregion

        private void visImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            startPoint = e.GetPosition(this);
            endPoint = startPoint;
        }

        private void visImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && selecting) {
                endPoint = e.GetPosition(this);
                DrawRubberBand();
            }
        }

        private void visImage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            int sx = (int)((startPoint.X - openGlCanvas.Margin.Left) * wfPA.Width / visImage.ActualWidth);
            int sy = (int)((startPoint.Y - openGlCanvas.Margin.Top - thumbGrid.ActualHeight) * wfPA.Height / visImage.ActualHeight);
            int ex = (int)((endPoint.X - openGlCanvas.Margin.Left) * wfPA.Width / visImage.ActualWidth);
            int ey = (int)((endPoint.Y - openGlCanvas.Margin.Top - thumbGrid.ActualHeight) * wfPA.Height / visImage.ActualHeight);
            int w = (int)Math.Abs(ex - sx);
            int h = (int)Math.Abs(ey - sy);
            if (w < 5) w = 5;
            if (h < 5) h = 5;
            int mx = (ex - sx) / 2 + sx;
            int my = (ey - sy) / 2 + sy;
            PerformPicking(mx, my, w, h);
            startPoint = e.GetPosition(this);
            endPoint = startPoint;
            RemoveAdornerArray();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
            dataGrid.ShowNumberSelPoints();
        }

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

            //Initialize Picking Matrix
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            // create picking region near cursor location
            Glu.gluPickMatrix(x, (viewport[3] - y), w, h, viewport);

            //Draw Lines

            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glOrtho(0.0, 1.0, 0.0, 1.0, -1.0, 1.0);
            Gl.glPopAttrib();

            wfPA.SetupModelView(true);

            DrawLines();

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

        #region Visualization Member


        public object GetProperties() {
            return null;
        }

        #endregion

        #region Visualization Member


        public UIElement GetUIElement() {
            return null;
        }

        #endregion

        #region IDisposable Member

        public void Dispose() {
            dataGrid = null;
            this.SizeChanged -= ParallelPlot_SizeChanged;
            wfPA.DestroyContexts();
            wfPA = null;
            host.Child = null;
        }

        #endregion
    }
}
