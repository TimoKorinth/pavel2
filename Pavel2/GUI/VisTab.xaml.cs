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
using Pavel2.Framework;
using System.Windows.Interop;
using System.Windows.Threading;
using System.ComponentModel;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für VisTab.xaml
    /// </summary>
    public partial class VisTab : UserControl, IDisposable {

        private ProjectTreeItem pTI;

        public ProjectTreeItem PTI {
            get { return pTI; }
            set { pTI = value; }
        }

        public VisTab() {
            InitializeComponent();
            InitVisualizationTab();
        }

        public VisTab(ProjectTreeItem pTI) : this() {
            this.pTI = pTI;
            if (CurrentVisualization == null) CurrentVisualization = (Visualization)visualizationTabControl.SelectedContent;
            VisualizationTabFocus();
        }

        public Visualization CurrentVisualization {
            get {
                if (pTI != null) {
                    return pTI.LastVisualization;
                }
                return null;
            }
            set {
                if (pTI != null) {
                    pTI.LastVisualization = value;
                }
            }
        }

        private void InitVisualizationTab() {
            visualizationTabControl.Items.Clear();

            TabItem tableItem = new TabItem();
            tableItem.Header = typeof(TableView).Name;
            tableItem.Content = new TableView();
            visualizationTabControl.Items.Add(tableItem);

            Type[] types = System.Reflection.Assembly.GetAssembly(typeof(Visualization)).GetTypes();
            foreach (Type t in types) {
                Type tmp = t.GetInterface("Visualization");
                if (tmp != null) {
                    if (!t.Equals(typeof(TableView)) && !t.Equals(typeof(Notes))) {
                        TabItem tabItem = new TabItem();
                        tabItem.Header = t.Name;
                        Visualization vis = (Visualization)Activator.CreateInstance(t);
                        tabItem.Content = vis;
                        visualizationTabControl.Items.Add(tabItem);
                    }
                }
            }

            TabItem notesItem = new TabItem();
            notesItem.Header = typeof(Notes).Name;
            notesItem.Content = new Notes();
            visualizationTabControl.Items.Add(notesItem);
        }

        public void VisualizationTabFocus() {
            if (CurrentVisualization == null) return;
            foreach (TabItem item in visualizationTabControl.Items) {
                if (item.Content.GetType().Equals(CurrentVisualization.GetType())) {
                    item.IsSelected = true;
                }
            }
        }

        private void visualizationTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!(e.Source is TabControl)) return;
            if (e.AddedItems.Count > 1) return;
            TabItem tItem = e.AddedItems[0] as TabItem;
            if (tItem == null) return;
            CurrentVisualization = (Visualization)tItem.Content;
            if (CurrentVisualization == null) return;

            MainData.MainWindow.Cursor = Cursors.Wait;
            loading.Visibility = Visibility.Visible;

            //Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state) {
            //    ProjectTreeItem pTI = (ProjectTreeItem)state;
                pTI.LastVisualization.Render(pTI.DataGrid);
                pTI.TakeScreenShot();
                PropertyGrid pGrid = new PropertyGrid();
                pGrid.SelectedObject = pTI.LastVisualization.GetProperties();
                MainData.MainWindow.CreateOptionsPanel(pGrid);
                MainData.MainWindow.AddToOptionsPanel(pTI.LastVisualization.GetUIElement());
                MainData.MainWindow.AddDataGridOptions(pTI.DataGrid);
                MainData.MainWindow.ShowParserProperties();
                MainData.MainWindow.Cursor = Cursors.Arrow;
                loading.Visibility = Visibility.Collapsed;
                //return null;
            //}), this.pTI);
        }

        #region IDisposable Member

        public void Dispose() {
            pTI = null;
            foreach (TabItem item in visualizationTabControl.Items) {
                Visualization vis = (Visualization)item.Content;
                vis.Dispose();
                vis = null;
                item.Content = null;
            }
            //for (int i = 0; i < visualizationTabControl.Items.Count; i++) {
            //    visualizationTabControl.Items[i] = null;
            //}
        }

        #endregion
    }
}
