using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EntPhysicalTableTree
{
    public partial class frmMain : System.Windows.Forms.Form
    {
        private List<EntPhysicalTable> _list;

        public frmMain()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _list = new List<EntPhysicalTable>();

            FileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = fileDialog.FileName;

                var allLines = File.ReadAllLines(filePath);

                foreach (var line in allLines)
                {
                    if (line.StartsWith("\"entPhysicalIndex\"")) continue;

                    var arr = line.Split(new[] { "\",\"" }, StringSplitOptions.None).Select(q => q.Trim('\"')).ToList();

                    EntPhysicalTable item = new EntPhysicalTable()
                    {
                        entPhysicalIndex = arr[0],
                        entPhysicalDescr = arr[1],
                        entPhysicalVendorType = arr[2],
                        entPhysicalContainedIn = arr[3],
                        entPhysicalClass = arr[4],
                        entPhysicalParentRelPos = arr[5],
                        entPhysicalName = arr[6],
                        entPhysicalHardwareRev = arr[7],
                        entPhysicalFirmwareRev = arr[8],
                        entPhysicalSoftwareRev = arr[9],
                        entPhysicalSerialNum = arr[10],
                        entPhysicalMfgName = arr[11],
                        entPhysicalModelName = arr[12],
                        entPhysicalAlias = arr[13],
                        entPhysicalAssetID = arr[14],
                        entPhysicalIsFRU = arr[15],
                        entPhysicalMfgDate = arr[16],
                        entPhysicalUris = arr[16],
                        IndexValue = arr[18],
                    };

                    _list.Add(item);
                }

                treeView.Nodes.Add(GetTreeNode());

                treeView.NodeMouseClick += TreeView_NodeMouseClick;
            }
        }

        private void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var msg = (e.Node.Tag as EntPhysicalTable).AllProperties;

            dataGridView1.DataSource = msg.ToArray();
        }

        private TreeNode GetTreeNode()
        {
            var root = _list.FirstOrDefault(q => q.entPhysicalContainedIn == "0").TreeNode;

            AppendChildNodes(root);

            return root;
        }

        private void AppendChildNodes(TreeNode node)
        {
            EntPhysicalTable table = node.Tag as EntPhysicalTable;

            var nodes = _list.Where(q => q.entPhysicalContainedIn == table.entPhysicalIndex.ToString() && q.entPhysicalClass!="sensor").ToList();

            node.Nodes.AddRange(nodes.Select(q => q.TreeNode).ToArray());

            foreach (var cnode in node.Nodes.Cast<TreeNode>())
            {
                AppendChildNodes(cnode);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
