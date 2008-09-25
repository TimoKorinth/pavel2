using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pavel2.Framework {
    [Serializable()]
    public class LinkItem {

        private List<DataProjectTreeItem> dataItems;
        private List<ImageTreeItem> images;
        private String header;
        private bool isCombined = false;
        private CombinedDataItem combItem;

        public CombinedDataItem CombItem {
            get { return combItem; }
            set { combItem = value; }
        }

        public bool IsCombined {
            get { return isCombined; }
            set { isCombined = value; }
        }

        public bool IsCombineable {
            get {
                int cols = -1;
                foreach (DataProjectTreeItem item in dataItems) {
                    if (cols == -1) cols = item.DataGrid.Columns.Length;
                    if (cols != item.DataGrid.Columns.Length) {
                        return false;
                    }
                }
                return true;
            }
        }

        public String Header {
            get { return header; }
            set { header = value; }
        }

        public List<DataProjectTreeItem> DataItems {
            get { return dataItems; }
            set { dataItems = value; }
        }

        public List<ImageTreeItem> Images {
            get { return images; }
            set { images = value; }
        }

        public LinkItem() {
            dataItems = new List<DataProjectTreeItem>();
            images = new List<ImageTreeItem>();
            combItem = new CombinedDataItem(dataItems);
        }

        public void AddDataItem(DataProjectTreeItem dataItem) {
            this.dataItems.Add(dataItem);
            combItem = new CombinedDataItem(dataItems);
        }

        public void RemoveDataItem(DataProjectTreeItem dataItem) {
            this.dataItems.Remove(dataItem);
            combItem = new CombinedDataItem(dataItems);
        }

        public void AddImage(ImageTreeItem img) {
            this.images.Add(img);
        }

        public void RemoveImage(ImageTreeItem img) {
            this.images.Remove(img);
        }

    }
}
