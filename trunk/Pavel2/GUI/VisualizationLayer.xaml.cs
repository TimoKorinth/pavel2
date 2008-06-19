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

        public object VisualizationData {
            get { return visualizationData; }
            set { 
                visualizationData = value;
                Display();
            }
        }

        private void Display() {
            if (visualizationData == null) {
                visStackPanel.Children.Clear();
            }
            if (visualizationData is ProjectTreeItem) {
                ProjectTreeItem ptItem = (ProjectTreeItem)visualizationData;
                visStackPanel.Children.Clear();
                visStackPanel.Children.Add(new VisTab(ptItem));
            }
            if (visualizationData is LinkItem) {
                
            }
        }

        public VisualizationLayer() {
            InitializeComponent();
        }
    }
}
