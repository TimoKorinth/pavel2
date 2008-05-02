using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavel2.Framework {
    public class CSVParser : Parser {

        StreamReader stream;
        Char delimiter = ';';
        Boolean hasHeaders = false;
        List<IPoint>[] data;

        public override string Name {
            get { return "CSV Parser"; }
        }

        public Boolean HasHeaders {
            get { return hasHeaders; }
            set { hasHeaders = value; }
        }

        public Char Delimiter {
            get { return delimiter; }
            set { delimiter = value; }
        }

        protected override Column[] ParseAlgorithm(StreamReader stream) {
            this.stream = stream;
            String line;
            String[] lineSplit;
            this.data = null;
            while ((line = stream.ReadLine()) != null) {
                lineSplit = line.Split(delimiter);
                if (null == data) {
                    data = new List<IPoint>[lineSplit.Length];
                }
                for (int i = 0; i < lineSplit.Length; i++) {
                    if (null == data[i]) {
                        data[i] = new List<IPoint>();
                    }
                    data[i].Add(new DiscretePoint(lineSplit[i]));
                }

            }
            Column[] columns = new Column[data.Length];
            IPoint[] pointArray;
            for (int i = 0; i < columns.Length; i++) {
                if (hasHeaders) {
                    columns[i] = new Column(data[i][0].Data);
                    data[i].RemoveAt(0);
                    pointArray = data[i].ToArray();
                    columns[i].Points = pointArray;
                } else {
                    columns[i] = new Column();
                    pointArray = data[i].ToArray();
                    columns[i].Points = pointArray;
                }
            }
            return columns;
        }

    }
}
