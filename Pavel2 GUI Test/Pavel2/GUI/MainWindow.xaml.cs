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
        public MainWindow() {
			this.InitializeComponent();
		}

        private DataGrid currentDataGrid;

        public DataGrid CurrentDataGrid {
            get {
                return currentDataGrid;
            }
            set {
                currentDataGrid = value;
                //if (visualization != null) visualization.Render();
            }
        }
	}
}