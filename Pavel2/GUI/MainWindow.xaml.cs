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
using System.Windows.Shapes;
using Pavel2.Framework;

namespace Pavel2.GUI
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window {

        #region Expander Fields

        private double projectTreeExpanderMinWidth = double.NaN;
        private GridLength projectTreeExpanderWidth;
        private double linksExpanderMinHeight = double.NaN;
        private GridLength linksExpanderHeight;
        private double previewExpanderMinWidth = double.NaN;
        private GridLength previewExpanderWidth;
        private double explorerExpanderMinWidth = double.NaN;
        private GridLength explorerExpanderWidth;

        #endregion

        public MainWindow() {
			this.InitializeComponent();

            EmptyPreviewPanel();
            EmptyOptionsPanel();
		}

        #region Expander Event Handler

        private void projectTreeExpander_Collapsed(object sender, RoutedEventArgs e) {
            projectTreeExpanderMinWidth = windowGrid.ColumnDefinitions[0].MinWidth;
            projectTreeExpanderWidth = windowGrid.ColumnDefinitions[0].Width;
            windowGrid.ColumnDefinitions[0].MinWidth = 0;
            windowGrid.ColumnDefinitions[0].Width = GridLength.Auto;
            projectTreeSplitter.Visibility = Visibility.Collapsed;
        }

        private void projectTreeExpander_Expanded(object sender, RoutedEventArgs e) {
            if (double.IsNaN(projectTreeExpanderMinWidth)) return;
            windowGrid.ColumnDefinitions[0].MinWidth = projectTreeExpanderMinWidth;
            windowGrid.ColumnDefinitions[0].Width = projectTreeExpanderWidth;
            projectTreeSplitter.Visibility = Visibility.Visible;
        }

        private void linksExpander_Collapsed(object sender, RoutedEventArgs e) {
            linksExpanderMinHeight = projectTreeGrid.RowDefinitions[2].MinHeight;
            linksExpanderHeight = projectTreeGrid.RowDefinitions[2].Height;
            projectTreeGrid.RowDefinitions[2].MinHeight = 0;
            projectTreeGrid.RowDefinitions[2].Height = GridLength.Auto;
            linksSplitter.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void linksExpander_Expanded(object sender, RoutedEventArgs e) {
            if (double.IsNaN(linksExpanderMinHeight)) return;
            projectTreeGrid.RowDefinitions[2].MinHeight = linksExpanderMinHeight;
            projectTreeGrid.RowDefinitions[2].Height = linksExpanderHeight;
            linksSplitter.Visibility = Visibility.Visible;
            e.Handled = true;
        }

        private void previewExpander_Expanded(object sender, RoutedEventArgs e) {
            if (double.IsNaN(previewExpanderMinWidth)) return;
            windowGrid.ColumnDefinitions[6].MinWidth = previewExpanderMinWidth;
            windowGrid.ColumnDefinitions[6].Width = previewExpanderWidth;
            previewSplitter.Visibility = Visibility.Visible;
        }

        private void previewExpander_Collapsed(object sender, RoutedEventArgs e) {
            previewExpanderMinWidth = windowGrid.ColumnDefinitions[6].MinWidth;
            previewExpanderWidth = windowGrid.ColumnDefinitions[6].Width;
            windowGrid.ColumnDefinitions[6].MinWidth = 0;
            windowGrid.ColumnDefinitions[6].Width = GridLength.Auto;
            previewSplitter.Visibility = Visibility.Hidden;
        }

        private void explorerExpander_Collapsed(object sender, RoutedEventArgs e) {
            explorerExpanderMinWidth = windowGrid.ColumnDefinitions[2].MinWidth;
            explorerExpanderWidth = windowGrid.ColumnDefinitions[2].Width;
            windowGrid.ColumnDefinitions[2].MinWidth = 0;
            windowGrid.ColumnDefinitions[2].Width = GridLength.Auto;
            explorerExpander.Visibility = Visibility.Collapsed;
            explorerSplitter.Visibility = Visibility.Collapsed;
        }

        private void explorerExpander_Expanded(object sender, RoutedEventArgs e) {
            if (double.IsNaN(explorerExpanderMinWidth)) return;
            windowGrid.ColumnDefinitions[2].MinWidth = explorerExpanderMinWidth;
            windowGrid.ColumnDefinitions[2].Width = explorerExpanderWidth;
            explorerExpander.Visibility = Visibility.Visible;
            explorerSplitter.Visibility = Visibility.Visible;
        }

        #endregion

        #region Public Methods

        public void FillOptionsPanel(UIElement element, bool expand) {
            optionsExpander.Content = element;
            optionsExpander.Visibility = Visibility.Visible;
            optionsExpander.IsExpanded = expand;
        }

        public void EmptyOptionsPanel() {
            optionsExpander.Content = null;
            optionsExpander.Visibility = Visibility.Collapsed;
        }
        
        public void FillPreviewPanel(DataProjectTreeItem dPTI, bool expand) {
            List<DataProjectTreeItem> relData = projectTreeView.GetRelatedItems(dPTI);
            if (relData.Count == 0) {
                EmptyPreviewPanel();
            } else {
                previewList.ItemsSource = relData;
                previewExpander.Visibility = Visibility.Visible;
                previewExpander.IsExpanded = expand;
            }
        }

        public void EmptyPreviewPanel() {
            previewList.ItemsSource = null;
            previewExpander.IsExpanded = false;
            previewExpander.Visibility = Visibility.Collapsed;
        }

        public void UpdatePreviewPanel() {
            TreeViewItem selItem = projectTreeView.SelectedItem;
            if (selItem != null) {
                if (selItem.Tag is DataProjectTreeItem) {
                    FillPreviewPanel((DataProjectTreeItem)selItem.Tag, true);
                } else {
                    EmptyPreviewPanel();
                }
            }
        }

        #endregion

        private void importButton_Click(object sender, RoutedEventArgs e) {
            explorerExpander.IsExpanded = true;
        }

        private void ProjectTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            EmptyOptionsPanel();
            TreeViewItem item = e.OldValue as TreeViewItem;
            if (item != null) {
                if (item.Tag is ProjectTreeItem) {
                    ProjectTreeItem pTI = (ProjectTreeItem)item.Tag;
                    pTI.TakeScreenShot();
                }
                visualizationLayer.VisualizationData = item.Tag;
            }
            UpdatePreviewPanel();
        }

        private void projectTreeView_NewFileInserted(object sender, RoutedEventArgs e) {
            PropertyGrid pGrid = new PropertyGrid();
            pGrid.SelectedObject = ParserManagement.CurrentParser;
            pGrid.PropertyChanged += pGrid_PropertyChanged;
            FillOptionsPanel(pGrid, true);
        }

        void pGrid_PropertyChanged(object sender, RoutedEventArgs e) {
            projectTreeView.ParseAgain(ParserManagement.CurrentParser);
            visualizationLayer.VisualizationData = projectTreeView.SelectedItem.Tag;
        }
	}
}