using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Pavel2.Framework;

namespace Pavel2.GUI {

    public class PropertyTemplateSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            Property property = item as Property;
            FrameworkElement element = container as FrameworkElement;
            if (element == null) {
                return base.SelectTemplate(property.Value, container);
            }
            DataTemplate template = FindDataTemplate(property, element);
            return template;
        }

        private DataTemplate FindDataTemplate(Property property, FrameworkElement element) {
            Type propertyType = property.PropertyType;
            DataTemplate template = element.TryFindResource(propertyType) as DataTemplate;
            while (template == null && propertyType.BaseType != null) {
                propertyType = propertyType.BaseType;
                template = element.TryFindResource(propertyType) as DataTemplate;
            }
            if (template == null) {
                template = element.TryFindResource("default") as DataTemplate;
            }
            return template;
        }

        //private static DataTemplate TryFindDataTemplate(FrameworkElement element, object dataTemplateKey) {
        //    object dataTemplate = element.TryFindResource(dataTemplateKey);
        //    if (dataTemplate == null) {
        //        dataTemplateKey = new ComponentResourceKey(typeof(PropertyGrid), dataTemplateKey);
        //        dataTemplate = element.TryFindResource(dataTemplateKey);
        //    }
        //    return dataTemplate as DataTemplate;
        //}

    }
}
