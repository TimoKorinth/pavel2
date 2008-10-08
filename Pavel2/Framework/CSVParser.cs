using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Globalization;

namespace Pavel2.Framework {
    public class CSVParser : Parser {

        Boolean hasHeaders = false;
        public enum DecimalChar { Comma, Point };
        public enum DelimiterChar { Comma = (int)',', Point = (int)'.', Semicolon = (int)';', Tabular = (int)'\t', Space = (int)' ' };
        private DecimalChar currentDec = DecimalChar.Comma;
        private DelimiterChar currentDel = DelimiterChar.Semicolon;

        [Description("Delimiter")]
        public DelimiterChar CurrentDel {
            get { return currentDel; }
            set { currentDel = value; }
        }

        [Description("Decimal Character")]
        public DecimalChar CurrentDec {
            get { return currentDec; }
            set { currentDec = value; }
        }

        [Browsable(false)]
        public override string Name {
            get { return "CSV Parser"; }
        }

        [Description("Has Headers")]
        public Boolean HasHeaders {
            get { return hasHeaders; }
            set { hasHeaders = value; }
        }

        protected override void ParseAlgorithm() {
            String line;
            String[] lineSplit;
            Boolean firstLine = true;
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (currentDec == DecimalChar.Comma) {
                ci = new CultureInfo("de-DE", false);
            } else if (currentDec == DecimalChar.Point) {
                ci = new CultureInfo("en-US", false);
            }
            while ((line = stream.ReadLine()) != null) {
                lineSplit = line.Split((char)currentDel);
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
