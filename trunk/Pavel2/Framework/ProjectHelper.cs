using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;

namespace Pavel2.Framework {
    public static class ProjectHelper {

        private static SerializeObject serRoot;

        public static void SaveProject(String file) {
            Stream stream = File.Open(file, FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();

            serRoot = new SerializeObject();
            FillSerTree(serRoot, MainData.MainWindow.projectTreeView.root);

            bformatter.Serialize(stream, serRoot);
            stream.Close();
        }

        private static void FillSerTree(SerializeObject ser, TreeViewItem item) {
            if (!(item.Tag is ProjectTreeItem) && !(item.Tag is LinkItem)) return;
            if (ser.Items == null) ser.Items = new List<SerializeObject>();
            ser.Item = item.Tag;
            if (item.Tag is LinkItem) return;
            foreach (TreeViewItem tvItem in item.Items) {
                if (!(tvItem.Tag is ProjectTreeItem) && !(tvItem.Tag is LinkItem)) continue;
                SerializeObject serTmp = new SerializeObject();
                serTmp.Item = tvItem.Tag;
                ser.Items.Add(serTmp);
                FillSerTree(serTmp, tvItem);
            }
        }

        private static void RecoverTree(SerializeObject ser, TreeViewItem item) {
            item.Tag = ser.Item;
            if (ser.Item is FolderProjectTreeItem) {
                FolderProjectTreeItem fPTI = ser.Item as FolderProjectTreeItem;
                fPTI.ParentItem = item;
                item.Tag = fPTI;
            }
            foreach (SerializeObject serItem in ser.Items) {
                TreeViewItem tvItem = new TreeViewItem();
                tvItem.Tag = serItem.Item;
                if (serItem.Item is FolderProjectTreeItem) {
                    FolderProjectTreeItem fPTI = serItem.Item as FolderProjectTreeItem;
                    fPTI.ParentItem = tvItem;
                    tvItem.Tag = fPTI;
                }
                item.Items.Add(tvItem);
                RecoverTree(serItem, tvItem);
            }
            if (item.Tag is DataProjectTreeItem) MainData.MainWindow.projectTreeView.UpdateDataTreeViewItem(item);
            if (item.Tag is LinkItem) MainData.MainWindow.projectTreeView.UpdateLinkItem(item);
        }

        public static void OpenProject(String file) {
            serRoot = null;

            Stream stream = File.Open(file, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();

            serRoot = (SerializeObject)bformatter.Deserialize(stream);
            MainData.MainWindow.projectTreeView.root.Items.Clear();
            RecoverTree(serRoot, MainData.MainWindow.projectTreeView.root);
            stream.Close();
        }

    }
}
