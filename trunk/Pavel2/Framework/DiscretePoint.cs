using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    class DiscretePoint : IPoint {

        String data;

        public string Data {
            get {
                return data;
            }
            set {
                data = value;
            }
        }

        public DiscretePoint(String data) {
            this.data = data;
        }

    }
}
