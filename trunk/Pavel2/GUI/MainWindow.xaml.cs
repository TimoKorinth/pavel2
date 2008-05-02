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
using System.Data;

namespace Pavel2.GUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            InitDirectoryTree();
            propertyGridLayout.Visibility = Visibility.Collapsed;
        }

        private TreeViewItem lastAddedTreeViewItem;

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
                DataGrid dataGrid = ParserManagement.GetDataGrid(file);
                if (null != dataGrid) {
                    TreeViewItem tvItem = new TreeViewItem();
                    tvItem.Header = dataGrid.Name;
                    tvItem.Tag = dataGrid;
                        projectTree.Items.Add(tvItem);
                    this.lastAddedTreeViewItem = tvItem;
                    SetSelectedItem(ref projectTree, tvItem);
                    //Hier: Property Window für Import/Parser:
                    propertyGridLayout.Visibility = Visibility.Visible;
                    propertyGrid.SelectedObject = ParserManagement.CurrentParser;
                    propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(propertyGrid_PropertyValueChanged);
                    parserComboBox.ItemsSource = ParserManagement.ParserList;
                    parserComboBox.DisplayMemberPath = "Name";
                    parserComboBox.SelectedItem = ParserManagement.CurrentParser;
                } 
            }
        }

        private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e) {
            ParseAgain(parserComboBox.SelectedItem as Parser);
        }

        private void parserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!(parserComboBox.SelectedItem as Parser).Equals(ParserManagement.CurrentParser)) {
                ParseAgain(parserComboBox.SelectedItem as Parser);
            }
        }

        private void ParseAgain(Parser parser) {
            TreeViewItem item = (TreeViewItem)projectTree.SelectedItem;
            if (item != null) {
                DataGrid dataGrid = (DataGrid)item.Tag;
                MainData.RemoveColumns(dataGrid);
                DataGrid d = ParserManagement.GetDataGrid(parser);
                item.Header = d.Name;
                item.Tag = d;
                this.lastAddedTreeViewItem = item;
                DrawTable();
            }
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
            if (e.NewValue != null) {
                if (!e.NewValue.Equals(this.lastAddedTreeViewItem)) {
                    propertyGridLayout.Visibility = Visibility.Collapsed;
                }
            }
            DrawTable();
        }

        private void DrawTable() {
            TreeViewItem item = (TreeViewItem)projectTree.SelectedItem;
            if (item != null) {
                DataGrid dataGrid = (DataGrid)item.Tag;
                tableListView.ItemsSource = dataGrid.Data;
                GridView gView = new GridView();
                for (int i = 0; i < dataGrid.Columns.Length; i++) {
                    GridViewColumn gColumn = new GridViewColumn();
                    gColumn.Header = dataGrid.Columns[i].Header;
                    Binding bind = new Binding();
                    bind.Path = new PropertyPath("[" + i + "]");
                    gColumn.DisplayMemberBinding = bind;
                    gView.Columns.Add(gColumn);
                }
                tableListView.View = gView;
            }
        }
    }
}
