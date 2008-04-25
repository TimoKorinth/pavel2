using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavel2.Framework {
    public static class ParserManagement {

        private static List<Parser> parserList = new List<Parser>();
        private static Parser currentParser;

        public static Parser CurrentParser {
            get { return currentParser; }
        }

        public static DataGrid GetDataGrid(StreamReader stream) {
            UpdateParserList();
            currentParser = null;
            Column[] columns;
            foreach (Parser parser in parserList) {
                columns = parser.Parse(stream);
                if (null != columns) {
                    currentParser = parser;
                    return MainData.AddColumns(columns);
                }
            }
            return null;
        }

        //TODO: Automatisch alle die Parser implementieren holen; Reihenfolge nach Priorität
        private static void UpdateParserList() { 
            parserList.Clear();
            parserList.Add(new CSVParser());
        }

    }
}
