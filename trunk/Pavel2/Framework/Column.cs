using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    [Serializable()]
    public class Column {

        private IPoint[] points = new IPoint[0];
        private String header = "";
        private double min;
        private double max;
        private bool visible = true;

        public bool Visible {
            get { return visible; }
            set { visible = value; }
        }

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

        public void CalcMinMax() {
            min = double.PositiveInfinity;
            max = double.NegativeInfinity;
            for (int i = 0; i < points.Length; i++) {
            if (points[i].DoubleData != double.NaN) {
                if (points[i].DoubleData < min) min = points[i].DoubleData;
                if (points[i].DoubleData > max) max = points[i].DoubleData;
            }
            }
        }

    }
}
