using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Pavel2.Framework;

namespace Pavel2.GUI {
    public interface Visualization {

        void Render(DataGrid dataGrid);

        void RenderAfterResize();

    }
}
