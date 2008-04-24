using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class Column {

        private IPoint[] points = new IPoint[0];
        private String header = "";

        public String Header {
            get { return header; }
            set { header = value; }
        }

        public IPoint[] Points {
            get { return points; }
            set { points = value; }
        }

    }
}
