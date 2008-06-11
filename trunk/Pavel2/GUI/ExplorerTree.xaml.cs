using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Pavel2.Framework;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für ExplorerTree.xaml
    /// </summary>
    public partial class ExplorerTree : UserControl {
        public ExplorerTree() {
            InitializeComponent();
            InitDirectoryTree();
        }

        private void InitDirectoryTree() {
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                TreeViewItem item = new TreeViewItem();
                item.Tag = drive;
                item.Header = drive.Name;

                TreeViewItem t = new TreeViewItem();
                t.Header = "*";
                item.Items.Add(t);
                directoryTree.Items.Add(item);
            }
        }

        private void directoryTree_Expanded(object sender, RoutedEventArgs e) {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            DirectoryInfo dir;
            if (item.Tag is DriveInfo) {
                DriveInfo drive = (DriveInfo)item.Tag;
                dir = drive.RootDirectory;
            } else {
                dir = (DirectoryInfo)item.Tag;
            }
            try {
                foreach (DirectoryInfo subDir in dir.GetDirectories()) {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir.Name;
                    try {
                        TreeViewItem t = new TreeViewItem();
                        t.Header = "*";
                        if (subDir.GetDirectories().Length != 0 || subDir.GetFiles().Length != 0) newItem.Items.Add(t);
                        if (subDir.Attributes != (FileAttributes.System | FileAttributes.Hidden | FileAttributes.Directory)) {
                            item.Items.Add(newItem);
                        }
                    } catch {
                    }
                }
                foreach (FileInfo file in dir.GetFiles()) {
                    TreeViewItem tvItem = new TreeViewItem();
                    tvItem.Tag = file;
                    tvItem.Header = file.Name;
                    item.Items.Add(tvItem);
                }
            } catch {
            }
        }

        private void directoryTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = e.Source as TreeViewItem;
            if (item != null && item.Tag is FileInfo) {
                item.IsSelected = true;
                DragDropHelper.DoDragDrop(directoryTree, (FileInfo)item.Tag, DragDropEffects.Copy, MainData.MainWindow.projectTreeView);
                //DragDrop.DoDragDrop(directoryTree, (FileInfo)item.Tag, DragDropEffects.Copy);
            }
        }

    }
}
