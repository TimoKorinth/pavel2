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
using System.Reflection;

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
            try {
                if (item.Tag is DirectoryInfo) {
                    fileList.ItemsSource = ((DirectoryInfo)item.Tag).GetFiles();
                } else if (item.Tag is DriveInfo) {
                    fileList.ItemsSource = ((DriveInfo)item.Tag).RootDirectory.GetFiles();
                }
            } catch (Exception fileExeption) {
                fileExeption.GetType();
            }
        }

        private void importButton_Click(object sender, RoutedEventArgs e) {
            if (fileList.SelectedItem != null) {
                FileInfo file = (FileInfo)fileList.SelectedItem;
                DataGrid dataGrid = ParserManagement.GetDataGrid(file.OpenText());
                if (null != dataGrid) {
                    dataGrid.Parser = ParserManagement.CurrentParser;
                    dataGrid.Name = file.Name;
                    projectTree.Items.Add(dataGrid);
                    SetSelectedItem(ref projectTree, dataGrid);
                    //Hier: Property Window für Import/Parser:
                    propertyGrid.SelectedObject = dataGrid.Parser;
                    propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(propertyGrid_PropertyValueChanged);
                } 
            }
        }

        void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e) {
            DataGrid dataGrid = (DataGrid)projectTree.SelectedItem;
            FileInfo file = (FileInfo)fileList.SelectedItem;
            MainData.RemoveColumns(dataGrid);
            projectTree.Items.Remove(dataGrid);
            DataGrid d = ParserManagement.GetDataGrid(file.OpenText(), dataGrid.Parser);
            d.Name = file.Name;
            d.Parser = ParserManagement.CurrentParser;
            projectTree.Items.Add(d);
        }

        public void SetSelectedItem(ref TreeView control, object item) {
            try {
                DependencyObject dObject = control
                    .ItemContainerGenerator
                    .ContainerFromItem(item);

                //uncomment the following line if UI updates are unnecessary
                //((TreeViewItem)dObject).IsSelected = true;                

                MethodInfo selectMethod =
                   typeof(TreeViewItem).GetMethod("Select",
                   BindingFlags.NonPublic | BindingFlags.Instance);

                selectMethod.Invoke(dObject, new object[] { true });
            } catch { }
        }

        private void projectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            DataGrid dataGrid = (DataGrid)projectTree.SelectedItem;
            StackPanel tmp;
            stackPanel2.Children.Clear();
            if (dataGrid != null) {
                for (int i = 0; i < dataGrid.Columns.Length; i++) {
                    tmp = new StackPanel();
                    for (int j = 0; j < dataGrid.Columns[i].Points.Length; j++) {
                        Label lab = new Label();
                        lab.Content = dataGrid.Columns[i].Points[j].Data;
                        tmp.Children.Add(lab);
                    }
                    stackPanel2.Children.Add(tmp);
                }
            }
        }
    }
}
