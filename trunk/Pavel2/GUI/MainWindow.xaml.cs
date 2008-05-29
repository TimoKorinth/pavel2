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
        private TreeViewItem editItem;
        private Visualization visualization;

        public MainWindow() {
            InitializeComponent();
            propertyGridLayout.Visibility = Visibility.Collapsed;

            root = new TreeViewItem();
            root.Header = "Root";
            FolderProjectTreeItem fPTI = new FolderProjectTreeItem(root);
            root.Tag = fPTI;
            projectTree.Items.Add(root);
            root.IsSelected = true;

            Visualize(new TableView());
        }

        private void Visualize(UIElement item) {
            visualizationGrid.Children.Clear();
            if (item is Visualization) {
                visualization = (Visualization)item;
                visualizationGrid.Children.Add(item);
                visualization.Render();
            } else {
                visualization = null;
            }
        }

        private DataGrid currentDataGrid;

        public DataGrid CurrentDataGrid {
            get {
                return currentDataGrid;
            }
            set {
                currentDataGrid = value;
                if (visualization != null) visualization.Render();
            }
        }

        private void AddDataProjectTreeItem(FileInfo file) {
            AddDataProjectTreeItem(file, null);
        }

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
                        tmp.Header = i;
                    }
                    item.Items.Add(tmp);
                }
            }
        }

        private void UpdateCompTreeViewItem(TreeViewItem item) {
            if (item.Tag is ComparableProjectTreeItem) {
                for (int i = 0; i < item.Items.Count; i++) {
                    if (item.Items[i] is TreeViewItem) {
                        UpdateDataTreeViewItem((TreeViewItem)item.Items[i]);
                    }
                }
            }
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

                propertyGridLayout.Visibility = Visibility.Visible;
                propertyGrid.SelectedObject = ParserManagement.CurrentParser;
                propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(propertyGrid_PropertyValueChanged);
                parserComboBox.ItemsSource = ParserManagement.ParserList;
                parserComboBox.DisplayMemberPath = "Name";
                parserComboBox.SelectedItem = ParserManagement.CurrentParser;
            }
        }

        /// <summary>
        /// Insert a TreeViewItem at the position of the currently selected Item.  If
        /// no item is selected, the new Item is inserted into the root element.
        /// </summary>
        /// <param name="item">TreeViewItem to insert.</param>
        /// <param name="isSelected"></param>
        /// <param name="isExpanded"></param>
        private void InsertToProjectTree(TreeViewItem item, bool isSelected, bool isExpanded) {
            TreeViewItem rootItem = (TreeViewItem)projectTree.SelectedItem;
            if (rootItem != null) {
                InsertToProjectTree(item, rootItem, isSelected, isExpanded);
            } else {
                InsertToProjectTree(item, this.root, isSelected, isExpanded);
            }
        }

        /// <summary>
        /// Insert a TreeViewItem.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="rootItem"></param>
        /// <param name="isSelected"></param>
        /// <param name="isExpanded"></param>
        private void InsertToProjectTree(TreeViewItem item, TreeViewItem rootItem, bool isSelected, bool isExpanded) {
            if (rootItem != null) {
                int insertIndex = -1;
                if (rootItem.Tag is DataProjectTreeItem) {
                    TreeViewItem tmp = rootItem;
                    rootItem = (TreeViewItem)rootItem.Parent;
                    insertIndex = rootItem.Items.IndexOf(tmp);
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
                this.CurrentDataGrid = d;
                UpdateDataTreeViewItem(item);
                if (d != null) {
                    item.Header = ParserManagement.File.Name;
                    item.Tag = dPTI;
                }
            }
        }

        private void projectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            propertyGridLayout.Visibility = Visibility.Collapsed;
            if (editItem != null) editItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
            editItem = null;
            TreeViewItem tvItem = (TreeViewItem)projectTree.SelectedItem;
            if (tvItem != null) {
                if (tvItem.Tag is ProjectTreeItem) {
                    ProjectTreeItem ptItem = (ProjectTreeItem)tvItem.Tag;
                    this.CurrentDataGrid = ptItem.DataGrid;
                } else {
                    this.CurrentDataGrid = null;
                }
            } else {
                this.CurrentDataGrid = null;
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e) {
            propertyGridLayout.Visibility = Visibility.Collapsed;
        }

        private void projectTree_Drop(object sender, DragEventArgs e) {
            this.lastModifiedItem.Background = null;
            object data = e.Data.GetData("System.IO.FileInfo");
            if (data is FileInfo) {
                FileInfo file = (FileInfo)data;
                TreeViewItem item = TreeViewHelper.GetTreeViewItem(e.GetPosition, this.root);
                AddDataProjectTreeItem(file, item);
            }
        }

        private void projectTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = TreeViewHelper.GetTreeViewItem(e.GetPosition, this.root);
            if (item != null) item.IsSelected = true;
        }

        private void AddNewFolder(object sender, RoutedEventArgs e) {
            TreeViewItem newItem = new TreeViewItem();
            newItem.Header = "Folder";
            FolderProjectTreeItem fPTI = new FolderProjectTreeItem(newItem);
            newItem.Tag = fPTI;
            InsertToProjectTree(newItem, true, true);
        }

        private void RemoveItem(object sender, RoutedEventArgs e) {
            TreeViewItem item = (TreeViewItem)projectTree.SelectedItem;
            if (item.Tag is DataProjectTreeItem) {
                DeleteDataProjectTreeItem((DataProjectTreeItem)item.Tag);
            } else if (item.Tag is ComparableProjectTreeItem) {
                DeleteCompProjectTreeItem((ComparableProjectTreeItem)item.Tag);
            }
            RemoveTreeViewItem(item);
        }

        private void DeleteCompProjectTreeItem(ComparableProjectTreeItem comp) {
            comp = null;
        }

        private void RemoveTreeViewItem(TreeViewItem item) {
            if (item.Parent is TreeViewItem) {
                TreeViewItem tvItem = (TreeViewItem)item.Parent;
                tvItem.Items.Remove(item);
            }
        }

        private void DeleteDataProjectTreeItem(DataProjectTreeItem dPTI) {
            MainData.RemoveColumns(dPTI.DataGrid);
            dPTI.DataGrid = null;
            dPTI = null;
        }

        private void projectTree_DragOver(object sender, DragEventArgs e) {
            TreeViewItem item = TreeViewHelper.GetTreeViewItem(e.GetPosition, this.root);
            if (this.lastModifiedItem != null) this.lastModifiedItem.Background = null;
            item.Background = Brushes.Turquoise;
            this.lastModifiedItem = item;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e) {
            TreeViewItem item = new TreeViewItem();
            item.Header = "DataTable";
            item.Tag = new DataProjectTreeItem(new DataGrid());
            InsertToProjectTree(item, true, true);
        }

        private void virtualizationGrid_Drop(object sender, DragEventArgs e) {
            object data = e.Data.GetData("System.Windows.Controls.TreeViewItem");
            TreeViewItem selItem = (TreeViewItem)projectTree.SelectedItem;
            if (this.lastModifiedItem != null) this.lastModifiedItem.Background = null;
            if (data is TreeViewItem) {
                TreeViewItem tvItem = (TreeViewItem)data;
                if (tvItem.Tag is Column) {
                    if (selItem.Tag is DataProjectTreeItem) {
                        DataProjectTreeItem dPTITmp = (DataProjectTreeItem)selItem.Tag;
                        if (dPTITmp.DataGrid == null) {
                            dPTITmp.DataGrid = new DataGrid();
                        }
                        dPTITmp.DataGrid.AddColumn((Column)tvItem.Tag);
                        UpdateDataTreeViewItem(selItem);
                        this.CurrentDataGrid = dPTITmp.DataGrid;
                    }
                } else if (tvItem.Tag is DataProjectTreeItem) {
                    if (selItem.Tag is ComparableProjectTreeItem) {
                        ComparableProjectTreeItem comp = (ComparableProjectTreeItem)selItem.Tag;
                        DataProjectTreeItem dataItem = (DataProjectTreeItem)tvItem.Tag;
                        dataItem.DataGrid.Comparables.Add(comp);
                        comp.AddDataGrid(dataItem.DataGrid);
                        TreeViewItem tmp = new TreeViewItem();
                        tmp.Tag = dataItem;
                        tmp.Header = tvItem.Header;
                        selItem.Items.Add(tmp);
                        UpdateCompTreeViewItem(selItem);
                        this.CurrentDataGrid = comp.DataGrid;
                    }
                }
            }
        }

        private void tableButton_Click(object sender, RoutedEventArgs e) {
            Visualize(new TableView());
        }

        private void scatterPlotButton_Click(object sender, RoutedEventArgs e) {
            BitmapImage source = new BitmapImage(new Uri("pack://application:,,,/GUI/Images/ScatterPlot.png"));
            Image img = new Image();
            img.Source = source;
            Visualize(img);
        }

        private void parallelPlotButton_Click(object sender, RoutedEventArgs e) {
            Visualize(new ParallelPlot());
        }

        private void scatterMatrixButton_Click(object sender, RoutedEventArgs e) {
            BitmapImage source = new BitmapImage(new Uri("pack://application:,,,/GUI/Images/ScatterMatrix.png"));
            Image img = new Image();
            img.Source = source;
            Visualize(img);
        }

        private void projectTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = TreeViewHelper.GetTreeViewItem(e.GetPosition, this.root);
            if (item.Tag is Column || item.Tag is DataProjectTreeItem) {
                if (this.editItem == null) DragDrop.DoDragDrop(projectTree, item, DragDropEffects.Copy);
            }
        }

        private void CompMenuItem_Click(object sender, RoutedEventArgs e) {
            TreeViewItem item = new TreeViewItem();
            item.Header = "Comp. Item";
            ComparableProjectTreeItem comp = new ComparableProjectTreeItem();
            item.Tag = comp;
            InsertToProjectTree(item, true, true);
        }

        private void projectTree_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = TreeViewHelper.GetTreeViewItem(e.GetPosition, this.root);
            this.editItem = item;
            DataTemplate editTemplate = (DataTemplate)this.FindResource("EditTemplate");
            item.HeaderTemplate = editTemplate;
        }

        private void projectTree_KeyDown(object sender, KeyEventArgs e) {
            if (editItem != null) {
                if (e.Key == Key.Enter || e.Key == Key.Return) {
                    editItem.HeaderTemplate = (DataTemplate)this.FindResource("DefaultTemplate");
                    editItem = null;
                }
            }
        }

        private void visualizationGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            visualization.RenderAfterResize();
        }
    }
}
