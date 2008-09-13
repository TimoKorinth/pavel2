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
using System.Windows.Controls.Primitives;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für TableView.xaml
    /// </summary>
    public partial class TableView : UserControl, Visualization {

        private DataGrid dataGrid;
        private bool selecting;
        
        public TableView() {
            InitializeComponent();
            tableListView.SelectionChanged += tableListView_SelectionChanged;
        }

        void tableListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!selecting) {
                IEnumerable<Int32> tmp = GetSelectedIndices(tableListView);
                this.dataGrid.ClearSelectedPoints();
                foreach (int i in tmp) {
                    this.dataGrid.SelectedPoints[i] = true;
                }
            }
        }

        private IEnumerable<Int32> GetSelectedIndices(ListView listView) {
            return from item in listView.SelectedItems.OfType<Object>()
                   let container = listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem
                   where container != null
                   select listView.ItemContainerGenerator.IndexFromContainer(container);
        }

        public void Render(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            tableListView.SelectedItems.Clear();
            if (dataGrid != null) {
                tableListView.Visibility = Visibility.Visible;
                tableListView.ItemsSource = dataGrid.DataField;
                GridView gView = new GridView();
                gView.Columns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
                for (int i = 0; i < dataGrid.Columns.Length; i++) {
                    GridViewColumn gColumn = new GridViewColumn();
                    gColumn.Header = dataGrid.Columns[i].Header;
                    Binding bind = new Binding();
                    bind.Path = new PropertyPath("[" + i + "]");
                    gColumn.DisplayMemberBinding = bind;
                    gView.Columns.Add(gColumn);
                }
                tableListView.View = gView;
            } else {
                tableListView.Visibility = Visibility.Collapsed;
                tableListView.ItemsSource = null;
                tableListView.View = null;
            }
            selecting = true;
            for (int row = 0; row < dataGrid.MaxPoints; row++) {
                if (dataGrid.SelectedPoints[row]) {
                    tableListView.SelectedItems.Add(dataGrid.DataField[row]);
                }
            }
            selecting = false;
        }

        void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move) {
                dataGrid.ChangeColOrder(dataGrid.Columns[e.OldStartingIndex], e.NewStartingIndex);
            }
        }

        public void RenderAfterResize() {
        }

        #region Visualization Member


        public bool OwnScreenshot() {
            return false;
        }

        public ImageSource GetScreenshot() {
            return null;
        }

        #endregion
    }
}
