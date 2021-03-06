﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Pavel2.Framework;
using System.IO;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Data;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für ProjectTree.xaml
    /// </summary>
    public partial class ProjectTree : UserControl {

        #region Fields

        public TreeViewItem root;
        private TreeViewItem editItem;
        private TreeViewItem highlightedItem;
        private String oldHeader;
        private List<TreeViewItem> linkTreeViewItems = new List<TreeViewItem>();
        private bool insertDir = false;
        private bool dragEnabled = false;
        private Point startPoint;

        #endregion

        #region Constructor

        public ProjectTree() {
            InitializeComponent();
            InitProjectTree();
        }

        #endregion

        #region Properties

        public TreeViewItem SelectedItem {
            get { return projectTree.SelectedItem as TreeViewItem; }
        }

        #endregion

        #region Public Methods

        public void ParseAgain(Parser parser, FileInfo file) {
            if (parser == null) return;
            TreeViewItem item = this.SelectedItem;
            if (item != null && item.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                MainData.RemoveColumns(dPTI.DataGrid);
                dPTI.DataGrid = null;
                DataGrid d = ParserManagement.GetDataGrid(parser, file);
                if (d != null) {
                    dPTI.DataGrid = d;
                    dPTI.Header = ParserManagement.File.Name;
                    item.Tag = dPTI;
                    UpdateDataTreeViewItem(item);
                }
            }
        }

        public List<object> GetRelatedItems(DataProjectTreeItem dPTI) {
            List<object> relData = new List<object>();
            foreach (TreeViewItem tvItem in linkTreeViewItems) {
                if (tvItem.Tag is LinkItem) {
                    LinkItem lItem = (LinkItem)tvItem.Tag;
                    bool isIn = false;
                    for (int i = 0; i < lItem.DataItems.Count; i++) {
                        if (lItem.DataItems[i].OriginalData.Equals(dPTI)) {
                            isIn = true;
                        }
                    }
                    if (isIn) {
                        foreach (DataProjectTreeItem d in lItem.DataItems) {
                            if (!relData.Contains(d.OriginalData)) relData.Add(d.OriginalData);
                        }
                        relData.Remove(dPTI);
                        foreach (ImageTreeItem img in lItem.Images) {
                            if (!relData.Contains(img)) relData.Add(img);
                        }
                    }
                }
            }
            return relData;
        }

        public List<object> GetRelatedItems(ImageTreeItem imgItem) {
            List<object> relData = new List<object>();
            foreach (TreeViewItem tvItem in linkTreeViewItems) {
                if (tvItem.Tag is LinkItem) {
                    LinkItem lItem = (LinkItem)tvItem.Tag;
                    bool isIn = false;
                    for (int i = 0; i < lItem.Images.Count; i++) {
                        if (lItem.Images[i].Equals(imgItem)) {
                            isIn = true;
                        }
                    }
                    if (isIn) {
                        foreach (DataProjectTreeItem d in lItem.DataItems) {
                            if (!relData.Contains(d.OriginalData)) relData.Add(d.OriginalData);
                        }
                        foreach (ImageTreeItem img in lItem.Images) {
                            if (!relData.Contains(img)) relData.Add(img);
                        }
                        relData.Remove(imgItem);
                    }
                }
            }
            return relData;
        }

        public void UpdateDataTreeViewItem(TreeViewItem item) {
            if (item.Tag is DataProjectTreeItem) {
                item.Items.Clear();
                DataProjectTreeItem dPTVI = (DataProjectTreeItem)item.Tag;
                dPTVI.DataGrid.ColumnChanged -= dataGrid_ColumnChanged;
                dPTVI.DataGrid.ColumnVisChanged -= dataGrid_ColumnVisChanged;
                dPTVI.DataGrid.ColumnChanged += dataGrid_ColumnChanged;
                dPTVI.DataGrid.ColumnVisChanged += dataGrid_ColumnVisChanged;
                for (int i = 0; i < dPTVI.DataGrid.RealColumns.Length; i++) {
                    TreeViewItem tmp = new TreeViewItem();
                    tmp.FontWeight = FontWeights.Normal;
                    tmp.Tag = dPTVI.DataGrid.RealColumns[i];
                    item.Items.Add(tmp);
                }
            }
        }

        public void UpdateLinkItem(TreeViewItem item) {
            if (item.Tag is LinkItem) {
                item.Items.Clear();
                LinkItem lItem = (LinkItem)item.Tag;
                for (int i = 0; i < lItem.DataItems.Count; i++) {
                    TreeViewItem tmp = new TreeViewItem();
                    tmp.ToolTip = this.GetPath(lItem.DataItems[i]);
                    tmp.FontWeight = FontWeights.Normal;
                    tmp.Opacity = 0.6;
                    tmp.Tag = lItem.DataItems[i];
                    item.Items.Add(tmp);
                    UpdateDataTreeViewItem(tmp);
                    if (lItem.IsCombined) {
                        tmp.BorderThickness = new Thickness(0,0,15,0);
                        tmp.BorderBrush = new SolidColorBrush(ColorManagement.GetColor(i + 2).MediaColor);
                    }
                }
                for (int i = 0; i < lItem.Images.Count; i++) {
                    TreeViewItem tmp = new TreeViewItem();
                    tmp.ToolTip = this.GetPath(lItem.Images[i]);
                    tmp.FontWeight = FontWeights.Normal;
                    tmp.Opacity = 0.6;
                    tmp.Tag = lItem.Images[i];
                    item.Items.Add(tmp);
                }
            }
        }

        #endregion

        #region Private Methods

        public void InsertToProjectTree(TreeViewItem item, bool isSelected, bool isExpanded) {
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
                if (!(rootItem.Tag is FolderProjectTreeItem)) {
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
                    if (!insertDir) projectTree.SelectItem(item);
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
        }

        public void AddDirectory(DirectoryInfo dir, TreeViewItem root) {
            insertDir = true;
            TreeViewItem tmp = new TreeViewItem();
            FolderProjectTreeItem fPTI = new FolderProjectTreeItem(tmp);
            tmp.Tag = fPTI;
            fPTI.Header = dir.Name;
            foreach (FileInfo file in dir.GetFiles()) {
                AddDataProjectTreeItem(file, tmp);
            }
            if (root != null) {
                InsertToProjectTree(tmp, root, false, true);
            } else {
                InsertToProjectTree(tmp, false, true);
            }
            foreach (DirectoryInfo directory in dir.GetDirectories()) {
                AddDirectory(directory, tmp);
            }
            insertDir = false;
        }

        public String GetPath(object item) {
            if (item is DataProjectTreeItem) {
                DataProjectTreeItem d = (DataProjectTreeItem)item;
                List<String> list = new List<string>();
                TreeViewItem tvItem = GetRelatedItem(d.OriginalData.DataGrid, root);
                if (tvItem == null) return d.Header;
                list.Add(d.Header);
                while (tvItem.Parent is TreeViewItem) {
                    TreeViewItem parent = tvItem.Parent as TreeViewItem;
                    if (parent.Tag is FolderProjectTreeItem) {
                        list.Add((parent.Tag as FolderProjectTreeItem).Header);
                    }
                    tvItem = (TreeViewItem)tvItem.Parent;
                }
                String path = "";
                for (int i = 0; i < list.Count; i++) {
                    path = path + "/" + list[list.Count - i - 1];
                }
                return path;
            }
            if (item is ImageTreeItem) {
                ImageTreeItem d = (ImageTreeItem)item;
                List<String> list = new List<string>();
                TreeViewItem tvItem = GetRelatedItem(d, root);
                if (tvItem == null) return d.Header;
                list.Add(d.Header);
                while (tvItem.Parent is TreeViewItem) {
                    TreeViewItem parent = tvItem.Parent as TreeViewItem;
                    if (parent.Tag is FolderProjectTreeItem) {
                        list.Add((parent.Tag as FolderProjectTreeItem).Header);
                    }
                    tvItem = (TreeViewItem)tvItem.Parent;
                }
                String path = "";
                for (int i = 0; i < list.Count; i++) {
                    path = path + "/" + list[list.Count - i - 1];
                }
                return path;
            }
            return null;
        }

        public void AddDataProjectTreeItem(FileInfo file, TreeViewItem rootItem) {
            if (ImageParser.IsImage(file)) {
                ImageTreeItem imgItem = new ImageTreeItem(ImageParser.GetImage(file));
                TreeViewItem tvItem = new TreeViewItem();
                imgItem.Header = file.Name;
                tvItem.Tag = imgItem;
                if (rootItem != null) {
                    InsertToProjectTree(tvItem, rootItem, true, true);
                } else {
                    InsertToProjectTree(tvItem, true, true);
                }
            } else {
                DataGrid dataGrid = ParserManagement.GetDataGrid(file);
                if (null != dataGrid) {
                    TreeViewItem tvItem = new TreeViewItem();
                    DataProjectTreeItem dPTI = new DataProjectTreeItem(dataGrid);
                    dPTI.Header = file.Name;
                    dPTI.Filename = file.FullName;
                    dPTI.Parser = ParserManagement.CurrentParser;
                    tvItem.Tag = dPTI;
                    UpdateDataTreeViewItem(tvItem);
                    if (rootItem != null) {
                        InsertToProjectTree(tvItem, rootItem, true, true);
                    } else {
                        InsertToProjectTree(tvItem, true, true);
                    }
                } else {
                    //TODO: Fehlermeldung, dass nicht geparst werden konnte!
                }
            }
        }

        public void dataGrid_ColumnVisChanged(object sender, EventArgs e) {
            Column col = (Column)sender;
            UpdateRelatedDataSets(col, root);
        }

        public void dataGrid_ColumnChanged(object sender, EventArgs e) {
            DataGrid d = (DataGrid)sender;
            TreeViewItem tvItem = GetRelatedItem(d, root);
            if (tvItem == null) return;
            UpdateDataTreeViewItem(tvItem);
        }

        public void Select(object item) {
            TreeViewItem tvItem = null;
            if (item is DataProjectTreeItem) {
                tvItem = GetRelatedItem((item as DataProjectTreeItem).DataGrid, root);
            } else if (true) {
                tvItem = GetRelatedItem(item as ImageTreeItem, root);
            }
            if (tvItem == null) return;
            projectTree.SelectItem(tvItem);
            tvItem.IsSelected = true;
        }

        public TreeViewItem GetRelatedItem(DataGrid d, TreeViewItem root) {
            foreach (TreeViewItem item in root.Items) {
                if (item.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                    if (dPTI.DataGrid.Equals(d)) return item;
                } else if (item.Tag is FolderProjectTreeItem) {
                    TreeViewItem tVI = GetRelatedItem(d, item);
                    if (tVI != null) return tVI;
                }
            }
            return null;
        }

        public TreeViewItem GetRelatedItem(ImageTreeItem imgItem, TreeViewItem root) {
            foreach (TreeViewItem item in root.Items) {
                if (item.Tag is ImageTreeItem) {
                    ImageTreeItem img = (ImageTreeItem)item.Tag;
                    if (img.Equals(imgItem)) return item;
                } else if (item.Tag is FolderProjectTreeItem) {
                    TreeViewItem tVI = GetRelatedItem(imgItem, item);
                    if (tVI != null) return tVI;
                }
            }
            return null;
        }

        public void UpdateRelatedDataSets(Column col, TreeViewItem root) {
            foreach (TreeViewItem item in root.Items) {
                if (item.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                    for (int i = 0; i < dPTI.DataGrid.RealColumns.Length; i++) {
                        if (dPTI.DataGrid.RealColumns[i].Equals(col)) UpdateDataTreeViewItem(item);
                    }
                } else if (item.Tag is FolderProjectTreeItem) {
                    UpdateRelatedDataSets(col, item);
                }
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
                    if (tvItem.Tag is DataProjectTreeItem || tvItem.Tag is ImageTreeItem) {
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
                    if (tvItem.Tag is ImageTreeItem) {
                        lItem.AddImage(tvItem.Tag as ImageTreeItem);
                    }
                }
                UpdateLinkItem(target);
            }
        }

        private void CopyColumn(TreeViewItem tvItem, Column col) {
            if (tvItem.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = tvItem.Tag as DataProjectTreeItem;
                dPTI.DataGrid.AddColumn(col);
                UpdateDataTreeViewItem(tvItem);
            }
        }

        private void MoveTreeViewItem(TreeViewItem tvItem, TreeViewItem target) {
            if (target.Tag is FolderProjectTreeItem) {
                if (tvItem.Tag is DataProjectTreeItem || tvItem.Tag is ImageTreeItem) {
                    RemoveTreeViewItem(tvItem);
                    target.Items.Add(tvItem);
                }
            }
            if (target.Tag is LinkItem) {
                LinkItem lItem = target.Tag as LinkItem;
                if (tvItem.Tag is DataProjectTreeItem) {
                    lItem.AddDataItem((tvItem.Tag as DataProjectTreeItem).Clone());
                }
                if (tvItem.Tag is ImageTreeItem) {
                    lItem.AddImage(tvItem.Tag as ImageTreeItem);
                }
                UpdateLinkItem(target);
            }
            if (target.Tag is DataProjectTreeItem && tvItem.Tag is DataProjectTreeItem) {
                if (target.Parent is TreeViewItem) {
                    TreeViewItem tvLinkItem = target.Parent as TreeViewItem;
                    if (tvLinkItem.Tag is LinkItem) {
                        LinkItem lItem = tvLinkItem.Tag as LinkItem;
                        DataProjectTreeItem targetItem = target.Tag as DataProjectTreeItem;
                        DataProjectTreeItem sourceItem = tvItem.Tag as DataProjectTreeItem;
                        int index = lItem.DataItems.IndexOf(targetItem);
                        if (index != -1) {
                            lItem.RemoveDataItem(sourceItem);
                            lItem.AddDataItem(sourceItem, index);
                        }
                        UpdateLinkItem(tvLinkItem);
                    }
                }
            }
        }

        private void DeleteDataProjectTreeItem(DataProjectTreeItem dPTI) {
            MainData.RemoveColumns(dPTI.DataGrid);
            dPTI.DataGrid = null;
            dPTI = null;
        }

        private void DeleteDataProjectTreeItem(DataProjectTreeItem dPTI, LinkItem lItem) {
            lItem.RemoveDataItem(dPTI);
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

        private void ChangeItemsVisibility(TreeViewItem rootItem, Type typeToChange, bool visible) {
            if (rootItem == null) return;
            foreach (TreeViewItem tvItem in rootItem.Items) {
                if (tvItem.Tag is FolderProjectTreeItem) ChangeItemsVisibility(tvItem, typeToChange, visible);
                if (tvItem.Tag.GetType().Equals(typeToChange)) {
                    if (visible) tvItem.Visibility = Visibility.Visible;
                    else tvItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void viewPackageButton_Checked(object sender, RoutedEventArgs e) {
            ChangeItemsVisibility(this.root, typeof(LinkItem), true);
        }

        private void viewPackageButton_Unchecked(object sender, RoutedEventArgs e) {
            ChangeItemsVisibility(this.root, typeof(LinkItem), false);
        }

        private void viewAllButton_Checked(object sender, RoutedEventArgs e) {
            ChangeItemsVisibility(this.root, typeof(DataProjectTreeItem), true);
        }

        private void viewAllButton_Unchecked(object sender, RoutedEventArgs e) {
            ChangeItemsVisibility(this.root, typeof(DataProjectTreeItem), false);
        }

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
            if (item != null) projectTree.SelectItem(item);
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

        private void projectTree_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (this.editItem == null && dragEnabled && (Point.Subtract(startPoint, e.GetPosition(this)).Length > 10)) {
                    DragDropHelper.DoDragDrop(projectTree, projectTree.SelectedItems, DragDropEffects.Copy, this);
                    dragEnabled = false;
                }
            }
        }

        private void projectTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = e.Source as TreeViewItem;
            dragEnabled = false;
            if (item != null) {
                if (item.Tag is Column || item.Tag is DataProjectTreeItem || item.Tag is ImageTreeItem) {
                    dragEnabled = true;
                    startPoint = e.GetPosition(this);
                }
            }
        }

        private void projectTree_Drop(object sender, DragEventArgs e) {
            if (this.highlightedItem != null) {
                this.highlightedItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                this.highlightedItem = null;
            }
            object data = e.Data.GetData(typeof(List<TreeViewItem>));
            if (data is List<TreeViewItem>) {
                List<TreeViewItem> items = (List<TreeViewItem>)data;
                foreach (TreeViewItem tvItem in items) {
                    if (tvItem.Tag is FileInfo) AddDataProjectTreeItem(tvItem.Tag as FileInfo, e.Source as TreeViewItem);
                    if (tvItem.Tag is ProjectTreeItem || tvItem.Tag is ImageTreeItem) MoveTreeViewItem(tvItem, e.Source as TreeViewItem);
                    if (tvItem.Tag is DirectoryInfo) AddDirectory((DirectoryInfo)tvItem.Tag, e.Source as TreeViewItem);
                    if (tvItem.Tag is Column) CopyColumn(e.Source as TreeViewItem, tvItem.Tag as Column);
                }
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
            this.editItem = newItem;
            newItem.HeaderTemplate = (DataTemplate)this.FindResource("EditTemplate");
        }

        private void ContextMenu_RemoveItem(object sender, RoutedEventArgs e) {
            TreeViewItem[] tmp = projectTree.SelectedItems.ToArray();
            foreach (TreeViewItem item in tmp) {
                TreeViewItem parent = null;
                if (item.Parent is TreeViewItem) {
                    parent = (TreeViewItem)item.Parent;
                }
                if (item.Tag is DataProjectTreeItem) {
                    if (parent != null && parent.Tag is LinkItem) {
                        DeleteDataProjectTreeItem((DataProjectTreeItem)item.Tag, parent.Tag as LinkItem);
                        RemoveTreeViewItem(item);
                    } else {
                        DeleteDataProjectTreeItem((DataProjectTreeItem)item.Tag);
                        RemoveTreeViewItem(item);
                    }
                } else if (item.Tag is FolderProjectTreeItem) {
                    DeleteFolderProjectTreeItem(item);
                    (item.Tag as FolderProjectTreeItem).DataGrid.ColumnChanged -= dataGrid_ColumnChanged;
                    (item.Tag as FolderProjectTreeItem).DataGrid.ColumnVisChanged -= dataGrid_ColumnVisChanged;
                    RemoveTreeViewItem(item);
                } else if (item.Tag is LinkItem) {
                    DeleteLinkTreeItem(item.Tag as LinkItem);
                    RemoveTreeViewItem(item);
                }
                if (parent != null) projectTree.SelectItem(parent);
            }
        }

        private void ContextMenu_AddNewDataTable(object sender, RoutedEventArgs e) {
            TreeViewItem item = new TreeViewItem();
            DataProjectTreeItem dPTI = new DataProjectTreeItem(new DataGrid());
            dPTI.Header = "new Data Set";
            item.Tag = dPTI;
            InsertToProjectTree(item, true, true);
            this.editItem = item;
            item.HeaderTemplate = (DataTemplate)this.FindResource("EditTemplate");
        }

        private void ContextMenu_CreateNewGroup(object sender, RoutedEventArgs e) {
            TreeViewItem newItem = new TreeViewItem();
            LinkItem lItem = new LinkItem();
            foreach (TreeViewItem tvItem in projectTree.SelectedItems) {
                if (tvItem.Tag is DataProjectTreeItem) {
                    DataProjectTreeItem dPTI = tvItem.Tag as DataProjectTreeItem;
                    lItem.AddDataItem(dPTI.Clone());
                }
                if (tvItem.Tag is ImageTreeItem) {
                    ImageTreeItem img = (ImageTreeItem)tvItem.Tag;
                    lItem.AddImage(img);
                }
            }
            lItem.Header = "new Group";
            newItem.Tag = lItem;
            InsertToProjectTree(newItem, true, true);
            UpdateLinkItem(newItem);
            linkTreeViewItems.Add(newItem);
            projectTree.SelectItem(newItem);
            this.editItem = newItem;
            newItem.HeaderTemplate = (DataTemplate)this.FindResource("EditTemplate");
        }

        #endregion

        private void img_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (SelectedItem.Tag is Column) {
                Column col = (Column)SelectedItem.Tag;
                if (SelectedItem.Parent is TreeViewItem) {
                    TreeViewItem dataTreeItem = (TreeViewItem)SelectedItem.Parent;
                    if (dataTreeItem.Tag is DataProjectTreeItem) {
                        DataProjectTreeItem dPTI = (DataProjectTreeItem)dataTreeItem.Tag;
                        dPTI.DataGrid.ColIsVisible(col, !col.Visible);
                    }
                    projectTree.SelectItem(dataTreeItem);
                    MainData.MainWindow.UpdateVisualization();
                }
            }
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e) {
            if (sender is TextBox) {
                TextBox b = sender as TextBox;
                b.Focus();
                b.SelectAll();
            }
        }        
    }
}
