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
    /// Interaktionslogik für TableView.xaml
    /// </summary>
    public partial class TableView : UserControl, Visualization {

        private DataGrid dataGrid;
        
        public TableView() {
            InitializeComponent();
        }

        public void Render() {
            dataGrid = MainData.CurrentDataGrid;
            if (dataGrid != null) {
                tableListView.ItemsSource = dataGrid.Data;
                GridView gView = new GridView();
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
                tableListView.ItemsSource = null;
                tableListView.View = null;
            }
        }
    }
}
