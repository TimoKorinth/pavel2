using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pavel2.Framework;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private Visualization visualization;

        public MainWindow() {
            InitializeComponent();
            propertyGridLayout.Visibility = Visibility.Collapsed;

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
                visualizationGrid.Children.Add(item);
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

        private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e) {
            ParseAgain(parserComboBox.SelectedItem as Parser);
        }

        private void ParseAgain(Parser parser) {
            TreeViewItem item = projectTree.SelectedItem;
            if (item != null && item.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = (DataProjectTreeItem)item.Tag;
                DataGrid dataGrid = dPTI.DataGrid;
                MainData.RemoveColumns(dataGrid);
                DataGrid d = ParserManagement.GetDataGrid(parser);
                dPTI.DataGrid = d;
                this.CurrentDataGrid = d;
                if (d != null) {
                    item.Header = ParserManagement.File.Name;
                    item.Tag = dPTI;
                }
            }
        }

        private void parserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!(parserComboBox.SelectedItem as Parser).Equals(ParserManagement.CurrentParser)) {
                ParseAgain(parserComboBox.SelectedItem as Parser);
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e) {
            propertyGridLayout.Visibility = Visibility.Collapsed;
        }

        private void virtualizationGrid_Drop(object sender, DragEventArgs e) {
            object data = e.Data.GetData("System.Windows.Controls.TreeViewItem");
            TreeViewItem selItem = projectTree.SelectedItem;
            if (data is TreeViewItem && selItem != null) {
                TreeViewItem tvItem = (TreeViewItem)data;
                if (tvItem.Tag is Column) {
                    if (selItem.Tag is DataProjectTreeItem) {
                        DataProjectTreeItem dPTITmp = (DataProjectTreeItem)selItem.Tag;
                        if (dPTITmp.DataGrid == null) {
                            dPTITmp.DataGrid = new DataGrid();
                        }
                        dPTITmp.DataGrid.AddColumn((Column)tvItem.Tag);
                        projectTree.UpdateDataTreeViewItem(selItem);
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
                        projectTree.UpdateDataTreeViewItem(selItem);
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

        private void visualizationGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            visualization.RenderAfterResize();
        }

        private void projectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            propertyGridLayout.Visibility = Visibility.Collapsed;
            TreeViewItem tvItem = projectTree.SelectedItem;
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

        private void projectTree_NewFileInserted(object sender, RoutedEventArgs e) {
            propertyGridLayout.Visibility = Visibility.Visible;
            propertyGrid.SelectedObject = ParserManagement.CurrentParser;
            propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(propertyGrid_PropertyValueChanged);
            parserComboBox.ItemsSource = ParserManagement.ParserList;
            parserComboBox.DisplayMemberPath = "Name";
            parserComboBox.SelectedItem = ParserManagement.CurrentParser;
        }
    }
}
