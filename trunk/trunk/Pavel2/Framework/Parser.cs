using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavel2.Framework {
    public abstract class Parser {

        public Column[] Parse(StreamReader stream) {
            Column[] columns;
            try {
                columns = ParseAlgorithm(stream);
            } catch (Exception e) {
                e.GetType();
                return null;
            }
            return columns;
        }

        protected abstract Column[] ParseAlgorithm(StreamReader stream);

    }
}
