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
            if (!(item.Tag is ProjectTreeItem)) return;
            if (ser.Items == null) ser.Items = new List<SerializeObject>();
            ser.Item = item.Tag as ProjectTreeItem;
            foreach (TreeViewItem tvItem in item.Items) {
                if (!(tvItem.Tag is ProjectTreeItem)) continue;
                SerializeObject serTmp = new SerializeObject();
                serTmp.Item = tvItem.Tag as ProjectTreeItem;
                ser.Items.Add(serTmp);
                FillSerTree(serTmp, tvItem);
            }
        }

        public static void OpenProject(String file) {
            serRoot = null;

            Stream stream = File.Open(file, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();

            serRoot = (SerializeObject)bformatter.Deserialize(stream);
            stream.Close();
        }

    }
}
