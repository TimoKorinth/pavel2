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
using System.IO;
using System.Windows.Interop;

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
        private TreeViewItem lastItem;
        private PropertyGrid pGridlastDataGrid = new PropertyGrid();

        #endregion

        public MainWindow() {
			this.InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.StateChanged += MainWindow_StateChanged;

            EmptyPreviewPanel();
            previewExpander.IsExpanded = false;
            RemoveOptionsPanel();
            optionsExpander.IsExpanded = true;
            projectTreeView.root.IsSelected = true;
		}

        void MainWindow_StateChanged(object sender, EventArgs e) {
            TreeViewItem tvItem = projectTreeView.SelectedItem;
            if (tvItem.Tag is ProjectTreeItem) {
                (tvItem.Tag as ProjectTreeItem).LastVisualization.RenderAfterResize();
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            HwndTarget hwndTarget = hwndSource.CompositionTarget;
            // this is the new WPF API to force render mode.
            hwndTarget.RenderMode = RenderMode.SoftwareOnly;

            if (hwndSource != null) {
                hwndSource.AddHook(new HwndSourceHook(WinProc));
            }
        }

        public const Int32 WM_EXITSIZEMOVE = 0x0232;
        private IntPtr WinProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled) {
            IntPtr result = IntPtr.Zero;
            if (msg == WM_EXITSIZEMOVE) {
                TreeViewItem tvItem = projectTreeView.SelectedItem;
                if (tvItem == null) return result;
                if (tvItem.Tag is ProjectTreeItem) {
                    (tvItem.Tag as ProjectTreeItem).LastVisualization.RenderAfterResize();
                }
                //if (CurrentVisualization != null) CurrentVisualization.RenderAfterResize();
            }
            return result;
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
        }

        public void AddToOptionsPanel(UIElement element) {
            StackPanel stack = optionsExpander.Content as StackPanel;
            if (stack == null) {
                CreateOptionsPanel(element);
            } else {
                if (element != null) stack.Children.Add(element);
            }
        }

        public Button GetToolBarButton(String desc, ImageSource imgSource, String toolTip) {
            Button btn = new Button();
            btn.Margin = new Thickness(0,0,5,0);
            btn.Width = 60;
            btn.Height = 50;
            StackPanel stack = new StackPanel();
            Image img = new Image();
            img.Width = 20;
            img.Height = 20;
            img.Source = imgSource;
            stack.Children.Add(img);
            Label l = new Label();
            l.Content = desc;
            stack.Children.Add(l);
            btn.ToolTip = toolTip;
            btn.Content = stack;
            return btn;
        }

        public void SetupToolBarButtons(DataGrid d) {
            if (!(projectTreeView.SelectedItem.Tag is LinkItem)) visToolBar.Children.Clear();
            foreach (Button btn in d.Buttons) {
                visToolBar.Children.Add(btn);
            }
        }

        public void SetupLinkToolBarButtons(LinkItem item) {
            visToolBar.Children.Clear();
            if (item.IsCombined) {
                Button sep = GetToolBarButton("Separate", new BitmapImage(new Uri("Icons/table_multiple.png", UriKind.Relative)), "Show separate");
                sep.Tag = item;
                sep.Click += sep_Click;
                visToolBar.Children.Add(sep);
            } else {
                if (item.IsCombineable) {
                    Button comb = GetToolBarButton("Combined", new BitmapImage(new Uri("Icons/table_link.png", UriKind.Relative)), "Show combined");
                    comb.Tag = item;
                    comb.Click += comb_Click;
                    visToolBar.Children.Add(comb);
                }
                if (item.IsGridView) {
                    Button list = GetToolBarButton("Stack", new BitmapImage(new Uri("Icons/list.png", UriKind.Relative)), "Stack View");
                    list.Tag = item;
                    list.Click += list_Click;
                    visToolBar.Children.Add(list);
                } else {
                    Button grid = GetToolBarButton("Grid", new BitmapImage(new Uri("Icons/grid.png", UriKind.Relative)), "Grid View");
                    grid.Click += grid_Click;
                    grid.Tag = item;
                    visToolBar.Children.Add(grid);
                }
            }
        }

        void grid_Click(object sender, RoutedEventArgs e) {
            Button grid = sender as Button;
            if (grid == null) return;
            LinkItem item = grid.Tag as LinkItem;
            item.IsGridView = true;
            visualizationLayer.VisualizationData = item;
            SetupLinkToolBarButtons(item);
        }

        void list_Click(object sender, RoutedEventArgs e) {
            Button list = sender as Button;
            if (list == null) return;
            LinkItem item = list.Tag as LinkItem;
            item.IsGridView = false;
            visualizationLayer.VisualizationData = item;
            SetupLinkToolBarButtons(item);
        }

        void sep_Click(object sender, RoutedEventArgs e) {
            Button sep = sender as Button;
            if (sep == null) return;
            LinkItem item = sep.Tag as LinkItem;
            item.IsCombined = false;
            visualizationLayer.VisualizationData = item;
            SetupLinkToolBarButtons(item);
            projectTreeView.UpdateLinkItem(projectTreeView.SelectedItem);
        }

        void comb_Click(object sender, RoutedEventArgs e) {
            Button comb = sender as Button;
            if (comb == null) return;
            LinkItem item = comb.Tag as LinkItem;
            item.IsCombined = true;
            visualizationLayer.VisualizationData = item;
            SetupLinkToolBarButtons(item);

            AddDataGridOptions(item.CombItem.DataGrid);
            SetupToolBarButtons(item.CombItem.DataGrid);
            pointStatus.Visibility = Visibility.Visible;
            pointStatus.Content = item.CombItem.DataGrid.MaxPoints + " Points";
            selectionStatus.Visibility = Visibility.Collapsed;
            item.CombItem.DataGrid.ShowNumberSelPoints();
            projectTreeView.UpdateLinkItem(projectTreeView.SelectedItem);
        }
        
        public void FillPreviewPanel(DataProjectTreeItem dPTI, bool expand) {
            List<DataProjectTreeItem> relData = projectTreeView.GetRelatedItems(dPTI);
            if (relData.Count == 0) {
                EmptyPreviewPanel();
            } else {
                previewList.ItemsSource = relData;
                previewExpander.Visibility = Visibility.Visible;
            }
        }

        public void EmptyPreviewPanel() {
            previewList.ItemsSource = null;
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
            explorerExpander.IsExpanded = !explorerExpander.IsExpanded;
        }

        private void exportButton_Click(object sender, RoutedEventArgs e) {
            TreeViewItem item = projectTreeView.SelectedItem;
            if (item != null) {
                if (item.Tag is ProjectTreeItem) {
                    ProjectTreeItem pTI = (ProjectTreeItem)item.Tag;
                    pTI.TakeScreenShot();
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = pTI.LastVisualization.GetType().ToString();
                    dlg.DefaultExt = ".png";
                    dlg.Filter = "PNG file (.png)|*.png|JPEG file (.jpg)|*.jpg|Gif file (.gif)|*.gif";
                    Nullable<bool> result = dlg.ShowDialog();
                    if (result == true) {
                        ExportToPng(new Uri(dlg.FileName), (BitmapSource)pTI.Screenshot);
                    }
                }
            }
        }

        public void ExportToPng(Uri path, BitmapSource surface) {
            if (path == null) return;
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create)) {
                string Extension = System.IO.Path.GetExtension(path.AbsolutePath).ToLower();
                BitmapEncoder encoder;
                if (Extension == ".gif")
                    encoder = new GifBitmapEncoder();
                else if (Extension == ".png")
                    encoder = new PngBitmapEncoder();
                else if (Extension == ".jpg")
                    encoder = new JpegBitmapEncoder();
                else
                    return;
                encoder.Frames.Add(BitmapFrame.Create(surface));
                encoder.Save(outStream);
            }
        }

        private void ProjectTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (lastItem != null && lastItem.Equals(projectTreeView.SelectedItem)) return;
            lastItem = projectTreeView.SelectedItem;
            RemoveOptionsPanel();
            pointStatus.Visibility = Visibility.Collapsed;
            TreeViewItem item = projectTreeView.SelectedItem;
            if (item != null) {
                if (item.Tag is FolderProjectTreeItem) {
                    (item.Tag as FolderProjectTreeItem).UpdateDataGrid();
                }
                if (item.Tag is ProjectTreeItem) {
                    ProjectTreeItem pTI = (ProjectTreeItem)item.Tag;
                    if (pTI.DataGrid == null) return;
                    pTI.DataGrid.UpdateAll();
                    AddDataGridOptions(pTI.DataGrid);
                    if (pTI.DataGrid != null) SetupToolBarButtons(pTI.DataGrid);
                    pointStatus.Visibility = Visibility.Visible;
                    pointStatus.Content = pTI.DataGrid.MaxPoints + " Points";
                    selectionStatus.Visibility = Visibility.Collapsed;
                    pTI.DataGrid.ShowNumberSelPoints();
                }
                if (item.Tag is LinkItem) {
                    LinkItem lItem = item.Tag as LinkItem;
                    SetupLinkToolBarButtons(lItem);
                    if (lItem.IsCombined) {
                        AddDataGridOptions(lItem.CombItem.DataGrid);
                        SetupToolBarButtons(lItem.CombItem.DataGrid);
                        pointStatus.Visibility = Visibility.Visible;
                        pointStatus.Content = lItem.CombItem.DataGrid.MaxPoints + " Points";
                        selectionStatus.Visibility = Visibility.Collapsed;
                        lItem.CombItem.DataGrid.ShowNumberSelPoints();
                    }
                }
                if (item.Tag is ImageTreeItem) {
                    visToolBar.Children.Clear();
                }
                visualizationLayer.VisualizationData = item.Tag;
                ShowParserProperties();
                UpdatePreviewPanel();
            }
        }

        public void AddDataGridOptions(DataGrid d) {
            if (pGridlastDataGrid != null) {
                pGridlastDataGrid.PropertyChanged -= pGridDataGrid_PropertyChanged;
            }
            PropertyGrid pGrid = new PropertyGrid();
            pGrid.SelectedObject = d;
            pGrid.PropertyChanged += pGridDataGrid_PropertyChanged;
            AddToOptionsPanel(pGrid);
            pGridlastDataGrid = pGrid;
        }

        public void ShowParserProperties() {
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
            UpdateVisualization();
        }

        public void UpdateVisualization() {
            if (projectTreeView.SelectedItem.Tag is ProjectTreeItem) {
                ProjectTreeItem dPTI = (ProjectTreeItem)projectTreeView.SelectedItem.Tag;
                visualizationLayer.VisualizationData = dPTI;
            }
            if (projectTreeView.SelectedItem.Tag is LinkItem) {
                LinkItem item = (LinkItem)projectTreeView.SelectedItem.Tag;
                visualizationLayer.VisualizationData = item;
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