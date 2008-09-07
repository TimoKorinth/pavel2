﻿using System;
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
using System.IO;

namespace Pavel2.GUI
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window {

        #region Expander Fields

        private double projectTreeExpanderMinWidth = double.NaN;
        private GridLength projectTreeExpanderWidth;
        private double previewExpanderMinWidth = double.NaN;
        private GridLength previewExpanderWidth;
        private double explorerExpanderMinWidth = double.NaN;
        private GridLength explorerExpanderWidth;

        #endregion

        private bool optionsExpanded = true;

        public MainWindow() {
			this.InitializeComponent();

            EmptyPreviewPanel();
            RemoveOptionsPanel();
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

        public void CreateOptionsPanel(UIElement element) {
            StackPanel stack = new StackPanel();
            stack.Children.Add(element);
            optionsExpander.Content = stack;
            optionsExpander.Visibility = Visibility.Visible;
            optionsExpander.IsExpanded = optionsExpanded;
        }

        public void RemoveOptionsPanel() {
            StackPanel stack = optionsExpander.Content as StackPanel;
            if (stack == null) {
                optionsExpander.Content = null;
                optionsExpander.Visibility = Visibility.Collapsed;
                return;
            }
            foreach (UIElement el in stack.Children) {
                if (el is ParserOptions) {
                    ParserOptions pOpt = (ParserOptions)el;
                    pOpt.parserPropertyGrid.PropertyChanged -= pGrid_PropertyChanged;
                    pOpt.parserList.SelectionChanged -= parserList_PropertyChanged;
                } 
            }
            optionsExpander.Content = null;
            optionsExpander.Visibility = Visibility.Collapsed;
            optionsExpanded = optionsExpander.IsExpanded;
        }

        public void AddToOptionsPanel(UIElement element) {
            StackPanel stack = optionsExpander.Content as StackPanel;
            if (stack == null) {
                CreateOptionsPanel(element);
            } else {
                stack.Children.Add(element);
            }
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
            RemoveOptionsPanel();
            TreeViewItem item = projectTreeView.SelectedItem;
            if (item != null) {
                if (item.Tag is ProjectTreeItem) {
                    ProjectTreeItem pTI = (ProjectTreeItem)item.Tag;
                    pTI.TakeScreenShot();
                    AddDataGridOptions(pTI.DataGrid);
                }
                visualizationLayer.VisualizationData = item.Tag;
                ShowParserProperties();
            }
            UpdatePreviewPanel();
        }

        private void AddDataGridOptions(DataGrid d) {
            PropertyGrid pGrid = new PropertyGrid();
            pGrid.SelectedObject = d;
            pGrid.PropertyChanged += pGridDataGrid_PropertyChanged;
            AddToOptionsPanel(pGrid);
        }

        private void ShowParserProperties() {
            if (projectTreeView.SelectedItem.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = (DataProjectTreeItem)projectTreeView.SelectedItem.Tag;
                ParserOptions pOpts = new ParserOptions();
                pOpts.SelectedObject = dPTI.Parser;
                pOpts.parserPropertyGrid.PropertyChanged += pGrid_PropertyChanged;
                pOpts.parserList.SelectionChanged += parserList_PropertyChanged;
                AddToOptionsPanel(pOpts);
            }
        }

        void pGridDataGrid_PropertyChanged(object sender, RoutedEventArgs e) {
            if (projectTreeView.SelectedItem.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = (DataProjectTreeItem)projectTreeView.SelectedItem.Tag;
                visualizationLayer.VisualizationData = dPTI;
            }
        }

        void pGrid_PropertyChanged(object sender, RoutedEventArgs e) {
            if (projectTreeView.SelectedItem.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = (DataProjectTreeItem)projectTreeView.SelectedItem.Tag;
                try {
                    FileInfo file = new FileInfo(dPTI.Filename);
                    projectTreeView.ParseAgain(dPTI.Parser, file);
                } catch (Exception) {
                    //TODO: Fehlermeldung und automatische Anzeige eines File Open Dialoges
                }
                visualizationLayer.VisualizationData = dPTI;
            }
        }

        void parserList_PropertyChanged(object sender, RoutedEventArgs e) {
            ComboBox parserList = sender as ComboBox;
            if (parserList == null) return;
            if (projectTreeView.SelectedItem.Tag is DataProjectTreeItem) {
                DataProjectTreeItem dPTI = (DataProjectTreeItem)projectTreeView.SelectedItem.Tag;
                try {
                    FileInfo file = new FileInfo(dPTI.Filename);
                    projectTreeView.ParseAgain(parserList.SelectedItem as Parser, file); //Hier Parser aus Liste holen
                } catch (Exception) {
                    //TODO: Fehlermeldung und automatische Anzeige eines File Open Dialoges
                }
                visualizationLayer.VisualizationData = dPTI;
            }
        }

        private void saveMenu_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "PavelProject";
            dlg.DefaultExt = ".pav2";
            dlg.Filter = "Pavel 2 documents (.pav2)|*.pav2";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true) {
                string filename = dlg.FileName;
                ProjectHelper.SaveProject(filename);
            }
        }

        private void openMenu_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "PavelProject";
            dlg.DefaultExt = ".pav2";
            dlg.Filter = "Pavel 2 documents (.pav2)|*.pav2";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true) {
                string filename = dlg.FileName;
                ProjectHelper.OpenProject(filename);
            }
        }

        private void previewList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataProjectTreeItem dPTI = previewList.SelectedItem as DataProjectTreeItem;
            if (dPTI == null) return;
            projectTreeView.Select(dPTI);
        }
	}
}