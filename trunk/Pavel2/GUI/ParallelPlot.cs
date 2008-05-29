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

        private List<Visual> visuals = new List<Visual>();

        public void Render() {
            ClearVisuals();
            dataGrid = MainData.CurrentDataGrid;
            if (dataGrid != null) {
                DrawAxes();
                DrawLines();
            }
        }

        private void DrawAxes() {
            DrawingVisual visual = new DrawingVisual();
            double step = this.ActualWidth / dataGrid.Columns.Length;
            using (DrawingContext dc = visual.RenderOpen()) {
                for (int i = 0; i < dataGrid.Columns.Length; i++) {
                    dc.DrawLine(axesPen, new Point(i*step, 0), new Point(i*step, this.ActualHeight));
                }
            }
            AddVisual(visual);
        }

        private void DrawLines() { 
            
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

    }
}
