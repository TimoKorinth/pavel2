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

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für LinkList.xaml
    /// </summary>
    public partial class LinkList : UserControl {
        
        public LinkList() {
            InitializeComponent();
        }

        private void linkTreeView_Drop(object sender, DragEventArgs e) {
        }

        private void linkTreeView_DragEnter(object sender, DragEventArgs e) {
        }

        private void linkTreeView_DragLeave(object sender, DragEventArgs e) {
        }

        private void linkTreeView_DragOver(object sender, DragEventArgs e) {

        }

        private void newItemLabel_Drop(object sender, DragEventArgs e) {
            TreeViewItem tvItem = e.Data.GetData(typeof(TreeViewItem)) as TreeViewItem;
            if (tvItem == null) return;
            if (tvItem.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = tvItem.Tag as DataProjectTreeItem;
                LinkItem lItem = new LinkItem();
                lItem.AddDataItem(dPTI);
                TreeViewItem newItem = new TreeViewItem();
                newItem.Header = dPTI.Header;
                newItem.Tag = lItem;
                linkTreeView.Items.Add(newItem);
                UpdateLinkItem(newItem);
            }
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
    }
}
