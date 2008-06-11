using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using Pavel2.Framework;

namespace Pavel2.GUI {
    public class MultiTreeView : TreeView {

        private List<TreeViewItem> selItems = new List<TreeViewItem>();
        private AdornerLayer adornerLayer;
        private Boolean isDrawing;
        private Point startPoint;

        public AdornerLayer AdornerLayerLocal {
            get {
                adornerLayer = AdornerLayer.GetAdornerLayer(this);
                return adornerLayer; 
            }
            set { adornerLayer = value; }
        }

        private bool CtrlPressed {
            get { return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl); }
        }

        public MultiTreeView() : base() {
            this.SelectedItemChanged += TreeView_SelectedItemChanged;
            this.MouseDown += MouseDownHandler;
            this.MouseUp += MouseUpHandler;
            this.MouseMove += MouseMoveHandler;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeViewItem item = this.SelectedItem as TreeViewItem;
            if (item == null) return;
            item.IsSelected = false;
            if (!CtrlPressed) {
                List<TreeViewItem> selectedTreeViewItemList = new List<TreeViewItem>();
                foreach (TreeViewItem treeViewItem1 in selItems) {
                    selectedTreeViewItemList.Add(treeViewItem1);
                }

                foreach (TreeViewItem treeViewItem1 in selectedTreeViewItemList) {
                    Deselect(treeViewItem1);
                }
            }
            ChangeSelectedState(item);
        }

        void Deselect(TreeViewItem treeViewItem) {
            treeViewItem.Background = Brushes.White;
            treeViewItem.Foreground = Brushes.Black;
            selItems.Remove(treeViewItem);
        }

        void ChangeSelectedState(TreeViewItem treeViewItem) {
            if (!selItems.Contains(treeViewItem)) {
                treeViewItem.Background = Brushes.Silver;
                treeViewItem.Foreground = Brushes.Black;
                selItems.Add(treeViewItem);
            } else {
                Deselect(treeViewItem);
            }
        }

        private void MouseDownHandler(Object sender, MouseButtonEventArgs e) {
            startPoint = e.GetPosition(this);
            isDrawing = true;
        }

        private void MouseMoveHandler(Object sender, MouseEventArgs e) {
            if (isDrawing && e.LeftButton == MouseButtonState.Pressed) {
                Point endPoint = e.GetPosition(this);
                DrawRubberBand(endPoint);
            }
        }

        private void MouseUpHandler(Object sender, MouseButtonEventArgs e) {
            startPoint = e.GetPosition(this);
            isDrawing = false;
            RemoveAdornerArray();
        }

        private void DrawRubberBand(Point end) {
            RemoveAdornerArray();
            AdornerLayerLocal.Add(new RubberBandAdorner(this, startPoint, end));
        }

        private void RemoveAdornerArray() {
            Adorner[] toRemoveArray = AdornerLayerLocal.GetAdorners(this);
            if (toRemoveArray != null) {
                for (int x = 0; x < toRemoveArray.Length; x++) {
                    AdornerLayerLocal.Remove(toRemoveArray[x]);
                }
            }
        }



        private class RubberBandAdorner : Adorner {

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Transparent);
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Silver), 1);
            Point start;
            Point end;

            public RubberBandAdorner(UIElement adornedElement, Point start, Point end) : base(adornedElement) {
                this.start = start;
                this.end = end;
            }

            protected override void OnRender(DrawingContext dc) {
                this.IsHitTestVisible = false;
                Rect e = new Rect(start, end);
                dc.DrawRectangle(renderBrush, renderPen, e);
            }
        }

    }
}
