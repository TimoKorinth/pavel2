using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Pavel2.GUI {
    static class TreeViewHelper {

        public static TreeViewItem GetTreeViewItem(GetPositionDelegate getPosition, TreeView treeView) {
            if (IsMouseOverTarget(treeView, getPosition)) {
                foreach (TreeViewItem item in treeView.Items) {
                    if (IsMouseOverTarget(item, getPosition)) {
                        return GetTreeViewItem(getPosition, item);
                    }
                }
            }
            return null;
        }

        public static TreeViewItem GetTreeViewItem(GetPositionDelegate getPosition, TreeViewItem rootItem) {
            if (IsMouseOverTarget(rootItem, getPosition)) {
                foreach (TreeViewItem item in rootItem.Items) {
                    if (IsMouseOverTarget(item, getPosition)) {
                        return GetTreeViewItem(getPosition, item);
                    }
                }
            }
            return rootItem;
        }

        private static bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition) {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

    }
}
