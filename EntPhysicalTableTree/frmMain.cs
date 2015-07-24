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

            frmIPDialog dialog = new frmIPDialog();

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _list = SNMPUtil.GetEntPhysicalTable(dialog.IpAddress);

                treeView.Nodes.Clear();
                treeView.Nodes.Add(GetTreeNode());
                treeView.NodeMouseClick += TreeView_NodeMouseClick;
            }
        }

        private void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var msg = (e.Node.Tag as EntPhysicalTable).AllProperties;

            dataGridView1.DataSource = msg.ToArray();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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

            var nodes = _list.Where(q => q.entPhysicalContainedIn == table.entPhysicalIndex.ToString() && q.entPhysicalClass != ClassType.Sensor).ToList();

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
