using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Pavel2.Framework;
using System.IO;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für ProjectTree.xaml
    /// </summary>
    public partial class ProjectTree : UserControl {

        #region Fields

        private TreeViewItem root;
        private TreeViewItem editItem;
        private TreeViewItem highlightedItem;
        private String oldHeader;

        #endregion

        #region Constructor

        public ProjectTree() {
            InitializeComponent();
            InitProjectTree();
        }

        #endregion

        #region Routed Events

        public static readonly RoutedEvent NewFileInsertedEvent;
        static ProjectTree() {
            NewFileInsertedEvent = EventManager.RegisterRoutedEvent(
                "NewFileInserted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ProjectTree));
        }
        public event RoutedEventHandler NewFileInserted {
            add { AddHandler(NewFileInsertedEvent, value); }
            remove { RemoveHandler(NewFileInsertedEvent, value); }
        }

        #endregion

        #region Properties

        public TreeViewItem SelectedItem {
            get { return projectTree.SelectedItem as TreeViewItem; }
        }

        #endregion

        #region Public Methods

        public void ParseAgain(Parser parser) {
            TreeViewItem item = this.SelectedItem;
            if (item != null && item.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                DataGrid dataGrid = dPTI.DataGrid;
                MainData.RemoveColumns(dataGrid);
                DataGrid d = ParserManagement.GetDataGrid(parser);
                if (d != null) {
                    dPTI.DataGrid = d;
                    item.Header = ParserManagement.File.Name;
                    item.Tag = dPTI;
                    UpdateDataTreeViewItem(item);
                }
            }
        }

        #endregion

        #region Private Methods

        private void UpdateDataTreeViewItem(TreeViewItem item) {
            if (item.Tag is DataProjectTreeItem) {
                item.Items.Clear();
                DataProjectTreeItem dPTVI = (DataProjectTreeItem)item.Tag;
                for (int i = 0; i < dPTVI.DataGrid.Columns.Length; i++) {
                    TreeViewItem tmp = new TreeViewItem();
                    String header = dPTVI.DataGrid.Columns[i].Header;
                    tmp.Tag = dPTVI.DataGrid.Columns[i];
                    if (header != "") {
                        tmp.Header = header;
                    } else {
                        tmp.Header = i.ToString();
                    }
                    item.Items.Add(tmp);
                }
            }
        }

        private void InsertToProjectTree(TreeViewItem item, bool isSelected, bool isExpanded) {
            TreeViewItem rootItem = this.SelectedItem;
            if (rootItem != null) {
                InsertToProjectTree(item, rootItem, isSelected, isExpanded);
            } else {
                InsertToProjectTree(item, this.root, isSelected, isExpanded);
            }
        }

        private void InsertToProjectTree(TreeViewItem item, TreeViewItem rootItem, bool isSelected, bool isExpanded) {
            if (rootItem != null) {
                int insertIndex = -1;
                if (rootItem.Tag is DataProjectTreeItem) {
                    TreeViewItem tmp = rootItem;
                    rootItem = (TreeViewItem)rootItem.Parent;
                    insertIndex = rootItem.Items.IndexOf(tmp);      //TODO: Entscheiden, ob danach oder davor
                }
                if (rootItem.Tag is FolderProjectTreeItem) {
                    if (insertIndex < 0) {
                        rootItem.Items.Add(item);
                    } else {
                        rootItem.Items.Insert(insertIndex, item);
                    }
                    item.IsSelected = isSelected;
                    rootItem.IsExpanded = isExpanded;
                }
            }
        }

        private void InitProjectTree() {
            root = new TreeViewItem();
            root.Header = "Root";
            FolderProjectTreeItem fPTI = new FolderProjectTreeItem(root);
            root.Tag = fPTI;
            projectTree.Items.Add(root);
            root.IsSelected = true;
        }

        private void AddDataProjectTreeItem(FileInfo file, TreeViewItem rootItem) {
            DataGrid dataGrid = ParserManagement.GetDataGrid(file);
            if (null != dataGrid) {
                TreeViewItem tvItem = new TreeViewItem();
                tvItem.Header = file.Name;
                DataProjectTreeItem dPTI = new DataProjectTreeItem(dataGrid);
                tvItem.Tag = dPTI;
                UpdateDataTreeViewItem(tvItem);
                if (rootItem != null) {
                    InsertToProjectTree(tvItem, rootItem, true, true);
                } else {
                    InsertToProjectTree(tvItem, true, true);
                }
                RoutedEventArgs args = new RoutedEventArgs(NewFileInsertedEvent, this);
                this.RaiseEvent(args);
            } else { 
                //TODO: Fehlermeldung, dass nicht geparst werden konnte!
            }
        }

        private void RemoveTreeViewItem(TreeViewItem item) {
            if (item.Parent is TreeViewItem) {
                TreeViewItem tvItem = (TreeViewItem)item.Parent;
                tvItem.Items.Remove(item);
            }
        }

        private void DeleteDataProjectTreeItem(DataProjectTreeItem dPTI) {
            MainData.RemoveColumns(dPTI.DataGrid);
            //dPTI.DataGrid = null;
            //dPTI = null;
        }

        #endregion

        #region Event Handlers

        private void projectTree_DragLeave(object sender, DragEventArgs e) {
            if (this.highlightedItem != null) {
                this.highlightedItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                this.highlightedItem = null;
            }
        }

        private void projectTree_DragOver(object sender, DragEventArgs e) {
            TreeViewItem item = e.Source as TreeViewItem;
            if (item != null) {
                item.HeaderTemplate = (DataTemplate)this.FindResource("HighlightTemplate"); ;
                if (this.highlightedItem != null && !this.highlightedItem.Equals(item)) {
                    this.highlightedItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                }
                this.highlightedItem = item;
            }
        }

        private void projectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (editItem != null) editItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
            editItem = null;
        }

        private void projectTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = e.Source as TreeViewItem;
            if (item != null) item.IsSelected = true;
        }

        private void projectTree_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = this.SelectedItem;
            if (item == null) return;
            this.editItem = item;
            DataTemplate editTemplate = (DataTemplate)this.FindResource("EditTemplate");
            item.HeaderTemplate = editTemplate;
            this.oldHeader = item.Header as String;
        }

        private void projectTree_KeyDown(object sender, KeyEventArgs e) {
            if (editItem != null) {
                if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape) {
                    if (e.Key == Key.Escape) editItem.Header = this.oldHeader;
                    editItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                    editItem = null;
                }
            }
        }

        private void projectTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = e.Source as TreeViewItem;
            if (item != null) {
                if (item.Tag is Column || item.Tag is DataProjectTreeItem) {
                    if (this.editItem == null) DragDrop.DoDragDrop(projectTree, item, DragDropEffects.Copy);
                }
            }
        }

        private void projectTree_Drop(object sender, DragEventArgs e) {
            object data = e.Data.GetData("System.IO.FileInfo");
            if (this.highlightedItem != null) {
                this.highlightedItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                this.highlightedItem = null;
            }
            if (data is FileInfo) {
                AddDataProjectTreeItem((FileInfo)data, e.Source as TreeViewItem);
            }
        }

        #endregion

        #region Context Menu Event Handler

        private void ContextMenu_AddNewFolder(object sender, RoutedEventArgs e) {
            TreeViewItem newItem = new TreeViewItem();
            newItem.Header = "new Folder";
            FolderProjectTreeItem fPTI = new FolderProjectTreeItem(newItem);
            newItem.Tag = fPTI;
            InsertToProjectTree(newItem, true, true);
        }

        private void ContextMenu_RemoveItem(object sender, RoutedEventArgs e) {
            TreeViewItem item = this.SelectedItem;
            if (item.Tag is DataProjectTreeItem) {
                DeleteDataProjectTreeItem((DataProjectTreeItem)item.Tag);
            } else if (item.Tag is FolderProjectTreeItem) {
                
            }
            RemoveTreeViewItem(item);
        }

        private void ContextMenu_AddNewDataTable(object sender, RoutedEventArgs e) {
            TreeViewItem item = new TreeViewItem();
            item.Header = "new DataTable";
            item.Tag = new DataProjectTreeItem(new DataGrid());
            InsertToProjectTree(item, true, true);
        }

        #endregion
    }
}
