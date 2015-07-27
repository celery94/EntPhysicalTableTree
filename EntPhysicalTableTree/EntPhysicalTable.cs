using System.Collections.Generic;
using System.Linq;
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
                TreeNode node = new TreeNode
                {
                    Text = $"{entPhysicalIndex}({(ClassType)int.Parse(entPhysicalClass)})--{entPhysicalName}",
                    Tag = this
                };

                return node;
            }
        }

        public Dictionary<string, string> AllProperties
        {
            get
            {
                return GetType().GetProperties()
                    .Where(q => q.Name != "TreeNode" && q.Name != "AllProperties")
                    .ToDictionary(q => q.Name, q => q.GetValue(this, null)?.ToString());
            }
        }

        public static string OID => "1.3.6.1.2.1.47.1.1.1";
    }

    public enum ClassType
    {
        Chassis = 3,
        Backplane = 4,
        Container = 5,
        PowerSupply = 6,
        Fan = 7,
        Sensor = 8,
        Module = 9,
        Port = 10,
        Stack = 11
    }
}