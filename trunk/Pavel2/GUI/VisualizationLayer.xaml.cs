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
                //if (visualizationData is LinkItem) lastLinkItem = visualizationData as LinkItem;
                Display();
            }
        }

        private void Display() {
            if (!(visualizationData is Column)) {
                visStackPanel.Children.Clear();
                visStackPanel.RowDefinitions.Clear();
                visStackPanel.ColumnDefinitions.Clear();
            }
            if (visualizationData is ProjectTreeItem) {
                ProjectTreeItem ptItem = (ProjectTreeItem)visualizationData;
                VisTab visTab = new VisTab(ptItem);
                if (visualizationData is CombinedDataItem) {
                    visStackPanel.RowDefinitions.Add(new RowDefinition());
                    Grid.SetRow(visTab, 1);
                }
                visStackPanel.Children.Add(visTab);
            }
            if (visualizationData is LinkItem) {
                LinkItem lItem = (LinkItem)visualizationData;
                if (lItem.IsCombined) {
                    ProjectTreeItem ptItem = lItem.CombItem;
                    VisTab visTab = new VisTab(ptItem);
                    visStackPanel.Children.Add(visTab);
                } else {
                    int i = 0;
                    foreach (DataProjectTreeItem item in lItem.DataItems) {
                        visStackPanel.RowDefinitions.Add(new RowDefinition());
                        VisTab visTab = new VisTab(item);
                        Grid.SetRow(visTab, i);
                        visStackPanel.Children.Add(visTab);
                        i++;
                    }
                    foreach (ImageTreeItem imgItem in lItem.Images) {
                        visStackPanel.RowDefinitions.Add(new RowDefinition());
                        Image img = new Image();
                        img.Source = imgItem.ImageSource;
                        Grid.SetRow(img, i);
                        visStackPanel.Children.Add(img);
                        i++;
                    }
                }
            }
            if (visualizationData is ImageTreeItem) {
                ImageTreeItem imgItem = (ImageTreeItem)visualizationData;
                Image img = new Image();
                img.Source = imgItem.ImageSource;
                img.Stretch = Stretch.None;
                if (imgItem.ImageSource.Height > visStackPanel.ActualHeight || imgItem.ImageSource.Width > visStackPanel.ActualWidth) {
                    img.Stretch = Stretch.Uniform;
                }
                visStackPanel.Children.Add(img);
            }
        }

        public VisualizationLayer() {
            InitializeComponent();
        }
    }
}
