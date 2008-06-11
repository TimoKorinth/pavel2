using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace Pavel2.GUI {
    public class MultiTreeView : TreeView {

        private List<TreeViewItem> selItems = new List<TreeViewItem>();

        private bool CtrlPressed {
            get { return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl); }
        }

        public MultiTreeView() : base() {
            this.SelectedItemChanged += TreeView_SelectedItemChanged;
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

    }
}
