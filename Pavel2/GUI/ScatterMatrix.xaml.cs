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
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;

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
        int scaleNumber = 5;

        public ScatterMatrix() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            host.Child = wfPA; 
            wfPA.Width = 1000;
            wfPA.Height = 800;
            wfPA.SetupViewPort();
        }

        private void DrawPoints() {
            if (dataGrid == null) return;
            Gl.glEnable(Gl.GL_POINT_SMOOTH);
            if (dataGrid.Columns.Length == 2) {
                if (dataGrid.MaxPoints < 100) {
                    Gl.glPointSize(15f);
                    Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.9f));
                }
                if (dataGrid.MaxPoints > 100) {
                    Gl.glPointSize(10f);
                    Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.6f));
                }
                if (dataGrid.MaxPoints > 1000) {
                    Gl.glPointSize(5f);
                    Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.3f));
                }
                int index = -1;
                Gl.glBegin(Gl.GL_POINTS);
                for (int row = 0; row < dataGrid.MaxPoints; row++) {
                    if (comp != null) {
                        if (comp.GetDataItemIndex(row) != index) {
                            index = comp.GetDataItemIndex(row);
                            Gl.glColor4fv(ColorManagement.GetColor(index + 2).RGBwithA(0.7f));
                        }
                    }
                    double xCo = Normalize(dataGrid.DoubleDataField[row][0], dataGrid.Columns[0]);
                    double yCo = Normalize(dataGrid.DoubleDataField[row][1], dataGrid.Columns[1]);
                    if (xCo > 1 || xCo < 0 || yCo > 1 || yCo < 0) {
                        continue;
                    }
                    if (!dataGrid.Columns[0].DirUp) xCo = 1 - xCo;
                    if (!dataGrid.Columns[1].DirUp) yCo = 1 - yCo;
                    Gl.glVertex2d(xCo, yCo);
                }
                Gl.glEnd();
            } else {
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
                int index = -1;
                for (int x = 0; x < dataGrid.Columns.Length; x++) {
                    for (int y = 0; y < dataGrid.Columns.Length; y++) {
                        if (x == y) continue;
                        Gl.glBegin(Gl.GL_POINTS);
                        for (int row = 0; row < dataGrid.MaxPoints; row++) {
                            if (comp != null) {
                                if (comp.GetDataItemIndex(row) != index) {
                                    index = comp.GetDataItemIndex(row);
                                    Gl.glColor4fv(ColorManagement.GetColor(index + 2).RGBwithA(0.7f));
                                }
                            }
                            double xCo = Normalize(dataGrid.DoubleDataField[row][x], dataGrid.Columns[x]);
                            double yCo = Normalize(dataGrid.DoubleDataField[row][y], dataGrid.Columns[y]);
                            if (xCo > 1 || xCo < 0 || yCo > 1 || yCo < 0) {
                                continue;
                            }
                            if (!dataGrid.Columns[x].DirUp) xCo = step - (xCo * step) + step * x;
                                else xCo = (xCo * step) + step * x;
                            if (!dataGrid.Columns[y].DirUp) yCo = step - (yCo * step) + step * y;
                                else yCo = (yCo * step) + step * y;
                            Gl.glVertex2d(xCo, yCo);
                        }
                        Gl.glEnd();
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
            if (dataGrid == null) return;
            Gl.glColor3f(0.5f, 0.5f, 0.5f);
            Gl.glDisable(Gl.GL_LINE_SMOOTH);
            Gl.glLineWidth(2f);
            if (dataGrid.Columns.Length == 2) {
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2d(0, 0);
                Gl.glVertex2d(1, 0);
                Gl.glEnd();
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2d(0, 0);
                Gl.glVertex2d(0, 1);
                Gl.glEnd();
            } else {
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
        }

        private void SetupOverlayControls() {
            Canvas xCanvas = new Canvas();
            Thumb tLeftX = new Thumb();
            Thumb tRightX = new Thumb();
            tLeftX.Tag = dataGrid.Columns[0];
            tRightX.Tag = dataGrid.Columns[0];
            tRightX.Width = 8;
            tLeftX.Width = 8;
            tRightX.Height = 20;
            tLeftX.Height = 20;
            tRightX.DragDelta += tZoomX_DragDelta;
            tLeftX.DragDelta += tZoomX_DragDelta;
            tRightX.DragCompleted += tZoomX_DragCompleted;
            tLeftX.DragCompleted += tZoomX_DragCompleted;
            xCanvas.Children.Add(tRightX);
            xCanvas.Children.Add(tLeftX);
            Canvas.SetRight(tRightX, 0);
            Canvas.SetLeft(tLeftX, 0);

            Canvas yCanvas = new Canvas();
            Thumb tDownY = new Thumb();
            Thumb tUpY = new Thumb();
            tDownY.Tag = dataGrid.Columns[1];
            tUpY.Tag = dataGrid.Columns[1];
            tUpY.Width = 20;
            tDownY.Width = 20;
            tUpY.Height = 8;
            tDownY.Height = 8;
            tUpY.DragDelta += tZoomY_DragDelta;
            tDownY.DragDelta += tZoomY_DragDelta;
            tUpY.DragCompleted += tZoomY_DragCompleted;
            tDownY.DragCompleted += tZoomY_DragCompleted;
            yCanvas.Children.Add(tUpY);
            yCanvas.Children.Add(tDownY);
            Canvas.SetTop(tUpY, 0);
            Canvas.SetBottom(tDownY, 0);

            xLabels.Children.Add(xCanvas);
            yLabels.Children.Add(yCanvas);
            xCanvas.Visibility = Visibility.Collapsed;
            yCanvas.Visibility = Visibility.Collapsed;
            xLabels.Tag = xCanvas;
            xLabels.Background = System.Windows.Media.Brushes.Transparent;
            yLabels.Background = System.Windows.Media.Brushes.Transparent;
            yLabels.Tag = yCanvas;
            Grid.SetColumn(yCanvas, 1);
            Grid.SetRowSpan(yCanvas, this.scaleNumber - 1);
            Grid.SetColumnSpan(xCanvas, this.scaleNumber - 1);
        }

        void tZoomX_DragCompleted(object sender, DragCompletedEventArgs e) {
            Thumb t = sender as Thumb;
            if (t == null) return;
            Column col = t.Tag as Column;
            if (col == null) return;
            double tmp = Canvas.GetRight(t);
            double pixVal = (col.Max - col.Min) / xLabels.ActualWidth;
            double newVal;
            if (double.IsNaN(tmp)) {
                if (col.DirUp) {
                    newVal = col.Min + pixVal * e.HorizontalChange;
                    dataGrid.ChangeColZoom(col, newVal, col.Max);
                } else {
                    newVal = col.Max - pixVal * e.HorizontalChange;
                    dataGrid.ChangeColZoom(col, col.Min, newVal);
                }
            } else {
                if (col.DirUp) {
                    newVal = col.Max + pixVal * e.HorizontalChange;
                    dataGrid.ChangeColZoom(col, col.Min, newVal);
                } else {
                    newVal = col.Min - pixVal * e.HorizontalChange;
                    dataGrid.ChangeColZoom(col, newVal, col.Max);
                }
            }
            SetLabels();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void tZoomX_DragDelta(object sender, DragDeltaEventArgs e) {
            Thumb t = sender as Thumb;
            if (t == null) return;
            double tmp = Canvas.GetRight(t);
            double newPos;
            if (double.IsNaN(tmp)) {
                newPos = Canvas.GetLeft(t) + e.HorizontalChange;
                if ((newPos > 0) && (newPos < xLabels.ActualWidth - 8)) Canvas.SetLeft(t, newPos);
            } else {
                newPos = Canvas.GetRight(t) - e.HorizontalChange;
                if ((newPos > 0) && (newPos < xLabels.ActualWidth - 8)) Canvas.SetRight(t, newPos);
            }
        }

        void tZoomY_DragCompleted(object sender, DragCompletedEventArgs e) {
            Thumb t = sender as Thumb;
            if (t == null) return;
            Column col = t.Tag as Column;
            if (col == null) return;
            double tmp = Canvas.GetTop(t);
            double pixVal = (col.Max - col.Min) / yLabels.ActualHeight;
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
            SetLabels();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void tZoomY_DragDelta(object sender, DragDeltaEventArgs e) {
            Thumb t = sender as Thumb;
            if (t == null) return;
            double tmp = Canvas.GetTop(t);
            double newPos;
            if (double.IsNaN(tmp)) {
                newPos = Canvas.GetBottom(t) - e.VerticalChange;
                if ((newPos > 0) && (newPos < yLabels.ActualHeight - 8)) Canvas.SetBottom(t, newPos);
            } else {
                newPos = Canvas.GetTop(t) + e.VerticalChange;
                if ((newPos > 0) && (newPos < yLabels.ActualHeight - 8)) Canvas.SetTop(t, newPos);
            }
        }

        private void SetupSwitchButton() {
            switchColPanel.Children.Clear();
            switchColPanel.MouseLeave -= switchColPanel_MouseLeave;
            switchColPanel.MouseEnter -= switchColPanel_MouseEnter;
            Button btn = new Button();
            btn.Margin = new Thickness(5);
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = new BitmapImage(new Uri("Icons/arrow_switch.png", UriKind.Relative));
            btn.Content = img;
            btn.ToolTip = "Switch columns";
            switchColPanel.Children.Add(btn);
            switchColPanel.Tag = btn;
            switchColPanel.Background = System.Windows.Media.Brushes.Transparent;
            btn.Visibility = Visibility.Hidden;
            if (switchColPanel.IsMouseOver) btn.Visibility = Visibility.Visible;
            switchColPanel.MouseLeave += switchColPanel_MouseLeave;
            switchColPanel.MouseEnter += switchColPanel_MouseEnter;
            btn.Click += btnSwitch_Click;
        }

        void btnSwitch_Click(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn == null) return;
            dataGrid.ChangeColOrder(dataGrid.Columns[0], dataGrid.GetIndex(dataGrid.Columns[1]));
            SetLabels();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void switchColPanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            Grid cGrid = sender as Grid;
            if (cGrid == null) return;
            Button btn = cGrid.Tag as Button;
            if (btn == null) return;
            btn.Visibility = Visibility.Visible;
        }

        void switchColPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            Grid cGrid = sender as Grid;
            if (cGrid == null) return;
            Button btn = cGrid.Tag as Button;
            if (btn == null) return;
            btn.Visibility = Visibility.Hidden;
        }

        private void SetLabels() {
            if (dataGrid == null) return;
            labels.Children.Clear();
            labels.ColumnDefinitions.Clear();
            labels.RowDefinitions.Clear();
            xLabels.Children.Clear();
            yLabels.Children.Clear();
            if (dataGrid.Columns.Length == 2) {
                SetupSwitchButton();
                xLabels.ColumnDefinitions.Clear();
                yLabels.RowDefinitions.Clear();
                xLabels.RowDefinitions.Clear();
                yLabels.ColumnDefinitions.Clear();
                xLabels.RowDefinitions.Add(new RowDefinition());
                xLabels.RowDefinitions.Add(new RowDefinition());
                yLabels.ColumnDefinitions.Add(new ColumnDefinition());
                yLabels.ColumnDefinitions.Add(new ColumnDefinition());
                yLabels.ColumnDefinitions[0].Width = GridLength.Auto;

                TextBlock xCol = new TextBlock();
                xCol.Text = dataGrid.Columns[0].Header;
                xCol.HorizontalAlignment = HorizontalAlignment.Center;
                xCol.FontSize = 12;
                xCol.FontWeight = FontWeights.Bold;
                xCol.Margin = new Thickness(5);
                xLabels.Children.Add(xCol);
                Grid.SetRow(xCol, 1);
                Grid.SetColumn(xCol, 0);
                Grid.SetColumnSpan(xCol, scaleNumber);

                TextBlock yCol = new TextBlock();
                yCol.LayoutTransform = new RotateTransform(-90);
                yCol.VerticalAlignment = VerticalAlignment.Center;
                yCol.Text = dataGrid.Columns[1].Header;
                yCol.Margin = new Thickness(5);
                yCol.FontWeight = FontWeights.Bold;
                yCol.FontSize = 12;
                yLabels.Children.Add(yCol);
                Grid.SetRow(yCol, 0);
                Grid.SetColumn(yCol, 0);
                Grid.SetRowSpan(yCol, scaleNumber);

                double scaleStepX = (dataGrid.Columns[0].Max - dataGrid.Columns[0].Min) / (this.scaleNumber - 1);
                double scaleStepY = (dataGrid.Columns[1].Max - dataGrid.Columns[1].Min) / (this.scaleNumber - 1);
                double scaleTextX;
                double scaleTextY;
                if (dataGrid.Columns[0].DirUp) {
                    scaleTextX = dataGrid.Columns[0].Min;
                } else {
                    scaleTextX = dataGrid.Columns[0].Max;
                }
                if (dataGrid.Columns[1].DirUp) {
                    scaleTextY = dataGrid.Columns[1].Max;
                } else {
                    scaleTextY = dataGrid.Columns[1].Min;
                }
                for (int i = 0; i < this.scaleNumber; i++) {
                    if (i != scaleNumber - 1) {
                        xLabels.ColumnDefinitions.Add(new ColumnDefinition());
                        yLabels.RowDefinitions.Add(new RowDefinition());
                    }
                    TextBlock scaleX = new TextBlock();
                    TextBlock scaleY = new TextBlock();
                    String sTextX = scaleTextX.ToString();
                    if (sTextX.Length > 10) {
                        //border.ToolTip = sText;
                        sTextX = sTextX.Substring(0, 10) + "...";
                    }
                    scaleX.Text = sTextX;
                    if (dataGrid.Columns[0].DirUp) {
                        scaleTextX += scaleStepX;
                    } else {
                        scaleTextX -= scaleStepX;
                    }
                    String sTextY = scaleTextY.ToString();
                    if (sTextY.Length > 10) {
                        //border.ToolTip = sText;
                        sTextY = sTextY.Substring(0, 10) + "...";
                    }
                    scaleY.Text = sTextY;
                    if (dataGrid.Columns[1].DirUp) {
                        scaleTextY -= scaleStepY;
                    } else {
                        scaleTextY += scaleStepY;
                    }
                    xLabels.Children.Add(scaleX);
                    yLabels.Children.Add(scaleY);
                    if (i == scaleNumber - 1) {
                        Grid.SetColumn(scaleX, i - 1);
                        Grid.SetRow(scaleY, i - 1);
                        Grid.SetColumn(scaleY, 1);
                        scaleX.HorizontalAlignment = HorizontalAlignment.Right;
                        scaleY.VerticalAlignment = VerticalAlignment.Bottom;
                    } else {
                        Grid.SetColumn(scaleX, i);
                        Grid.SetRow(scaleY, i);
                        Grid.SetColumn(scaleY, 1);
                    }
                }
                SetupOverlayControls();
            } else { 
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

        private void Labels_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            Grid lGrid = sender as Grid;
            if (lGrid == null) return;
            Canvas canvas = lGrid.Tag as Canvas;
            if (canvas == null) return;
            canvas.Visibility = Visibility.Visible;
        }

        private void Labels_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            Grid lGrid = sender as Grid;
            if (lGrid == null) return;
            Canvas canvas = lGrid.Tag as Canvas;
            if (canvas == null) return;
            canvas.Visibility = Visibility.Collapsed;
        }
    }
}
