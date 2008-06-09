using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavel2.Framework {
    public abstract class Parser {

        public abstract String Name { get; }
        protected StreamReader stream;

        private List<List<IPoint>> points;
        private List<String> headers;
        private List<double> min, max;

        public Column[] Parse(StreamReader stream) {
            this.stream = stream;
            points = new List<List<IPoint>>();
            headers = new List<string>();
            min = new List<double>();
            max = new List<double>();
            try {
                ParseAlgorithm();
                Column[] columns = new Column[points.Count];
                for (int i = 0; i < columns.Length; i++) {
                    if (columns[i] == null) columns[i] = new Column();
                    if (headers.Count > i) {
                        columns[i].Header = headers[i];
                    } else {
                        columns[i].Header = "Column"+i.ToString();
                    }
                    columns[i].Points = points[i].ToArray();
                    columns[i].Min = min[i];
                    columns[i].Max = max[i];
                }
                return columns;
            } catch {
                return null;
            }
        }

        protected void AddPoint(IPoint point, int col) {
            if (points.Count <= col) {
                for (int i = points.Count-1; i < col; i++) {
                    points.Add(new List<IPoint>());
                    min.Add(double.PositiveInfinity);
                    max.Add(double.NegativeInfinity);
                }
            }
            points[col].Add(point);
            if (point.DoubleData != double.NaN) {
                if (point.DoubleData < min[col]) min[col] = point.DoubleData;
                if (point.DoubleData > max[col]) max[col] = point.DoubleData;
            }
        }

        protected void AddHeader(String header, int col) {
            if (headers.Count <= col) {
                for (int i = headers.Count-1; i < col; i++) {
                    headers.Add("");
                }
            }
            headers[col] = header;
        }

        protected abstract void ParseAlgorithm();

    }
}
