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
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                visStackPanel.RowDefinitions.Add(row);

                Button sep = new Button();
                sep.Content = "Separate";
                sep.Click += sep_Click;
                if (!lItem.IsCombined) sep.IsEnabled = false;
                Button tog = new Button();
                tog.Content = "Combined";
                tog.Click += tog_Click;
                if (!lItem.IsCombineable || lItem.IsCombined) tog.IsEnabled = false;
                Grid stack = new Grid();
                stack.ColumnDefinitions.Add(new ColumnDefinition());
                ColumnDefinition cTmp = new ColumnDefinition();
                cTmp.Width = GridLength.Auto;
                stack.ColumnDefinitions.Add(cTmp);
                ToolBar bar = new ToolBar();
                bar.Items.Add(sep);
                bar.Items.Add(tog);
                stack.Children.Add(bar);
                Grid.SetRow(stack, 0);
                visStackPanel.Children.Add(stack);


                ToolBar display = new ToolBar();
                stack.Children.Add(display);
                Grid.SetColumn(display, 1);
                Button list = new Button();
                Image listImg = new Image();
                listImg.ToolTip = "List View";
                if (lItem.IsCombined) list.Visibility = Visibility.Hidden;
                listImg.Source = new BitmapImage(new Uri("Icons/list.png", UriKind.Relative));
                list.Content = listImg;
                Button grid = new Button();
                Image gridImg = new Image();
                gridImg.ToolTip = "Grid View";
                if (lItem.IsCombined) grid.Visibility = Visibility.Hidden;
                gridImg.Source = new BitmapImage(new Uri("Icons/grid.png", UriKind.Relative));
                grid.Content = gridImg;
                display.Items.Add(list);
                display.Items.Add(grid);
                if (lItem.IsCombined) {
                    ProjectTreeItem ptItem = lItem.CombItem;
                    VisTab visTab = new VisTab(ptItem);
                    visStackPanel.RowDefinitions.Add(new RowDefinition());
                    Grid.SetRow(visTab, 1);
                    visStackPanel.Children.Add(visTab);
                } else {
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
            if (this.VisualizationData is LinkItem) {
                (this.VisualizationData as LinkItem).IsCombined = false;
                Display();
            }
        }

        void tog_Click(object sender, RoutedEventArgs e) {
            if (this.VisualizationData is LinkItem) {
                (this.VisualizationData as LinkItem).IsCombined = true;
                Display();
            }
        }

        public VisualizationLayer() {
            InitializeComponent();
        }
    }
}
