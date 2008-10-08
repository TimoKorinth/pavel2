using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Globalization;

namespace Pavel2.Framework {
    public class CSVParser : Parser {

        Char delimiter = ';';
        Boolean hasHeaders = false;

        [Browsable(false)]
        public override string Name {
            get { return "CSV Parser"; }
        }

        [Description("Has Headers")]
        public Boolean HasHeaders {
            get { return hasHeaders; }
            set { hasHeaders = value; }
        }

        public Char Delimiter {
            get { return delimiter; }
            set { delimiter = value; }
        }

        protected override void ParseAlgorithm() {
            String line;
            String[] lineSplit;
            Boolean firstLine = true;
            CultureInfo ci = CultureInfo.InstalledUICulture;
            while ((line = stream.ReadLine()) != null) {
                lineSplit = line.Split(delimiter);
                for (int i = 0; i < lineSplit.Length; i++) {
                    if (firstLine && hasHeaders) {
                        AddHeader(lineSplit[i], i);
                    } else {
                        double d;
                        if (double.TryParse(lineSplit[i], NumberStyles.Float | NumberStyles.AllowThousands, ci, out d)) {
                            AddPoint(new DiscretePoint(lineSplit[i], d), i);
                        } else {
                            AddPoint(new DiscretePoint(lineSplit[i]), i);
                        }
                    }
                }
                firstLine = false;
            }
        }

    }
}
