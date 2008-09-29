using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    [Serializable()]
    class DiscretePoint : IPoint {

        public DiscretePoint(String data) {
            this.data = data;
        }

        public DiscretePoint(String data, double dData) {
            this.data = data;
            this.doubleData = dData;
        }

    }
}
