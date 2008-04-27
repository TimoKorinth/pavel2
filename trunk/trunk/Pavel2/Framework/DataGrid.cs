using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class DataGrid {

        private Column[] columns;
        private String name;
        private Parser parser;

        public Parser Parser {
            get { return parser; }
            set { parser = value; }
        }

        public String Name {
            get { return name; }
            set { name = value; }
        }

        public Column[] Columns {
            get { return columns; }
            set { columns = value; }
        }

        public DataGrid(Column[] columns, String name) {
            this.columns = columns;
            this.name = name;
        }

    }
}
