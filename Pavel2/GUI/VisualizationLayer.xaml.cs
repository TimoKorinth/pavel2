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

        private LinkItem lastLinkItem;
        private object visualizationData;

        public object VisualizationData {
            get { return visualizationData; }
            set { 
                visualizationData = value;
                if (visualizationData is LinkItem) lastLinkItem = visualizationData as LinkItem;
                Display();
            }
        }

        private void Display() {
            if (!(visualizationData is Column)) {
                visStackPanel.Children.Clear();
                visStackPanel.RowDefinitions.Clear();
                visStackPanel.ColumnDefinitions.Clear();
            }
            if ((visualizationData is CombinedDataItem) || (visualizationData is LinkItem)) {
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                visStackPanel.RowDefinitions.Add(row);
                Button sep = new Button();
                sep.Content = "Separate";
                sep.Click += sep_Click;
                Button tog = new Button();
                tog.Content = "Combined";
                tog.Click += tog_Click;
                if (lastLinkItem != null) {
                    if (!lastLinkItem.IsCombineable) tog.IsEnabled = false;
                }
                ToolBar bar = new ToolBar();
                bar.Items.Add(sep);
                bar.Items.Add(tog);
                Grid.SetRow(bar, 0);
                visStackPanel.Children.Add(bar);
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
                int i = 1;
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

        void sep_Click(object sender, RoutedEventArgs e) {
            if (lastLinkItem != null) {
                this.VisualizationData = lastLinkItem;
            }
        }

        void tog_Click(object sender, RoutedEventArgs e) {
            if (lastLinkItem != null) {
                CombinedDataItem item = new CombinedDataItem(lastLinkItem.DataItems);
                this.VisualizationData = item;
            }
        }

        public VisualizationLayer() {
            InitializeComponent();
        }
    }
}
