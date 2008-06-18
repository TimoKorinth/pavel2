using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Pavel2.Framework;
using System.Windows.Controls.Primitives;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für LinkList.xaml
    /// </summary>
    public partial class LinkList : UserControl {

        private TreeViewItem editItem;
        private TreeViewItem highlightedItem;
        private String oldHeader;

        public LinkList() {
            InitializeComponent();
        }

        private void linkTreeView_Drop(object sender, DragEventArgs e) {
            List<TreeViewItem> sendItems = e.Data.GetData(typeof(List<TreeViewItem>)) as List<TreeViewItem>;
            if (sendItems == null) return;
            TreeViewItem oldItem = e.Source as TreeViewItem;
            if (oldItem == null) return;
            if (oldItem.Tag is Column) oldItem = (TreeViewItem)oldItem.Parent;
            if (oldItem.Tag is DataProjectTreeItem) oldItem = (TreeViewItem)oldItem.Parent;
            
            foreach (TreeViewItem tvItem in sendItems) {
                if (tvItem.Tag is DataProjectTreeItem) {
                    if (oldItem.Tag is LinkItem) ((LinkItem)oldItem.Tag).AddDataItem(tvItem.Tag as DataProjectTreeItem);
                }
            }
            UpdateLinkItem(oldItem);
            MainData.MainWindow.UpdatePreviewPanel();
        }

        private void linkTreeView_DragOver(object sender, DragEventArgs e) {
            TreeViewItem item = e.Source as TreeViewItem;
            if (item != null) {
                item.HeaderTemplate = (DataTemplate)this.FindResource("HighlightTemplate"); ;
                if (this.highlightedItem != null && !this.highlightedItem.Equals(item)) {
                    this.highlightedItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                }
                this.highlightedItem = item;
            }
        }

        private void newItemGrid_Drop(object sender, DragEventArgs e) {
            List<TreeViewItem> tvItems = e.Data.GetData(typeof(List<TreeViewItem>)) as List<TreeViewItem>;
            if (tvItems == null) return;
            LinkItem lItem = new LinkItem();
            TreeViewItem newItem = new TreeViewItem();
            foreach (TreeViewItem tvItem in tvItems) {
                if (tvItem.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = tvItem.Tag as DataProjectTreeItem;
                    lItem.AddDataItem(dPTI);
                    if (lItem.Header == null) lItem.Header = dPTI.Header;
                } 
            }
            newItem.Tag = lItem;
            linkTreeView.Items.Add(newItem);
            UpdateLinkItem(newItem);
        }

        private void UpdateLinkItem(TreeViewItem item) {
            if (item.Tag is LinkItem) {
                item.Items.Clear();
                LinkItem lItem = (LinkItem)item.Tag;
                for (int i = 0; i < lItem.DataItems.Count; i++) {
                    TreeViewItem tmp = new TreeViewItem();
                    tmp.Tag = lItem.DataItems[i];
                    tmp.Header = lItem.DataItems[i].Header;
                    item.Items.Add(tmp);
                }
            }
        }
        
        private void linkTreeView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = linkTreeView.SelectedItem as TreeViewItem;
            if (item == null) return;
            if (!(item.Tag is LinkItem)) return;
            this.editItem = item;
            DataTemplate editTemplate = (DataTemplate)this.FindResource("EditTemplate");
            item.HeaderTemplate = editTemplate;
            this.oldHeader = ((LinkItem)item.Tag).Header;
        }

        private void linkTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (editItem != null) editItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
            editItem = null;
        }

        private void linkTreeView_KeyDown(object sender, KeyEventArgs e) {
            if (editItem != null) {
                if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape) {
                    if (e.Key == Key.Escape) {
                        if (editItem.Tag is LinkItem) {
                            ((LinkItem)editItem.Tag).Header = this.oldHeader;
                        }
                    }
                    editItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                    editItem = null;
                }
            }
        }
    }
}
