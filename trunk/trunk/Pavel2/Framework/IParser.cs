using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavel2.Framework {
    public interface IParser {

        StreamReader File {
            get;
            set;
        }

        void Parse();

    }
}
