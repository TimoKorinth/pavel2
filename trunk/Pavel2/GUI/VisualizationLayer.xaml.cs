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
            visStackPanel.Children.Clear();
            visStackPanel.RowDefinitions.Clear();
            visStackPanel.ColumnDefinitions.Clear();
            if (visualizationData is ProjectTreeItem) {
                ProjectTreeItem ptItem = (ProjectTreeItem)visualizationData;
                visStackPanel.Children.Add(new VisTab(ptItem));
            }
            if (visualizationData is LinkItem) {
                LinkItem lItem = (LinkItem)visualizationData;
                int i = 0;
                foreach (DataProjectTreeItem item in lItem.DataItems) {
                    visStackPanel.RowDefinitions.Add(new RowDefinition());
                    VisTab visTab = new VisTab(item);
                    Grid.SetRow(visTab, i);
                    visStackPanel.Children.Add(visTab);
                    i++;
                }
            }
        }

        public VisualizationLayer() {
            InitializeComponent();
        }
    }
}
