using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavel2.Framework {
    public static class ParserManagement {

        private static List<Parser> parserList = new List<Parser>();
        private static Parser currentParser;
        private static FileInfo file;

        public static FileInfo File {
            get { return ParserManagement.file; }
        }

        public static List<Parser> ParserList {
            get { return ParserManagement.parserList; }
        }

        public static Parser CurrentParser {
            get { return currentParser; }
        }

        public static DataGrid GetDataGrid(FileInfo file) {
            ParserManagement.file = file;
            UpdateParserList();
            currentParser = null;
            Column[] columns;
            foreach (Parser parser in parserList) {
                try {
                    StreamReader st = new StreamReader(ParserManagement.file.OpenRead(), System.Text.Encoding.Default);
                    columns = parser.Parse(st);
                    if (null != columns) {
                        currentParser = parser;
                        DataGrid d = MainData.AddColumns(columns);
                        return d;
                    }
                } catch { }
            }
            return null;
        }

        public static DataGrid GetDataGrid(Parser parser, FileInfo file) {
            ParserManagement.file = file;
            currentParser = parser;
            Column[] columns;
            StreamReader st = new StreamReader(ParserManagement.file.OpenRead(), System.Text.Encoding.Default);
            columns = parser.Parse(st);
            if (null != columns) {
                DataGrid d = MainData.AddColumns(columns);
                return d;
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
