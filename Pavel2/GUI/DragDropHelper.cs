using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using Pavel2.Framework;
using System.Windows.Input;

namespace Pavel2.GUI {
    static class DragDropHelper {

        static private Color brushColor = Colors.Cyan;
        static private Color borderColor = Colors.Turquoise;
        static private double opac = 0.35;

        public static void HighlightElement(UIElement[] elements) {
            MainData.MainWindow.rootAdorner.AdornerLayer.Add(new ElementAdorner(elements, brushColor, borderColor, opac));
        }

        public static void RemoveAdorners() {
            RemoveAdornerArray(MainData.MainWindow.rootAdorner.AdornerLayer.GetAdorners(MainData.MainWindow.windowGrid));
        }

        private static void RemoveAdornerArray(Adorner[] toRemoveArray) {
            if (toRemoveArray != null) {
                for (int x = 0; x < toRemoveArray.Length; x++) {
                    MainData.MainWindow.rootAdorner.AdornerLayer.Remove(toRemoveArray[x]);
                }
            }
        }

        public static void DoDragDrop(DependencyObject dragSource, object data, DragDropEffects effect, params UIElement[] targets) {
            HighlightElement(targets);
            DragDrop.DoDragDrop(dragSource, data, effect);
            RemoveAdorners();
        }

        private class ElementAdorner : Adorner {

            SolidColorBrush renderBrush;
            Pen renderPen;
            UIElement[] elements;

            public ElementAdorner(UIElement adornedElement, Color brush, Color border, double opac)
                : base(MainData.MainWindow.windowGrid) {
                this.elements = new UIElement[1];
                this.elements[0] = adornedElement;
                InitBrushes(brush, border, opac);
            }

            public ElementAdorner(UIElement[] adornedElements, Color brush, Color border, double opac)
                : base(MainData.MainWindow.windowGrid) {
                this.elements = adornedElements;
                InitBrushes(brush, border, opac);
            }

            private void InitBrushes(Color brush, Color border, double opac) {
                renderBrush = new SolidColorBrush(brush);
                renderBrush.Opacity = opac;
                renderPen = new Pen(new SolidColorBrush(border), 10);
                renderPen.Brush.Opacity = opac+0.25;
            }

            protected override void OnRender(DrawingContext dc) {
                this.IsHitTestVisible = false;
                Rect window = new Rect(MainData.MainWindow.windowGrid.RenderSize);
                GeometryGroup group = new GeometryGroup();
                group.Children.Add(new RectangleGeometry(window));
                foreach (UIElement item in elements) {
                    Rect e = new Rect(item.RenderSize);
                    e.Location = item.TransformToAncestor(MainData.MainWindow.windowGrid).Transform(new Point(0, 0));
                    group.Children.Add(new RectangleGeometry(e));
                    dc.DrawGeometry(new SolidColorBrush(Colors.Transparent), renderPen, new RectangleGeometry(e));
                }
                dc.DrawGeometry(renderBrush, new Pen(renderBrush, 0), group);
            }
        }

    }
}
