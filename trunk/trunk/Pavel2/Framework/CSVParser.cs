using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavel2.Framework {
    public class CSVParser : IParser {

        StreamReader stream;
        Char delimiter = ',';
        Column[] columns;

        public Column[] Columns {
            get { return columns; }
            set { columns = value; }
        }
        List<DiscretePoint>[] tmp;

        public Char Delimiter {
            get { return delimiter; }
            set { delimiter = value; }
        }

        public StreamReader File {
            get {
                return stream;
            }
            set {
                stream = value;
            }
        }

        public void Parse() {
            int columnNumber = NumerOfColumns();
            columns = new Column[columnNumber];
            tmp = new List<DiscretePoint>[columnNumber];
            for (int i = 0; i < columnNumber; i++) {
                tmp[i] = new List<DiscretePoint>();
                columns[i] = new Column();
            }
            String line;
            String[] lineSplit;
            while ((line = stream.ReadLine()) != null) {
                lineSplit = line.Split(delimiter);
                for (int i = 0; i < columnNumber; i++) {
                    tmp[i].Add(new DiscretePoint(lineSplit[i]));
                }
            }
            for (int i = 0; i < tmp.Length; i++) {
                columns[i].Points = tmp[i].ToArray(); ;
            }
            MainData.AddColumns(columns);
        }

        //TODO: Erste Zeile damit schon weg!!!
        private int NumerOfColumns() {
            return stream.ReadLine().Split(delimiter).Length;
        }

    }
}
