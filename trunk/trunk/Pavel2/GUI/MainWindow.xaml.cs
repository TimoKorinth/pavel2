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
using System.IO;
using Pavel2.Framework;

namespace Pavel2.GUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            InitDirectoryTree();
        }

        private void InitDirectoryTree() {
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                TreeViewItem item = new TreeViewItem();
                item.Tag = drive;
                item.Header = drive.ToString();

                item.Items.Add("*");
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
                    newItem.Header = subDir.ToString();
                    newItem.Items.Add("*");
                    item.Items.Add(newItem);
                }
            } catch {
                // An exception could be thrown in this code if you don't
                // have sufficient security permissions for a file or directory.
                // You can catch and then ignore this exception.
            }
        }

        private void directoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeViewItem item = (TreeViewItem)directoryTree.SelectedItem;
            if (item.Tag is DirectoryInfo) {
                fileList.ItemsSource = ((DirectoryInfo)item.Tag).GetFiles();
            } else if (item.Tag is DriveInfo) {
                fileList.ItemsSource = ((DriveInfo)item.Tag).RootDirectory.GetFiles();
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e) {
            FileInfo file = (FileInfo)fileList.SelectedItem;
            DataGrid dataGrid = ParserManagement.GetDataGrid(file.OpenText());
            dataGrid.Name = file.Name;
            projectTree.Items.Add(dataGrid);
        }

        private void projectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            DataGrid dataGrid = (DataGrid)projectTree.SelectedItem;
            listViewTable.ItemsSource = dataGrid.Columns[0].Points;
            listViewTable.DisplayMemberPath = "Data";
        }
    }
}
