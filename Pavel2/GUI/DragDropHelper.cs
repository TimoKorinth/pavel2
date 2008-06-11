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

        static private Color brushColor = Colors.Gray;
        static private Color borderColor = Colors.Turquoise;
        static private double opac = 0.4;

        public static void HighlightElement(UIElement element) {
            MainData.MainWindow.rootAdorner.AdornerLayer.Add(new ElementAdorner(element, brushColor, borderColor, opac));
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

        public static void DoDragDrop(DependencyObject dragSource, object data, DragDropEffects effect, UIElement target) {
            HighlightElement(target);
            if (target is LinkList) ((LinkList)target).newItemGrid.Visibility = Visibility.Visible;
            DragDrop.DoDragDrop(dragSource, data, effect);
            if (target is LinkList) ((LinkList)target).newItemGrid.Visibility = Visibility.Collapsed;
            RemoveAdorners();
        }

        private class ElementAdorner : Adorner {

            SolidColorBrush renderBrush;
            Pen renderPen;
            UIElement element;

            public ElementAdorner(UIElement adornedElement) 
                : base(adornedElement) {
            }

            public ElementAdorner(UIElement adornedElement, Color brush, Color border, double opac)
                : this(MainData.MainWindow.windowGrid) {
                this.element = adornedElement;
                renderBrush = new SolidColorBrush(brush);
                renderBrush.Opacity = opac;
                renderPen = new Pen(new SolidColorBrush(border), 10);
                renderPen.Brush.Opacity = opac;
            }

            protected override void OnRender(DrawingContext dc) {
                this.IsHitTestVisible = false;
                Rect e = new Rect(this.element.RenderSize);
                Rect w = new Rect(MainData.MainWindow.windowGrid.RenderSize);
                e.Location = this.element.TransformToAncestor(MainData.MainWindow.windowGrid).Transform(new Point(0,0));
                GeometryGroup group = new GeometryGroup();
                group.Children.Add(new RectangleGeometry(w));
                group.Children.Add(new RectangleGeometry(e));
                dc.DrawGeometry(renderBrush, new Pen(renderBrush, 0), group);
                dc.DrawGeometry(new SolidColorBrush(Colors.Transparent), renderPen, new RectangleGeometry(e));
            }
        }

    }
}
