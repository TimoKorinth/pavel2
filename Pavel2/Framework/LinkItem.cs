using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    public class LinkItem {

        private List<DataProjectTreeItem> dataItems;
        private String header;

        public String Header {
            get { return header; }
            set { header = value; }
        }

        public List<DataProjectTreeItem> DataItems {
            get { return dataItems; }
            set { dataItems = value; }
        }

        public LinkItem() {
            dataItems = new List<DataProjectTreeItem>();
        }

        public void AddDataItem(DataProjectTreeItem dataItem) {
            this.dataItems.Add(dataItem);
        }

        public void RemoveDataItem(DataProjectTreeItem dataItem) {
            this.dataItems.Remove(dataItem);
        }

    }
}
