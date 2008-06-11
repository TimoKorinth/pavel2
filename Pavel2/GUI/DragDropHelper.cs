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

        static private Color brushColor = Colors.Turquoise;
        static private Color borderColor = Colors.Black;
        static private double opac = 0.3;

        public static void HighlightElement(UIElement element) {
            elementToHighlight = element;
            parentAdorner = AdornerLayer.GetAdornerLayer(elementToHighlight);
            if (parentAdorner == null) return;
            parentAdorner.Add(new ElementAdorner(elementToHighlight, brushColor, borderColor, opac));
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
            RemoveAdorners();
        }

        private class ElementAdorner : Adorner {

            SolidColorBrush renderBrush;
            Pen renderPen;

            public ElementAdorner(UIElement adornedElement) : base(adornedElement) {
            }

            public ElementAdorner(UIElement adornedElement, Color brush, Color border, double opac) : this(adornedElement) {
                renderBrush = new SolidColorBrush(brush);
                renderBrush.Opacity = opac;
                renderPen = new Pen(new SolidColorBrush(border), 1.5);
                renderPen.Brush.Opacity = opac;
            }

            protected override void OnRender(DrawingContext dc) {
                this.IsHitTestVisible = false;
                Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);
                
                dc.DrawRectangle(renderBrush, renderPen, adornedElementRect);
            }
        }

    }
}
