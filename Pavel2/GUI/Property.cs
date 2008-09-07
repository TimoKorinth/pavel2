using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Pavel2.GUI {
    class Property : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private object instance;
        private PropertyDescriptor property;

        public Property(object instance, PropertyDescriptor property) {
            this.instance = instance;
            this.property = property;

            this.property.AddValueChanged(instance, instance_PropertyChanged);
        }

        void instance_PropertyChanged(object sender, EventArgs e) {
            NotifyPropertyChanged("Value");
        }

        /// <value>
        /// Initializes the reflected instance property
        /// </value>
        /// <exception cref="NotSupportedException">
        /// The conversion cannot be performed
        /// </exception>
        public object Value {
            get { return property.GetValue(instance); }
            set {
                object currentValue = property.GetValue(instance);
                if (value != null && value.Equals(currentValue)) {
                    return;
                }
                Type propertyType = property.PropertyType;
                if (propertyType == typeof(object) ||
                    value == null && propertyType.IsClass ||
                    value != null && propertyType.IsAssignableFrom(value.GetType())) {
                    property.SetValue(instance, value);
                } else {
                    TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                    object convertedValue = converter.ConvertFrom(value);
                    property.SetValue(instance, convertedValue);
                }
            }
        }

        public string Name {
            get {
                if (property.Description.Length > 0) return property.Description;
                return property.Name; 
            }
        }

        public bool IsWriteable {
            get { return !IsReadOnly; }
        }

        public bool IsReadOnly {
            get { return property.IsReadOnly; }
        }

        public Type PropertyType {
            get { return property.PropertyType; }
        }

        public string Category {
            get { return property.Category; }
        }

    }
}
