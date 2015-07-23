using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EntPhysicalTableTree
{
    public partial class frmIPDialog : Form
    {
        public frmIPDialog()
        {
            InitializeComponent();
        }

        public string IpAddress
        {
            get { return txtIpAddress.Text; }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
