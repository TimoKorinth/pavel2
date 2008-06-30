using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Pavel2.Framework {
    [Serializable()]
    public class SerializeObject {

        ProjectTreeItem item;

        public ProjectTreeItem Item {
            get { return item; }
            set { item = value; }
        }

        List<SerializeObject> items;

        public List<SerializeObject> Items {
            get { return items; }
            set { items = value; }
        }

    }
}
