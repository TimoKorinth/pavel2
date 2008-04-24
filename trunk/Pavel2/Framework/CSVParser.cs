using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavel2.Framework {
    public class CSVParser : Parser {

        StreamReader stream;
        Char delimiter = ';';
        List<IPoint>[] data;

        protected override Column[] ParseAlgorithm(StreamReader stream) {
            this.stream = stream;
            String line;
            String[] lineSplit;
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
            for (int i = 0; i < columns.Length; i++) {
                columns[i] = new Column();
                columns[i].Points = data[i].ToArray();
            }
            return columns;
        }

    }
}
