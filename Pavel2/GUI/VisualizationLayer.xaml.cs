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
            if (!(visualizationData is Column)) {
                if (visStackPanel.Children.Count == 1 && visStackPanel.Children[0] is VisTab) {
                    VisTab v = visStackPanel.Children[0] as VisTab;
                    v.Dispose();
                }

                visStackPanel.Children.Clear();
                visStackPanel.RowDefinitions.Clear();
                visStackPanel.ColumnDefinitions.Clear();
            }
            if (visualizationData is ProjectTreeItem) {
                ProjectTreeItem ptItem = (ProjectTreeItem)visualizationData;
                VisTab visTab = new VisTab(ptItem);
                visStackPanel.Children.Add(visTab);
            }
            if (visualizationData is LinkItem) {
                LinkItem lItem = (LinkItem)visualizationData;
                if (lItem.IsCombined) {
                    ProjectTreeItem ptItem = lItem.CombItem;
                    VisTab visTab = new VisTab(ptItem);
                    visStackPanel.Children.Add(visTab);
                } else {
                    ScrollViewer scroller = new ScrollViewer();
                    scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    visStackPanel.Children.Add(scroller);
                    if (lItem.IsGridView) {
                        WrapPanel wrap = new WrapPanel();
                        scroller.Content = wrap;
                        double width = this.ActualWidth / Math.Ceiling(Math.Sqrt(lItem.DataItems.Count + lItem.Images.Count));
                        double height = this.ActualHeight / Math.Ceiling((lItem.DataItems.Count + lItem.Images.Count) / Math.Ceiling(Math.Sqrt(lItem.DataItems.Count + lItem.Images.Count)));
                        foreach (DataProjectTreeItem item in lItem.DataItems) {
                            VisTab visTab = new VisTab(item);
                            visTab.MinHeight = 150;
                            visTab.MinWidth = 200;
                            visTab.MaxHeight = 500;
                            visTab.MaxWidth = 600;
                            visTab.Height = height;
                            visTab.Width = width;
                            wrap.Children.Add(visTab);
                        }
                        foreach (ImageTreeItem imgItem in lItem.Images) {
                            Image img = new Image();
                            img.MinHeight = 150;
                            img.MinWidth = 200;
                            img.MaxHeight = 500;
                            img.MaxWidth = 600;
                            img.Source = imgItem.ImageSource;
                            img.Stretch = Stretch.Uniform;
                            img.Height = height;
                            img.Width = width;
                            if (img.Source.Height < img.Height) {
                                img.Height = img.Source.Height;
                                img.MinHeight = img.Height;
                            }
                            wrap.Children.Add(img);
                        }
                    } else {
                        StackPanel stack = new StackPanel();
                        scroller.Content = stack;
                        foreach (DataProjectTreeItem item in lItem.DataItems) {
                            VisTab visTab = new VisTab(item);
                            visTab.MinHeight = 150;
                            visTab.MaxHeight = 500;
                            visTab.Height = this.ActualHeight / (lItem.DataItems.Count + lItem.Images.Count);
                            stack.Children.Add(visTab);
                        }
                        foreach (ImageTreeItem imgItem in lItem.Images) {
                            Image img = new Image();
                            img.MinHeight = 150;
                            img.MaxHeight = 500;
                            img.Source = imgItem.ImageSource;
                            img.Stretch = Stretch.Uniform;
                            img.Height = this.ActualHeight / (lItem.DataItems.Count + lItem.Images.Count);
                            if (img.Source.Height < img.Height) {
                                img.MinHeight = img.Height;
                                img.Height = img.Source.Height;
                            }
                            stack.Children.Add(img);
                        }
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
