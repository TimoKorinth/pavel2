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
using System.Windows.Controls.Primitives;

namespace Pavel2.GUI
{

    delegate Point GetPositionDelegate(IInputElement element);

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private TreeViewItem root;

        public MainWindow() {
            InitializeComponent();
            InitDirectoryTree();
            propertyGridLayout.Visibility = Visibility.Collapsed;

            root = new TreeViewItem();
            root.Header = "Root";
            projectTree.Items.Add(root);
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
                AddTreeViewItem(file);
            }
        }

        private void AddTreeViewItem(FileInfo file) {
            DataGrid dataGrid = ParserManagement.GetDataGrid(file);
            if (null != dataGrid) {

                TreeViewItem tvItem = new TreeViewItem();
                tvItem.Header = dataGrid.Name;
                tvItem.Tag = dataGrid;
                this.root.Items.Add(tvItem);

                tvItem.IsSelected = true;
                //Hier: Property Window für Import/Parser:
                propertyGridLayout.Visibility = Visibility.Visible;
                propertyGrid.SelectedObject = ParserManagement.CurrentParser;
                propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(propertyGrid_PropertyValueChanged);
                parserComboBox.ItemsSource = ParserManagement.ParserList;
                parserComboBox.DisplayMemberPath = "Name";
                parserComboBox.SelectedItem = ParserManagement.CurrentParser;
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
                if (d != null) {
                    item.Header = d.Name;
                    item.Tag = d;
                    DrawTable();
                }
            }
        }

        private void projectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            propertyGridLayout.Visibility = Visibility.Collapsed;
            DrawTable();
        }

        private void DrawTable() {
            TreeViewItem item = (TreeViewItem)projectTree.SelectedItem;
            if (item != null) {
                DataGrid dataGrid = (DataGrid)item.Tag;
                if (dataGrid != null) {
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

        private void button5_Click(object sender, RoutedEventArgs e) {
            propertyGridLayout.Visibility = Visibility.Collapsed;
        }

        private void fileList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            int index = GetFileListIndex(e.GetPosition);
            fileList.SelectedIndex = index;
            FileInfo file = fileList.Items[index] as FileInfo;
            DragDrop.DoDragDrop(fileList, file, DragDropEffects.Copy);
        }

        private void projectTree_Drop(object sender, DragEventArgs e) {
            FileInfo file = (FileInfo)e.Data.GetData("System.IO.FileInfo");
            AddTreeViewItem(file);
        }

        private ListViewItem GetListViewItem(int index) {
            if (fileList.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return fileList.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        private int GetFileListIndex(GetPositionDelegate getPosition) {
            int index = -1;
            for (int i = 0; i < fileList.Items.Count; ++i) {
                ListViewItem item = GetListViewItem(i);
                if (this.IsMouseOverTarget(item, getPosition)) {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition) {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }
    }
}
