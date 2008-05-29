using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Pavel2.GUI {
    public class ParallelPlot : Canvas, Visualization {

        private Pen drawingPen = new Pen(Brushes.Turquoise, 2);
        private Brush drawingBrush = Brushes.Black;

        private List<Visual> visuals = new List<Visual>();

        public ParallelPlot() {
            Render();
        }

        public void Render() {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen()) {
                dc.DrawLine(drawingPen, new Point(10,10), new Point(200,200));
            }
            AddVisual(visual);
        }

        protected override int VisualChildrenCount {
            get {
                return visuals.Count;
            }
        }

        protected override Visual GetVisualChild(int index) {
            return visuals[index];
        }

        public void AddVisual(Visual visual) {
            visuals.Add(visual);
            
            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }

        public void DeleteVisual(Visual visual) {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }

    }
}
