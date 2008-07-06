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
    /// Interaktionslogik für ParserOptions.xaml
    /// </summary>
    public partial class ParserOptions : UserControl {
        public ParserOptions() {
            InitializeComponent();
            InitParserList();
        }

        private void InitParserList() {
            parserList.ItemsSource = ParserManagement.ParserList;
            parserList.DisplayMemberPath = "Name";
            parserList.SelectionChanged += parserList_SelectionChanged;
        }

        void parserList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            parserPropertyGrid.SelectedObject = parserList.SelectedItem as Parser;
        }

        public object SelectedObject {
            get { return parserPropertyGrid.SelectedObject; }
            set {
                if (value is Parser) {
                    foreach (object ob in parserList.Items) {
                        if (ob.GetType().Equals(value.GetType())) parserList.SelectedItem = ob;
                    }
                }
                parserPropertyGrid.SelectedObject = value;
            }
        }
    }
}
