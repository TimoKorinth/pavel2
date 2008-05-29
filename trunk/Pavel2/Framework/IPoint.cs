using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public abstract class IPoint {

        protected double doubleData = double.NaN;
        protected String data = "";

        public String Data {
            get { return data; }
        }

        public double DoubleData {
            get { return doubleData; }
        }

    }
}
