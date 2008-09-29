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
using System.ComponentModel;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für PropertyGrid.xaml
    /// </summary>
    public partial class PropertyGrid : UserControl {

        public static readonly RoutedEvent PropertyChangedEvent;
        static PropertyGrid() {
            PropertyGrid.PropertyChangedEvent = EventManager.RegisterRoutedEvent(
                "PropertyChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyGrid));
        }
        public event RoutedEventHandler PropertyChanged {
            add { base.AddHandler(PropertyGrid.PropertyChangedEvent, value); }
            remove { base.RemoveHandler(PropertyGrid.PropertyChangedEvent, value); }
        }

        private object selectedObject;
        private List<Property> properties;

        public object SelectedObject {
            get { return selectedObject; }
            set { 
                selectedObject = value;
                InitPropertyGrid();
            }
        }

        public PropertyGrid() {
            InitializeComponent();
        }

        private void InitPropertyGrid() {
            properties = new List<Property>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(selectedObject)) {
                if (!property.IsBrowsable) continue;
                Property prop = new Property(selectedObject, property);
                prop.PropertyChanged += prop_PropertyChanged;
                properties.Add(prop);
            }
            itemsControl.ItemsSource = properties;
        }

        void prop_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            RoutedEventArgs args = new RoutedEventArgs(PropertyGrid.PropertyChangedEvent, this);
            base.RaiseEvent(args);
        }

    }
}
