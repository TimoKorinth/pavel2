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
using System.Windows.Documents;
using System.Windows.Input;

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
        private System.Windows.Point startPoint;
        private System.Windows.Point endPoint;
        CombinedDataItem comp;
        WindowsFormsHost host = new WindowsFormsHost();
        System.Windows.Controls.Image lastBtn;
        int scaleNumber = 5;

        public ScatterMatrix() {
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

        private void DrawPoints() {
            if (dataGrid == null) return;
            Gl.glEnable(Gl.GL_POINT_SMOOTH);
            float alpha = 1;
            if (dataGrid.IsScatterplot) {
                if (dataGrid.MaxPoints < 100) {
                    Gl.glPointSize(15f);
                    alpha = 0.9f;
                }
                if (dataGrid.MaxPoints > 100) {
                    Gl.glPointSize(10f);
                    alpha = 0.6f;
                }
                if (dataGrid.MaxPoints > 1000) {
                    Gl.glPointSize(5f);
                    alpha = 0.3f;
                }
                Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(alpha));
                int index = -1;
                for (int row = 0; row < dataGrid.MaxPoints; row++) {
                    if (!dataGrid.SelectedPoints[row]) {
                        if (comp != null) {
                            if (comp.GetDataItemIndex(row) != index) {
                                index = comp.GetDataItemIndex(row);
                                Gl.glColor4fv(ColorManagement.GetColor(index + 2).RGBwithA(alpha));
                            }
                        }
                        double xCo = Normalize(dataGrid.DoubleDataField[row][dataGrid.ScatterCol1], dataGrid.Columns[dataGrid.ScatterCol1]);
                        double yCo = Normalize(dataGrid.DoubleDataField[row][dataGrid.ScatterCol2], dataGrid.Columns[dataGrid.ScatterCol2]);
                        if (xCo > 1 || xCo < 0 || yCo > 1 || yCo < 0) {
                            continue;
                        }
                        if (!dataGrid.Columns[dataGrid.ScatterCol1].DirUp) xCo = 1 - xCo;
                        if (!dataGrid.Columns[dataGrid.ScatterCol2].DirUp) yCo = 1 - yCo;
                        Gl.glLoadName(row);
                        Gl.glBegin(Gl.GL_POINTS);
                        Gl.glVertex2d(xCo, yCo);
                        Gl.glEnd();
                    }
                }
                //Selected Points:
                Gl.glColor4fv(ColorManagement.CurrentSelectionColor.RGBwithA(0.8f));
                for (int row = 0; row < dataGrid.MaxPoints; row++) {
                    if (dataGrid.SelectedPoints[row]) {
                        double xCo = Normalize(dataGrid.DoubleDataField[row][dataGrid.ScatterCol1], dataGrid.Columns[dataGrid.ScatterCol1]);
                        double yCo = Normalize(dataGrid.DoubleDataField[row][dataGrid.ScatterCol2], dataGrid.Columns[dataGrid.ScatterCol2]);
                        if (xCo > 1 || xCo < 0 || yCo > 1 || yCo < 0) {
                            continue;
                        }
                        if (!dataGrid.Columns[dataGrid.ScatterCol1].DirUp) xCo = 1 - xCo;
                        if (!dataGrid.Columns[dataGrid.ScatterCol2].DirUp) yCo = 1 - yCo;
                        Gl.glLoadName(row);
                        Gl.glBegin(Gl.GL_POINTS);
                        Gl.glVertex2d(xCo, yCo);
                        Gl.glEnd();
                    }
                }
            } else {
                if (dataGrid.MaxPoints < 100) {
                    Gl.glPointSize(6f);
                    alpha = 0.9f;
                }
                if (dataGrid.MaxPoints > 100) {
                    Gl.glPointSize(3f);
                    alpha = 0.6f;
                }
                if (dataGrid.MaxPoints > 1000) {
                    Gl.glPointSize(1f);
                    alpha = 0.3f;
                }
                int index = -1;
                for (int x = 0; x < dataGrid.Columns.Length; x++) {
                    for (int y = 0; y < dataGrid.Columns.Length; y++) {
                        if (x == y) continue;
                        Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(alpha));
                        for (int row = 0; row < dataGrid.MaxPoints; row++) {
                            if (!dataGrid.SelectedPoints[row]) {
                                if (comp != null) {
                                    if (comp.GetDataItemIndex(row) != index) {
                                        index = comp.GetDataItemIndex(row);
                                        Gl.glColor4fv(ColorManagement.GetColor(index + 2).RGBwithA(alpha));
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
                                Gl.glLoadName(row);
                                Gl.glBegin(Gl.GL_POINTS);
                                Gl.glVertex2d(xCo, yCo);
                                Gl.glEnd();
                            }
                        }
                        //Selected Points:
                        Gl.glColor4fv(ColorManagement.CurrentSelectionColor.RGBwithA(0.8f));
                        for (int row = 0; row < dataGrid.MaxPoints; row++) {
                            if (dataGrid.SelectedPoints[row]) {
                                double xCo = Normalize(dataGrid.DoubleDataField[row][x], dataGrid.Columns[x]);
                                double yCo = Normalize(dataGrid.DoubleDataField[row][y], dataGrid.Columns[y]);
                                if (xCo > 1 || xCo < 0 || yCo > 1 || yCo < 0) {
                                    continue;
                                }
                                if (!dataGrid.Columns[x].DirUp) xCo = step - (xCo * step) + step * x;
                                else xCo = (xCo * step) + step * x;
                                if (!dataGrid.Columns[y].DirUp) yCo = step - (yCo * step) + step * y;
                                else yCo = (yCo * step) + step * y;
                                Gl.glLoadName(row);
                                Gl.glBegin(Gl.GL_POINTS);
                                Gl.glVertex2d(xCo, yCo);
                                Gl.glEnd();
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
            if (dataGrid == null) return;
            Gl.glColor3f(0.5f, 0.5f, 0.5f);
            Gl.glDisable(Gl.GL_LINE_SMOOTH);
            Gl.glLineWidth(2f);
            if (dataGrid.IsScatterplot) {
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
            tLeftX.Tag = dataGrid.Columns[dataGrid.ScatterCol1];
            tRightX.Tag = dataGrid.Columns[dataGrid.ScatterCol1];
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
            tDownY.Tag = dataGrid.Columns[dataGrid.ScatterCol2];
            tUpY.Tag = dataGrid.Columns[dataGrid.ScatterCol2];
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
            Canvas.SetRight(tUpY, 0);
            Canvas.SetRight(tDownY, 0);

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

        private void SetupToggleButtons() {
            togglePanel.Children.Clear();
            togglePanel.MouseLeave -= switchColPanel_MouseLeave;
            togglePanel.MouseEnter -= switchColPanel_MouseEnter;
            
            Button btnSwitch = new Button();
            btnSwitch.Margin = new Thickness(5);
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = new BitmapImage(new Uri("Icons/arrow_switch.png", UriKind.Relative));
            btnSwitch.Content = img;
            btnSwitch.ToolTip = "Switch columns";
            btnSwitch.Click += btnSwitch_Click;

            Button btnBack = new Button();
            btnBack.Margin = new Thickness(5);
            System.Windows.Controls.Image img2 = new System.Windows.Controls.Image();
            img2.Source = new BitmapImage(new Uri("Icons/arrow_undo.png", UriKind.Relative));
            btnBack.Content = img2;
            btnBack.ToolTip = "Show all columns";
            btnBack.Click += btnBack_Click;

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;
            stack.Children.Add(btnBack);
            stack.Children.Add(btnSwitch);
            stack.Visibility = Visibility.Hidden;

            togglePanel.Children.Add(stack);
            togglePanel.Tag = stack;
            togglePanel.Background = System.Windows.Media.Brushes.Transparent;
            if (togglePanel.IsMouseOver) btnSwitch.Visibility = Visibility.Visible;
            togglePanel.MouseLeave += switchColPanel_MouseLeave;
            togglePanel.MouseEnter += switchColPanel_MouseEnter;
        }

        void btnBack_Click(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn == null) return;
            dataGrid.IsScatterplot = false;
            SetLabels();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void btnSwitch_Click(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn == null) return;
            int tmp = dataGrid.ScatterCol1;
            dataGrid.ScatterCol1 = dataGrid.ScatterCol2;
            dataGrid.ScatterCol2 = tmp;
            SetLabels();
            RenderScene();
            visImage.Source = TakeScreenshot();
            dataGrid.Cache[this.GetType()] = visImage.Source;
        }

        void switchColPanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            Grid cGrid = sender as Grid;
            if (cGrid == null) return;
            StackPanel stack = cGrid.Tag as StackPanel;
            if (stack == null) return;
            stack.Visibility = Visibility.Visible;
        }

        void switchColPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            Grid cGrid = sender as Grid;
            if (cGrid == null) return;
            StackPanel stack = cGrid.Tag as StackPanel;
            if (stack == null) return;
            stack.Visibility = Visibility.Hidden;
        }

        private void SetLabels() {
            if (dataGrid == null) return;
            labels.Children.Clear();
            labels.ColumnDefinitions.Clear();
            labels.RowDefinitions.Clear();
            togglePanel.Children.Clear();
            xLabels.Children.Clear();
            yLabels.Children.Clear();
            if (dataGrid.IsScatterplot) {
                SetupToggleButtons();
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
                xCol.Text = dataGrid.Columns[dataGrid.ScatterCol1].Header;
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
                yCol.Text = dataGrid.Columns[dataGrid.ScatterCol2].Header;
                yCol.Margin = new Thickness(5);
                yCol.FontWeight = FontWeights.Bold;
                yCol.FontSize = 12;
                yLabels.Children.Add(yCol);
                Grid.SetRow(yCol, 0);
                Grid.SetColumn(yCol, 0);
                Grid.SetRowSpan(yCol, scaleNumber);

                double scaleStepX = (dataGrid.Columns[dataGrid.ScatterCol1].Max - dataGrid.Columns[dataGrid.ScatterCol1].Min) / (this.scaleNumber - 1);
                double scaleStepY = (dataGrid.Columns[dataGrid.ScatterCol2].Max - dataGrid.Columns[dataGrid.ScatterCol2].Min) / (this.scaleNumber - 1);
                double scaleTextX;
                double scaleTextY;
                if (dataGrid.Columns[dataGrid.ScatterCol1].DirUp) {
                    scaleTextX = dataGrid.Columns[dataGrid.ScatterCol1].Min;
                } else {
                    scaleTextX = dataGrid.Columns[dataGrid.ScatterCol1].Max;
                }
                if (dataGrid.Columns[dataGrid.ScatterCol2].DirUp) {
                    scaleTextY = dataGrid.Columns[dataGrid.ScatterCol2].Max;
                } else {
                    scaleTextY = dataGrid.Columns[dataGrid.ScatterCol2].Min;
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
                    if (dataGrid.Columns[dataGrid.ScatterCol1].DirUp) {
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
                    if (dataGrid.Columns[dataGrid.ScatterCol2].DirUp) {
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
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                    img.MouseDown += btn_Click;
                    Canvas.SetTop(img, 2);
                    Canvas.SetRight(img, 2);
                    img.Tag = dataGrid.Columns[x];
                    img.Source = new BitmapImage(new Uri("Icons/cross.png", UriKind.Relative));
                    rect.Children.Add(img);
                    labels.Children.Add(rect);
                    rect.IsMouseDirectlyOverChanged += rect_IsMouseDirectlyOverChanged;
                    img.Visibility = Visibility.Collapsed;
                    rect.Tag = img;
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
                            Column[] cols = new Column[2];
                            cols[0] = dataGrid.Columns[col];
                            cols[1] = dataGrid.Columns[row];
                            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                            img.Source = new BitmapImage(new Uri("Icons/zoom_in.png", UriKind.Relative));
                            img.Tag = cols;
                            Canvas.SetTop(img, 2);
                            Canvas.SetRight(img, 2);
                            img.MouseDown += btnZoom_Click;
                            rect.Children.Add(img);
                            labels.Children.Add(rect);
                            rect.IsMouseDirectlyOverChanged += rect_IsMouseDirectlyOverChanged;
                            img.Visibility = Visibility.Collapsed;
                            rect.Tag = img;
                            rect.Background = System.Windows.Media.Brushes.Transparent;
                        }
                    }                
                }
            }
            
        }

        void btnZoom_Click(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Image btn = sender as System.Windows.Controls.Image;
            if (btn == null) return;
            Column[] cols = btn.Tag as Column[];
            if (cols == null) return;
            dataGrid.IsScatterplot = true;
            dataGrid.ScatterCol1 = dataGrid.GetIndex(cols[0]);
            dataGrid.ScatterCol2 = dataGrid.GetIndex(cols[1]);
            step = (double)1 / dataGrid.Columns.Length;
            SetLabels();
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state) {
                if (visImage.ActualHeight != 0 && visImage.ActualWidth != 0) {
                    wfPA.Height = (int)visImage.ActualHeight;
                    wfPA.Width = (int)visImage.ActualWidth;
                }
                RenderScene();
                visImage.Source = TakeScreenshot();
                dataGrid.Cache[this.GetType()] = visImage.Source;
                return null;
            }), null);
        }

        void btn_Click(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Image btn = sender as System.Windows.Controls.Image;
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
            System.Windows.Controls.Image btn = cGrid.Tag as System.Windows.Controls.Image;
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
            if (MainData.MainWindow.visualizationLayer.VisualizationData is LinkItem) {
                LinkItem link = MainData.MainWindow.visualizationLayer.VisualizationData as LinkItem;
                if (link.IsCombined) comp = link.CombItem;
            }
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
            if (visImage.ActualHeight != 0 && visImage.ActualWidth != 0) {
                wfPA.Height = (int)visImage.ActualHeight;
                wfPA.Width = (int)visImage.ActualWidth;
            }
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

        private void visImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            startPoint = e.GetPosition(this);
            endPoint = startPoint;
        }

        private void visImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                endPoint = e.GetPosition(this);
                DrawRubberBand();
            }
        }

        private void visImage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            int sx;
            int sy;
            int ex;
            int ey;
            int w;
            int h;
            int mx;
            int my;
            if (dataGrid.IsScatterplot) {
                sx = (int)((startPoint.X - yLabels.ActualWidth - visImage.Margin.Left) * wfPA.Width / visImage.ActualWidth);
                sy = (int)((startPoint.Y - visImage.Margin.Top) * wfPA.Height / visImage.ActualHeight);
                ex = (int)((endPoint.X - yLabels.ActualWidth - visImage.Margin.Left) * wfPA.Width / visImage.ActualWidth);
                ey = (int)((endPoint.Y - visImage.Margin.Top) * wfPA.Height / visImage.ActualHeight);
                w = (int)Math.Abs(ex - sx);
                h = (int)Math.Abs(ey - sy);
                if (w < 5) w = 5;
                if (h < 5) h = 5;
                mx = (ex - sx) / 2 + sx;
                my = (ey - sy) / 2 + sy;
            } else {
                sx = (int)((startPoint.X - visImage.Margin.Left) * wfPA.Width / visImage.ActualWidth);
                sy = (int)((startPoint.Y - visImage.Margin.Top) * wfPA.Height / visImage.ActualHeight);
                ex = (int)((endPoint.X - visImage.Margin.Left) * wfPA.Width / visImage.ActualWidth);
                ey = (int)((endPoint.Y - visImage.Margin.Top) * wfPA.Height / visImage.ActualHeight);
                w = (int)Math.Abs(ex - sx);
                h = (int)Math.Abs(ey - sy);
                if (w < 5) w = 5;
                if (h < 5) h = 5;
                mx = (ex - sx) / 2 + sx;
                my = (ey - sy) / 2 + sy;
            }
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
            int[] selectBuffer = new int[dataGrid.MaxPoints * 4 * (dataGrid.Columns.Length * (dataGrid.Columns.Length - 1))];
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
    }
}
