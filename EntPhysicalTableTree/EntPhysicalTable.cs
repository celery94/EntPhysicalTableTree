using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EntPhysicalTableTree
{
    public class EntPhysicalTable
    {
        public string entPhysicalIndex { get; set; }
        public string entPhysicalDescr { get; set; }
        public string entPhysicalVendorType { get; set; }
        public string entPhysicalContainedIn { get; set; }
        public string entPhysicalClass { get; set; }
        public string entPhysicalParentRelPos { get; set; }
        public string entPhysicalName { get; set; }
        public string entPhysicalHardwareRev { get; set; }
        public string entPhysicalFirmwareRev { get; set; }
        public string entPhysicalSoftwareRev { get; set; }
        public string entPhysicalSerialNum { get; set; }
        public string entPhysicalMfgName { get; set; }
        public string entPhysicalModelName { get; set; }
        public string entPhysicalAlias { get; set; }
        public string entPhysicalAssetID { get; set; }
        public string entPhysicalIsFRU { get; set; }
        public string entPhysicalMfgDate { get; set; }
        public string entPhysicalUris { get; set; }
        public string IndexValue { get; set; }

        public TreeNode TreeNode
        {
            get
            {
                TreeNode node = new TreeNode();
                node.Text = string.Format("({2}){0}-{1}", entPhysicalIndex, entPhysicalDescr, entPhysicalClass);
                node.Tag = this;

                return node;
            }
        }

        public Dictionary<string, string> AllProperties
        {
            get
            {
                return this.GetType().GetProperties().Where(q => q.Name != "TreeNode" && q.Name != "AllProperties")
                    .ToDictionary(q => q.Name, q => q.GetValue(this, null).ToString());
            }
        }
    }
}
