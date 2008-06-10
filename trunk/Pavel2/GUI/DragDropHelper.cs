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

        static private AdornerLayer parentAdorner;
        static private UIElement elementToHighlight;

        public static void HighlightElement(UIElement element) {
            elementToHighlight = element;
            parentAdorner = AdornerLayer.GetAdornerLayer(elementToHighlight);
            parentAdorner.Add(new ElementAdorner(elementToHighlight));
        }

        public static void RemoveAdorners() {
            Adorner[] toRemoveArray = parentAdorner.GetAdorners(elementToHighlight);
            if (toRemoveArray != null) {
                for (int x = 0; x < toRemoveArray.Length; x++) {
                    parentAdorner.Remove(toRemoveArray[x]);
                }
            }
        }

        public static void DoDragDrop(DependencyObject dragSource, object data, DragDropEffects effect, UIElement target) {
            HighlightElement(target);
            DragDrop.DoDragDrop(dragSource, data, effect);
            Mouse.AddQueryCursorHandler(target, MouseUP);
            Mouse.Capture(target);
        }

        private static void MouseUP(object sender, MouseEventArgs args) {
            RemoveAdorners();
            Mouse.Capture(null);
        }

        private class ElementAdorner : Adorner {

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);

            public ElementAdorner(UIElement adornedElement) : base(adornedElement) {
                renderBrush.Opacity = 0.2;
            }

            public ElementAdorner(UIElement adornedElement, Color brush, Color border, double opac) : base(adornedElement) {
                renderBrush.Opacity = opac;
                renderBrush = new SolidColorBrush(brush);
                renderPen = new Pen(new SolidColorBrush(border), 1.5);
            }

            protected override void OnRender(DrawingContext dc) {
                this.IsHitTestVisible = false;
                Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);
                
                dc.DrawRectangle(renderBrush, renderPen, adornedElementRect);
            }
        }

    }
}
