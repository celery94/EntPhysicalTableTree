﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EntPhysicalTableTree
{
    public partial class FormMain : Form
    {
        private List<EntPhysicalTable> _list;

        public FormMain()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _list = new List<EntPhysicalTable>();

            FormIPDialog dialog = new FormIPDialog();

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _list = SNMPUtil.GetTable<EntPhysicalTable>(dialog.IpAddress, EntPhysicalTable.OID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                treeView.Nodes.Clear();
                treeView.Nodes.Add(GetTreeNode());
                treeView.NodeMouseClick += TreeView_NodeMouseClick;
            }
        }

        private void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var msg = (e.Node.Tag as EntPhysicalTable)?.AllProperties;

            dgvProps.DataSource = msg?.ToArray();
            dgvProps.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private TreeNode GetTreeNode()
        {
            var root = _list.FirstOrDefault(q => q.entPhysicalContainedIn == "0")?.TreeNode;

            AppendChildNodes(root);

            return root;
        }

        private void AppendChildNodes(TreeNode node)
        {
            EntPhysicalTable item = node.Tag as EntPhysicalTable;

            var nodes = _list.Where(q => q.entPhysicalContainedIn == item?.entPhysicalIndex.ToString() && q.entPhysicalClass != ((int)ClassType.Sensor).ToString()).ToList();

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