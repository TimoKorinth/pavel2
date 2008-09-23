using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Pavel2.Framework;
using System.Windows.Media;
using System.Windows;

namespace Pavel2.GUI {
    public interface Visualization {

        void Render(DataGrid dataGrid);

        void RenderAfterResize();

        bool OwnScreenshot();

        ImageSource GetScreenshot();

        object GetProperties();

        UIElement GetUIElement();

    }
}
