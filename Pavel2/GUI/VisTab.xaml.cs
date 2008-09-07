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

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für VisTab.xaml
    /// </summary>
    public partial class VisTab : UserControl {

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

        public void VisualizationTabFocus() {
            if (CurrentVisualization == null) return;
            foreach (TabItem item in visualizationTabControl.Items) {
                if (item.Content.GetType().Equals(CurrentVisualization.GetType())) {
                    //CurrentVisualization.Render(this.pTI.DataGrid);
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
            CurrentVisualization.Render(this.pTI.DataGrid);
            this.pTI.TakeScreenShot();
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);
            HwndSource source = HwndSource.FromVisual(MainData.MainWindow) as HwndSource;
            if (source != null) {
                source.AddHook(new HwndSourceHook(WinProc));
            }
        }

        public const Int32 WM_EXITSIZEMOVE = 0x0232;
        private IntPtr WinProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled) {
            IntPtr result = IntPtr.Zero;
            switch (msg) {
                case WM_EXITSIZEMOVE: {
                        if (CurrentVisualization == null) break;
                        CurrentVisualization.RenderAfterResize();
                        break;
                    }
            }
            return result;
        }
    }
}
