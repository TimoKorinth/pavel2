﻿using System;
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
        private Point endPoint;
        private bool unselect = false;
        public bool clickFirst = true;
        public bool firstStart = true;

        public AdornerLayer AdornerLayerLocal {
            get {
                adornerLayer = AdornerLayer.GetAdornerLayer(this);
                return adornerLayer; 
            }
            set { adornerLayer = value; }
        }

        public List<TreeViewItem> SelectedItems {
            get {
                return selItems;
            }
        }

        public void SelectItem(TreeViewItem item) {
            if (!CtrlPressed && !isDrawing) {
                DeselectAll();
            }
            ChangeSelectedState(item);
        }

        new public object SelectedItem {
            get {
                if (base.SelectedItem != null) return base.SelectedItem;
                return LastSelectedItem;
            }
        }

        public TreeViewItem LastSelectedItem {
            get {
                if (selItems.Count > 0) {
                    return selItems[selItems.Count-1];
                }
                return null;
            }
        }

        private bool CtrlPressed {
            get { return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl); }
        }

        private bool ShiftPressed {
            get { return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftShift); }
        }

        public MultiTreeView() : base() {
            this.SelectedItemChanged += TreeView_SelectedItemChanged;
            this.MouseDown += MouseDownHandler;
            this.MouseUp += MouseUpHandler;
            this.MouseMove += MouseMoveHandler;
            this.PreviewMouseLeftButtonDown += MultiTreeView_PreviewMouseLeftButtonDown;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeViewItem item = base.SelectedItem as TreeViewItem;
            if (item == null) return;
            if (!clickFirst) {
                item.IsSelected = false;
                return;
            }
            if (isDrawing || unselect) e.Handled = true;
            if (!clickFirst) {
                if (!CtrlPressed && !isDrawing) {
                    DeselectAll();
                }
                ChangeSelectedState(item);
            }
            if (firstStart) {
                ChangeSelectedState(item);
                firstStart = false;
            }
            unselect = true;
            item.IsSelected = false;
            unselect = false;
            clickFirst = false;
        }

        private void DeselectAll() {
            List<TreeViewItem> selectedTreeViewItemList = new List<TreeViewItem>();
            foreach (TreeViewItem treeViewItem1 in selItems) {
                selectedTreeViewItemList.Add(treeViewItem1);
            }

            foreach (TreeViewItem treeViewItem1 in selectedTreeViewItemList) {
                Deselect(treeViewItem1);
            }
        }

        private void Deselect(TreeViewItem treeViewItem) {
            treeViewItem.Background = Brushes.Transparent;
            treeViewItem.Foreground = Brushes.Black;
            treeViewItem.FontWeight = FontWeights.Normal;
            //Hack to enable drag and drop on background of item:
            treeViewItem.IsTextSearchEnabled = false;
            selItems.Remove(treeViewItem);
        }

        private void ChangeSelectedState(TreeViewItem treeViewItem) {
            if (!selItems.Contains(treeViewItem)) {
                Select(treeViewItem);
            } else {
                if (!isDrawing) Deselect(treeViewItem);
            }
        }

        private void Select(TreeViewItem treeViewItem) {
            Color c1 = Color.FromRgb(157, 173, 204);
            Color c2 = Color.FromRgb(120, 138, 176);
            LinearGradientBrush b = new LinearGradientBrush(c1, c2, 90);
            treeViewItem.Background = b;
            //Hack to enable drag and drop on background of item:
            treeViewItem.IsTextSearchEnabled = true;
            treeViewItem.Foreground = Brushes.White;
            treeViewItem.FontWeight = FontWeights.Bold;
            selItems.Add(treeViewItem);
        }

        private void MouseDownHandler(Object sender, MouseButtonEventArgs e) {
            if (!CtrlPressed && e.RightButton != MouseButtonState.Pressed) {
                DeselectAll();
            }
            startPoint = e.GetPosition(this);
            isDrawing = true;
        }

        void MultiTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem tvItem = e.Source as TreeViewItem;
            clickFirst = true;
            if (tvItem == null) return;
            if (!selItems.Contains(tvItem)) {
                if (!CtrlPressed && !isDrawing) {
                    DeselectAll();
                }
                ChangeSelectedState(tvItem);
                tvItem.IsSelected = true;
            }
        }

        private void MouseMoveHandler(Object sender, MouseEventArgs e) {
            if (isDrawing && e.LeftButton == MouseButtonState.Pressed) {
                endPoint = e.GetPosition(this);
                DrawRubberBand();
                SetHitItems();
            }
        }

        private void MouseUpHandler(Object sender, MouseButtonEventArgs e) {
            startPoint = e.GetPosition(this);
            endPoint = startPoint;
            isDrawing = false;
            RemoveAdornerArray();
        }

        private void DrawRubberBand() {
            RemoveAdornerArray();
            AdornerLayerLocal.Add(new RubberBandAdorner(this, startPoint, endPoint));
        }

        private void RemoveAdornerArray() {
            Adorner[] toRemoveArray = AdornerLayerLocal.GetAdorners(this);
            if (toRemoveArray != null) {
                for (int x = 0; x < toRemoveArray.Length; x++) {
                    AdornerLayerLocal.Remove(toRemoveArray[x]);
                }
            }
        }

        private void SetHitItems() {
            RectangleGeometry region = new RectangleGeometry(new Rect(startPoint, endPoint));
            GeometryHitTestParameters parameters = new GeometryHitTestParameters(region);
            HitTestResultCallback callback = new HitTestResultCallback(this.HitTestCallback);
            VisualTreeHelper.HitTest(this, HitTestFilterFunc, callback, parameters);
        }

        public HitTestFilterBehavior HitTestFilterFunc(DependencyObject potentialHitTestTarget) {
            if (potentialHitTestTarget is TreeViewItem) {
                TreeViewItem item = potentialHitTestTarget as TreeViewItem;
                Rect itemBounds = VisualTreeHelper.GetDescendantBounds(item);
                GeneralTransform trans = item.TransformToAncestor(this);
                itemBounds.Location = trans.Transform(new Point(0, 0));
                Rect rubberBand = new Rect(startPoint, endPoint);
                if ((rubberBand.Contains(itemBounds.BottomLeft) && rubberBand.Contains(itemBounds.TopLeft)) ||
                    (rubberBand.Contains(itemBounds.BottomRight) && rubberBand.Contains(itemBounds.TopRight)) ||
                    (rubberBand.Contains(new Point((this.ActualWidth) / 2, itemBounds.BottomLeft.Y)) && rubberBand.Contains(new Point((this.ActualWidth) / 2, itemBounds.TopLeft.Y)))) {
                    ChangeSelectedState(item);
                } else {
                    Deselect(item);
                }
                return HitTestFilterBehavior.Continue;
            }
            return HitTestFilterBehavior.Continue;
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result) {
            return HitTestResultBehavior.Continue;
        }



        private class RubberBandAdorner : Adorner {

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Turquoise);
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Turquoise), 1);
            Point start;
            Point end;

            public RubberBandAdorner(UIElement adornedElement, Point start, Point end) : base(adornedElement) {
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

    }
}
