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
        private TreeViewItem lastModifiedItem;

        public MainWindow() {
            InitializeComponent();
            InitDirectoryTree();
            propertyGridLayout.Visibility = Visibility.Collapsed;

            root = new TreeViewItem();
            root.Header = "Root";
            FolderProjectTreeItem fPTI = new FolderProjectTreeItem(root);
            root.Tag = fPTI;
            projectTree.Items.Add(root);
            root.IsSelected = true;
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
                TreeViewItem projItem = (TreeViewItem)projectTree.SelectedItem;
                AddDataProjectTreeItem(file, projItem);
            }
        }

        private void AddDataProjectTreeItem(FileInfo file, TreeViewItem rootItem) {
            int insertIndex = -1;
            if (rootItem.Tag is DataProjectTreeItem) {
                TreeViewItem tmp = rootItem;
                rootItem = (TreeViewItem)rootItem.Parent;
                insertIndex = rootItem.Items.IndexOf(tmp);
            }
            if (rootItem.Tag is FolderProjectTreeItem) {
                DataGrid dataGrid = ParserManagement.GetDataGrid(file);
                if (null != dataGrid) {

                    TreeViewItem tvItem = new TreeViewItem();
                    tvItem.Header = dataGrid.Name;
                    for (int i = 0; i < dataGrid.Columns.Length; i++) {
                        TreeViewItem tmp = new TreeViewItem();
                        String header = dataGrid.Columns[i].Header;
                        if (header != "") {
                            tmp.Header = header;
                        } else {
                            tmp.Header = i;
                        }
                        tvItem.Items.Add(tmp);
                    }
                    DataProjectTreeItem dPTI = new DataProjectTreeItem(dataGrid);
                    tvItem.Tag = dPTI;
                    if (insertIndex < 0) {
                        rootItem.Items.Add(tvItem);
                    } else {
                        rootItem.Items.Insert(insertIndex, tvItem);
                    }
                    tvItem.IsSelected = true;
                    rootItem.IsExpanded = true;

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
            if (item != null && item.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                DataGrid dataGrid = dPTI.DataGrid;
                MainData.RemoveColumns(dataGrid);
                DataGrid d = ParserManagement.GetDataGrid(parser);
                dPTI.DataGrid = d;
                if (d != null) {
                    item.Header = d.Name;
                    item.Tag = dPTI;
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
            if (item != null && item.Tag != null) {
                ProjectTreeItem pTI = (ProjectTreeItem)item.Tag;
                DataGrid dataGrid = pTI.DataGrid;
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
                } else {
                    tableListView.ItemsSource = null;
                    tableListView.View = null;
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
            this.lastModifiedItem.Background = null;
            FileInfo file = (FileInfo)e.Data.GetData("System.IO.FileInfo");
            TreeViewItem item = GetProjectTreeItem(e.GetPosition, this.root);
            AddDataProjectTreeItem(file, item);
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

        //TODO: Wenn nicht im Gebiet des TreeView geklickt wird, wird immer root ausgewählt
        //evtl. dann einfach das schon ausgewählte Object im Baum nehmen
        private TreeViewItem GetProjectTreeItem(GetPositionDelegate getPosition, TreeViewItem rootItem) {
            if (IsMouseOverTarget(rootItem, getPosition)) {
                foreach (TreeViewItem item in rootItem.Items) {
                    if (IsMouseOverTarget(item, getPosition)) {
                        return GetProjectTreeItem(getPosition, item);
                    }
                }
            }
            return rootItem;
        }

        private void projectTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = GetProjectTreeItem(e.GetPosition, this.root);
            if (item != null) item.IsSelected = true;
        }

        private void AddNewFolder(object sender, RoutedEventArgs e) {
            TreeViewItem item = (TreeViewItem)projectTree.SelectedItem;
            if (item.Tag is DataProjectTreeItem) {
                item = (TreeViewItem)item.Parent;
            }
            if (item.Tag is FolderProjectTreeItem) {
                TreeViewItem newItem = new TreeViewItem();
                newItem.Header = "Folder";
                FolderProjectTreeItem fPTI = new FolderProjectTreeItem(newItem);
                newItem.Tag = fPTI;
                item.Items.Add(newItem);
                newItem.IsSelected = true;
                item.IsExpanded = true;
            }
        }

        private void RemoveItem(object sender, RoutedEventArgs e) {
            TreeViewItem item = (TreeViewItem)projectTree.SelectedItem;
            ProjectTreeItem dPTI = (ProjectTreeItem)item.Tag;
            MainData.RemoveColumns(dPTI.DataGrid);
            dPTI.DataGrid = null;
            RemoveProjectTreeItem(item, this.root);
        }

        private void RemoveProjectTreeItem(TreeViewItem delItem, TreeViewItem rootItem) {
            foreach (TreeViewItem item in rootItem.Items) {
                RemoveProjectTreeItem(delItem, item);
            }
            rootItem.Items.Remove(delItem);
        }

        private void projectTree_DragOver(object sender, DragEventArgs e) {
            TreeViewItem item = GetProjectTreeItem(e.GetPosition, this.root);
            if (this.lastModifiedItem != null) this.lastModifiedItem.Background = null;
            item.Background = Brushes.Turquoise;
            this.lastModifiedItem = item;
        }
    }
}
