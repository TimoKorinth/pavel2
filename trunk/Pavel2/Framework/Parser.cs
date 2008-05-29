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

        public Column[] Parse(StreamReader stream) {
            this.stream = stream;
            points = new List<List<IPoint>>();
            headers = new List<string>();
            try {
                ParseAlgorithm();
                Column[] columns = new Column[points.Count];
                for (int i = 0; i < columns.Length; i++) {
                    if (columns[i] == null) columns[i] = new Column();
                    if (headers.Count > i) {
                        columns[i].Header = headers[i];
                    } else {
                        columns[i].Header = "";
                    }
                    columns[i].Points = points[i].ToArray();
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
                }
            }
            points[col].Add(point);
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
