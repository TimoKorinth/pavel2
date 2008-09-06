﻿using System;
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
        private double step;
        private int scaleNumber = 5;
        WindowsFormsHost host = new WindowsFormsHost();

        public ParallelPlot() {
            InitializeComponent();
            this.SizeChanged += ParallelPlot_SizeChanged;
            wfPA = new OpenGLRenderWind();
            host.Child = wfPA;
            wfPA.Width = 1000;
            wfPA.Height = 800;
            wfPA.SetupViewPort();
        }

        void ParallelPlot_SizeChanged(object sender, SizeChangedEventArgs e) {
            for (int i = 0; i < thumbGrid.Children.Count; i++) {
                Thumb t = (Thumb)thumbGrid.Children[i];
                t.Margin = new Thickness(step * i * (thumbGrid.ActualWidth-10), 2, t.Margin.Right, 2);
            }
        }

        private void DrawLines() {
            Gl.glEnable(Gl.GL_LINE_SMOOTH);
            if (dataGrid.Columns[dataGrid.MaxColumn].Points.Length < 100) {
                Gl.glLineWidth(6f);
                Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.5f));
            }
            if (dataGrid.Columns[dataGrid.MaxColumn].Points.Length > 100) {
                Gl.glLineWidth(3f);
                Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.2f));
            }
            if (dataGrid.Columns[dataGrid.MaxColumn].Points.Length > 1000) {
                Gl.glLineWidth(1f);
                Gl.glColor4fv(ColorManagement.UnselectedColor.RGBwithA(0.02f));
            }
            bool breakLine = false;
            if (dataGrid == null) return;
            int index = -1;
            for (int row = 0; row < dataGrid.Columns[dataGrid.MaxColumn].Points.Length; row++) {
                if (comp != null) {
                    if (comp.GetDataItemIndex(row) != index) {
                        index = comp.GetDataItemIndex(row);
                        Gl.glColor4fv(ColorManagement.GetColor(index+2).RGBwithA(0.2f));
                    }
                }
                Gl.glBegin(Gl.GL_LINE_STRIP);
                for (int col = 0; col < dataGrid.Columns.Length; col++) {
                    if (dataGrid.DoubleDataField[row][col] == double.NaN) {
                        Gl.glEnd();
                        breakLine = true;
                    } else {
                        if (breakLine) Gl.glBegin(Gl.GL_LINE_STRIP);
                        double nValue = Normalize(dataGrid.DoubleDataField[row][col], dataGrid.Columns[col]);
                        if (dataGrid.Columns[col].DirUp) {
                            Gl.glVertex2d(step * col, nValue);
                        } else {
                            Gl.glVertex2d(step * col, 1-nValue);
                        }
                    }
                }
                if (!breakLine) Gl.glEnd();
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
            for (int col = 0; col < dataGrid.Columns.Length-1; col++) {
                ColumnDefinition colDef = new ColumnDefinition();
                overlayControls.ColumnDefinitions.Add(colDef);

                Canvas cGrid = new Canvas();
                cGrid.IsMouseDirectlyOverChanged += cGrid_IsMouseDirectlyOverChanged;
                cGrid.Background = System.Windows.Media.Brushes.Transparent;
                overlayControls.Children.Add(cGrid);
                Grid.SetColumn(cGrid, col);

                WrapPanel wPanel = new WrapPanel();
                Button b1 = new Button();
                b1.Content = "But1";
                wPanel.Children.Add(b1);
                Button b2 = new Button();
                b2.Content = "But2";
                wPanel.Children.Add(b2);

                cGrid.Tag = wPanel;
                cGrid.Children.Add(wPanel);
                wPanel.Visibility = Visibility.Collapsed;
            }
        }

        void cGrid_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Canvas cGrid = sender as Canvas;
            if (cGrid == null) return;
            WrapPanel wPanel = cGrid.Tag as WrapPanel;
            if (wPanel == null) return;
            if ((bool)e.NewValue) {
                wPanel.Visibility = Visibility.Visible;
            } else {
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

        private void SetThumbPanel() {
            thumbGrid.Children.Clear();
            for (int col = 0; col < dataGrid.Columns.Length; col++) {
                Thumb t = new Thumb();
                t.DragStarted += DragStarted;
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
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state) {
                    t.Margin = new Thickness(step * (int)state * (thumbGrid.ActualWidth-10), 2, t.Margin.Right, 2);
                    return null;
                }), col);
                thumbGrid.Children.Add(t);
            }
        }

        void DragDelta(object sender, DragDeltaEventArgs e) {
            Thumb t = sender as Thumb;
            if (t == null) return;
            if ((t.Margin.Left + e.HorizontalChange) < -1) return;
            t.Margin = new Thickness(t.Margin.Left + e.HorizontalChange, t.Margin.Top, t.Margin.Right, t.Margin.Bottom);

            line.Visibility = Visibility.Visible;
            line.Margin = new Thickness(line.Margin.Left + e.HorizontalChange, line.Margin.Top, line.Margin.Right, line.Margin.Bottom);
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
                    pos = (int)Math.Ceiling((t.Margin.Left / (step * thumbGrid.ActualWidth)));
                } else {
                    pos = (int)((t.Margin.Left / (step * thumbGrid.ActualWidth)));
                }
                dataGrid.ChangeColOrder(col, pos);
                SetLabelPanel();
                SetThumbPanel();
                line.Visibility = Visibility.Collapsed;
            }

            wfPA.Height = (int)this.ActualHeight;
            wfPA.Width = (int)this.ActualWidth;
            RenderScene();
            visImage.Source = TakeScreenshot();
        }

        void DragStarted(object sender, DragStartedEventArgs e) {
            Thumb t = sender as Thumb;
            if (t == null) return;
            t.Background = System.Windows.Media.Brushes.Turquoise;
            line.Margin = new Thickness(t.Margin.Left, line.Margin.Top, line.Margin.Right, line.Margin.Bottom);
        }

        #region Visualization Member

        public void Render(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            comp = MainData.MainWindow.visualizationLayer.VisualizationData as CombinedDataItem;
            if (dataGrid == null) return;
            step = (double)1 / (dataGrid.Columns.Length - 1);
            SetLabelPanel();
            SetThumbPanel();
            SetOverlayControls();
            //Abfrage ob sich was geändert hat, sonst einfach den evtl. schon
            //vorhandenen Screenshot nehmen:
            if (!dataGrid.Cache.ContainsKey(this.GetType()) || this.dataGrid.Changed) {
                this.dataGrid.Changed = false;
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
            SetThumbPanel();
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
    }
}
