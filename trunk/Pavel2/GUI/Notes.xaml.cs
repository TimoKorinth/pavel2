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
    /// Interaktionslogik für Notes.xaml
    /// </summary>
    public partial class Notes : UserControl, Visualization {

        private DataGrid dataGrid;
        
        public Notes() {
            InitializeComponent();
        }

        #region Visualization Member

        public void Render(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
            Binding binding = new Binding("Notes");
            binding.Source = dataGrid;
            textBox.SetBinding(TextBox.TextProperty, binding);
        }

        public void RenderAfterResize() {

        }

        public bool OwnScreenshot() {
            return false;
        }

        public ImageSource GetScreenshot() {
            return null;
        }

        public object GetProperties() {
            return null;
        }

        public UIElement GetUIElement() {
            return null;
        }

        #endregion

        #region IDisposable Member

        public void Dispose() {
            this.dataGrid = null;
        }

        #endregion
    }
}
