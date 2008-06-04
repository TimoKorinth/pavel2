using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Pavel2.Framework;

namespace Pavel2.GUI {
    public class ParallelPlot : Canvas, Visualization {

        private Pen drawingPen = new Pen(Brushes.Turquoise, 1);
        private Pen axesPen = new Pen(Brushes.Black, 2);
        private Brush drawingBrush = Brushes.Black;
        private DataGrid dataGrid;
        private double step = 0.0;

        private List<Visual> visuals = new List<Visual>();

        public void Render() {
            ClearVisuals();
            dataGrid = MainData.CurrentDataGrid;
            if (dataGrid == null) return;
            step = this.ActualWidth / (dataGrid.Columns.Length - 1);
            if (dataGrid != null) {
                DrawAxes();
                DrawLines();
            }
        }

        private void DrawAxes() {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen()) {
                for (int i = 0; i < dataGrid.Columns.Length; i++) {
                    dc.DrawLine(axesPen, new Point(i*step, 0), new Point(i*step, this.ActualHeight));
                }
            }
            AddVisual(visual);
        }

        private double Normalize(double value, Column col) {
            if ((col.Max - col.Min) == 0) {
                return 0.5;
            }
            return ((value - col.Min) / (col.Max - col.Min));
        }

        private void DrawLines() {
            for (int row = 0; row < dataGrid.DoubleDataField.Length; row++) {
                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen()) {
                    Point startPoint = new Point();
                    for (int col = 0; col < dataGrid.Columns.Length; col++) {
                        double value = dataGrid.DoubleDataField[row][col];
                        if (value != double.NaN) {
                            double nValue = Normalize(value, dataGrid.Columns[col]);
                            if (startPoint == null) {
                                startPoint = new Point(col * step, this.ActualHeight * nValue);
                            } else {
                                dc.DrawLine(drawingPen, startPoint, new Point(col * step, this.ActualHeight * nValue));
                                startPoint = new Point(col * step, this.ActualHeight * nValue);
                            }
                            
                        }
                    }
                }
                AddVisual(visual);
            }
        }

        protected override int VisualChildrenCount {
            get { return visuals.Count; }
        }

        protected override Visual GetVisualChild(int index) {
            return visuals[index];
        }

        private void AddVisual(Visual visual) {
            visuals.Add(visual);
            
            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }

        private void DeleteVisual(Visual visual) {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }

        private void ClearVisuals() {
            for (int i = 0; i < visuals.Count; i++) {
                base.RemoveLogicalChild(visuals[i]);
                base.RemoveVisualChild(visuals[i]);
            }
            visuals.Clear();
        }

        public void RenderAfterResize() {
            Render();
        }
    }
}
