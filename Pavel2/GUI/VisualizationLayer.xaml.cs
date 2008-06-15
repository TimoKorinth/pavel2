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

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für VisualizationLayer.xaml
    /// </summary>
    public partial class VisualizationLayer : UserControl {

        private object visualizationData;
        private DataGrid currentDataGrid;

        public DataGrid CurrentDataGrid {
            get { return currentDataGrid; }
            set {
                currentDataGrid = value;
                if (CurrentVisualization != null) {
                    VisualizationTabFocus(CurrentVisualization);
                }
            }
        }

        public object VisualizationData {
            get { return visualizationData; }
            set { 
                visualizationData = value;
                if (CurrentVisualization == null) CurrentVisualization = (Visualization)visualizationTabControl.SelectedContent;
                SetCurrentDataGrid();
            }
        }

        public VisualizationLayer() {
            InitializeComponent();
            InitVisualizationTab();
        }

        public Visualization CurrentVisualization {
            get {
                if (visualizationData is ProjectTreeItem) {
                    ProjectTreeItem pTI = (ProjectTreeItem)visualizationData;
                    return pTI.LastVisualization;
                }
                return null;
            }
            set {
                if (visualizationData is ProjectTreeItem) {
                    ProjectTreeItem pTI = (ProjectTreeItem)visualizationData;
                    pTI.LastVisualization = value;
                }
            }
        }

        public void VisualizationTabFocus(Visualization vis) {
            foreach (TabItem item in visualizationTabControl.Items) {
                if (item.Content.Equals(vis)) {
                    CurrentVisualization.Render();
                    item.IsSelected = true;
                }
            }
        }

        private void SetCurrentDataGrid() {
            if (visualizationData != null) {
                if (visualizationData is ProjectTreeItem) {
                    ProjectTreeItem ptItem = (ProjectTreeItem)visualizationData;
                    this.CurrentDataGrid = ptItem.DataGrid;
                } else {
                    this.CurrentDataGrid = null;
                }
            } else {
                this.CurrentDataGrid = null;
            }
        }

        private void InitVisualizationTab() {
            visualizationTabControl.Items.Clear();
            Type[] types = System.Reflection.Assembly.GetAssembly(typeof(Visualization)).GetTypes();
            foreach (Type t in types) {
                Type tmp = t.GetInterface("Visualization");
                if (tmp != null) {
                    TabItem tabItem = new TabItem();
                    tabItem.Header = t.Name;
                    Visualization vis = (Visualization)Activator.CreateInstance(t);
                    tabItem.Content = vis;
                    visualizationTabControl.Items.Add(tabItem);
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
            CurrentVisualization.Render();
        }

        private void visualizationTabControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (CurrentVisualization == null) return;
            CurrentVisualization.RenderAfterResize();
        }
    }
}
