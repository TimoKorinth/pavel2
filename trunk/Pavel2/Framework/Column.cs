using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class Column {

        private IPoint[] points = new IPoint[0];
        private String header = "";
        private double min;
        private double max;

        public double Max {
            get { return max; }
            set { max = value; }
        }

        public double Min {
            get { return min; }
            set { min = value; }
        }

        public String Header {
            get { return header; }
            set { header = value; }
        }

        public IPoint[] Points {
            get { return points; }
            set { points = value; }
        }

        public Column() {
        }

        public Column(String header) {
            this.header = header;
        }

    }
}
