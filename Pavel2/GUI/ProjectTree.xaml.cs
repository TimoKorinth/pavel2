﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Pavel2.Framework;
using System.IO;
using System.Windows.Documents;
using System.Collections.Generic;

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
        private List<TreeViewItem> linkTreeViewItems = new List<TreeViewItem>();

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
                    dPTI.Header = ParserManagement.File.Name;
                    item.Tag = dPTI;
                    UpdateDataTreeViewItem(item);
                }
            }
        }

        public List<DataProjectTreeItem> GetRelatedItems(DataProjectTreeItem dPTI) {
            List<DataProjectTreeItem> relData = new List<DataProjectTreeItem>();
            foreach (TreeViewItem tvItem in linkTreeViewItems) {
                if (tvItem.Tag is LinkItem) {
                    LinkItem lItem = (LinkItem)tvItem.Tag;
                    if (lItem.DataItems.Contains(dPTI)) {
                        foreach (DataProjectTreeItem d in lItem.DataItems) {
                            if (!relData.Contains(d)) relData.Add(d);
                        }
                        relData.Remove(dPTI);
                    }
                }
            }
            return relData;
        }

        #endregion

        #region Private Methods

        private void UpdateDataTreeViewItem(TreeViewItem item) {
            if (item.Tag is DataProjectTreeItem) {
                item.Items.Clear();
                DataProjectTreeItem dPTVI = (DataProjectTreeItem)item.Tag;
                for (int i = 0; i < dPTVI.DataGrid.Columns.Length; i++) {
                    TreeViewItem tmp = new TreeViewItem();
                    tmp.Tag = dPTVI.DataGrid.Columns[i];
                    item.Items.Add(tmp);
                }
            }
        }

        private void UpdateLinkItem(TreeViewItem item) {
            if (item.Tag is LinkItem) {
                item.Items.Clear();
                LinkItem lItem = (LinkItem)item.Tag;
                for (int i = 0; i < lItem.DataItems.Count; i++) {
                    TreeViewItem tmp = new TreeViewItem();
                    tmp.Tag = lItem.DataItems[i];
                    item.Items.Add(tmp);
                    UpdateDataTreeViewItem(tmp);
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
                if (rootItem.Tag is DataProjectTreeItem || rootItem.Tag is LinkItem) {
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
            FolderProjectTreeItem fPTI = new FolderProjectTreeItem(root);
            fPTI.Header = "Root";
            root.Tag = fPTI;
            projectTree.Items.Add(root);
            root.IsSelected = true;
        }

        private void AddDataProjectTreeItem(FileInfo file, TreeViewItem rootItem) {
            DataGrid dataGrid = ParserManagement.GetDataGrid(file);
            if (null != dataGrid) {
                TreeViewItem tvItem = new TreeViewItem();
                DataProjectTreeItem dPTI = new DataProjectTreeItem(dataGrid);
                dPTI.Header = file.Name;
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

        private void MoveTreeViewItems(List<TreeViewItem> items, TreeViewItem target) {
            if (target.Tag is FolderProjectTreeItem) {
                foreach (TreeViewItem tvItem in items) {
                    if (tvItem.Tag is DataProjectTreeItem) {
                        RemoveTreeViewItem(tvItem);
                        target.Items.Add(tvItem);
                    }
                }
            }
            if (target.Tag is LinkItem) {
                LinkItem lItem = target.Tag as LinkItem;
                foreach (TreeViewItem tvItem in items) {
                    if (tvItem.Tag is DataProjectTreeItem) {
                        lItem.AddDataItem(tvItem.Tag as DataProjectTreeItem);
                    }
                }
                UpdateLinkItem(target);
            }
        }

        private void DeleteDataProjectTreeItem(DataProjectTreeItem dPTI) {
            MainData.RemoveColumns(dPTI.DataGrid);
            dPTI.DataGrid = null;
            dPTI = null;
        }

        //TODO: Dispose für alle
        private void DeleteLinkTreeItem(LinkItem lItem) {
            List<TreeViewItem> tmp = new List<TreeViewItem>();
            foreach (TreeViewItem tvItem in linkTreeViewItems) {
                if ((tvItem.Tag as LinkItem).Equals(lItem)) tmp.Add(tvItem);
            }
            foreach (TreeViewItem item in tmp) {
                linkTreeViewItems.Remove(item);
            }
            lItem.DataItems = null;
            lItem = null;
        }

        private void DeleteFolderProjectTreeItem(TreeViewItem item) { 
            if (!(item.Tag is FolderProjectTreeItem)) return;
            foreach (TreeViewItem tvItem in item.Items) {
                if (tvItem.Tag is FolderProjectTreeItem) {
                    DeleteFolderProjectTreeItem(tvItem);
                } else if (tvItem.Tag is DataProjectTreeItem) {
                    DeleteDataProjectTreeItem(tvItem.Tag as DataProjectTreeItem);
                }
            }
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
            if (item.Tag is ProjectTreeItem) {
                this.oldHeader = ((ProjectTreeItem)item.Tag).Header;
            } else if (item.Tag is Column) {
                this.oldHeader = ((Column)item.Tag).Header;
            }
        }

        private void projectTree_KeyDown(object sender, KeyEventArgs e) {
            if (editItem != null) {
                if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape) {
                    if (e.Key == Key.Escape) {
                        if (editItem.Tag is ProjectTreeItem) {
                            ((ProjectTreeItem)editItem.Tag).Header = this.oldHeader;
                        } else if (editItem.Tag is Column) {
                            ((Column)editItem.Tag).Header = this.oldHeader;
                        }
                    }
                    editItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                    editItem = null;
                }
            }
        }

        private void projectTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = e.Source as TreeViewItem;
            if (item != null) {
                if (item.Tag is Column || item.Tag is DataProjectTreeItem) {
                    if (this.editItem == null) {
                        if (!projectTree.SelectedItems.Contains(item)) item.IsSelected = true;
                        DragDropHelper.DoDragDrop(projectTree, projectTree.SelectedItems, DragDropEffects.Copy, MainData.MainWindow.linkListPanel, this);
                    }
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
            data = e.Data.GetData(typeof(List<TreeViewItem>));
            if (data is List<TreeViewItem>) {
                MoveTreeViewItems((List<TreeViewItem>)data, e.Source as TreeViewItem);
            }
        }

        #endregion

        #region Context Menu Event Handler

        private void ContextMenu_AddNewFolder(object sender, RoutedEventArgs e) {
            TreeViewItem newItem = new TreeViewItem();
            FolderProjectTreeItem fPTI = new FolderProjectTreeItem(newItem);
            fPTI.Header = "new Folder";
            newItem.Tag = fPTI;
            InsertToProjectTree(newItem, true, true);
        }

        private void ContextMenu_RemoveItem(object sender, RoutedEventArgs e) {
            TreeViewItem item = this.SelectedItem;
            if (item.Tag is DataProjectTreeItem) {
                DeleteDataProjectTreeItem((DataProjectTreeItem)item.Tag);
            } else if (item.Tag is FolderProjectTreeItem) {
                DeleteFolderProjectTreeItem(item);
            } else if (item.Tag is LinkItem) {
                DeleteLinkTreeItem(item.Tag as LinkItem);
            }
            RemoveTreeViewItem(item);
        }

        private void ContextMenu_AddNewDataTable(object sender, RoutedEventArgs e) {
            TreeViewItem item = new TreeViewItem();
            DataProjectTreeItem dPTI = new DataProjectTreeItem(new DataGrid());
            dPTI.Header = "new Data Set";
            item.Tag = dPTI;
            InsertToProjectTree(item, true, true);
        }

        private void ContextMenu_CreateNewGroup(object sender, RoutedEventArgs e) {
            TreeViewItem newItem = new TreeViewItem();
            LinkItem lItem = new LinkItem();
            foreach (TreeViewItem tvItem in projectTree.SelectedItems) {
                if (tvItem.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = tvItem.Tag as DataProjectTreeItem;
                    lItem.AddDataItem(dPTI);
                    if (lItem.Header == null) lItem.Header = dPTI.Header;
                }
            }
            newItem.Tag = lItem;
            InsertToProjectTree(newItem, true, true);
            UpdateLinkItem(newItem);
            linkTreeViewItems.Add(newItem);
        }

        #endregion
    }
}
