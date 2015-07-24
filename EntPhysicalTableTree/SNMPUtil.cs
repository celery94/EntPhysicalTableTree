using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using SnmpSharpNet;

namespace EntPhysicalTableTree
{
    public class SNMPUtil
    {
        private static DataTable ConvertToDataTable(List<uint> tableColumns, Dictionary<string, Dictionary<uint, AsnType>> result)
        {
            DataTable dt = new DataTable();

            foreach (uint column in tableColumns)
            {
                dt.Columns.Add(column.ToString());
            }

            foreach (KeyValuePair<string, Dictionary<uint, AsnType>> kvp in result)
            {
                List<string> rowData = new List<string>();

                foreach (uint column in tableColumns)
                {
                    if (kvp.Value.ContainsKey(column))
                    {
                        rowData.Add(kvp.Value[column].ToString());
                    }
                    else
                    {
                        rowData.Add("");
                    }
                }

                dt.Rows.Add(rowData.ToArray());
            }
            return dt;
        }

        public static List<EntPhysicalTable> GetEntPhysicalTable(string ip)
        {
            string oid = "1.3.6.1.2.1.47.1.1.1";
            List<uint> tableColumns;

            var result = GetTableValue(ip, oid, out tableColumns);

            if (result.Count <= 0)
            {
                throw new Exception("No results returned.\n");
            }
            List<EntPhysicalTable> list = new List<EntPhysicalTable>();
            foreach (KeyValuePair<string, Dictionary<uint, AsnType>> kvp in result)
            {
                List<string> rowData = new List<string>();

                foreach (uint column in tableColumns)
                {
                    if (kvp.Value.ContainsKey(column))
                    {
                        rowData.Add(kvp.Value[column].ToString());
                    }
                    else
                    {
                        rowData.Add("");
                    }
                }

                EntPhysicalTable table = new EntPhysicalTable
                {
                    entPhysicalIndex = kvp.Key,
                    entPhysicalDescr = rowData.ElementAtOrDefault(0),
                    entPhysicalVendorType = rowData.ElementAtOrDefault(1),
                    entPhysicalContainedIn = rowData.ElementAtOrDefault(2),
                    entPhysicalClass = (ClassType)int.Parse(rowData.ElementAtOrDefault(3)),
                    entPhysicalParentRelPos = rowData.ElementAtOrDefault(4),
                    entPhysicalName = rowData.ElementAtOrDefault(5),
                    entPhysicalHardwareRev = rowData.ElementAtOrDefault(6),
                    entPhysicalFirmwareRev = rowData.ElementAtOrDefault(7),
                    entPhysicalSoftwareRev = rowData.ElementAtOrDefault(8),
                    entPhysicalSerialNum = rowData.ElementAtOrDefault(9),
                    entPhysicalMfgName = rowData.ElementAtOrDefault(10),
                    entPhysicalModelName = rowData.ElementAtOrDefault(11),
                    entPhysicalAlias = rowData.ElementAtOrDefault(12),
                    entPhysicalAssetID = rowData.ElementAtOrDefault(13),
                    entPhysicalIsFRU = rowData.ElementAtOrDefault(14),
                    entPhysicalMfgDate = rowData.ElementAtOrDefault(15),
                    entPhysicalUris = rowData.ElementAtOrDefault(16)
                    //IndexValue = rowData.ElementAt(18),
                };

                list.Add(table);
            }

            return list;
        }

        private static Dictionary<string, Dictionary<uint, AsnType>> GetTableValue(string ip, string oid, out List<uint> tableColumns)
        {
            Dictionary<string, Dictionary<uint, AsnType>> result = new Dictionary<string, Dictionary<uint, AsnType>>();
            // Not every row has a value for every column so keep track of all columns available in the table
            tableColumns = new List<uint>();
            // Prepare agent information
            AgentParameters param = new AgentParameters(SnmpVersion.Ver2, new OctetString("public"));
            IpAddress peer = new IpAddress(ip);
            if (!peer.Valid)
            {
                Console.WriteLine("Unable to resolve name or error in address for peer: {0}", "");
                return result;
            }
            UdpTarget target = new UdpTarget((IPAddress)peer);
            // This is the table OID supplied on the command line
            Oid startOid = new Oid(oid);
            // Each table OID is followed by .1 for the entry OID. Add it to the table OID
            startOid.Add(1); // Add Entry OID to the end of the table OID
            // Prepare the request PDU
            Pdu bulkPdu = Pdu.GetBulkPdu();
            bulkPdu.VbList.Add(startOid);
            // We don't need any NonRepeaters
            bulkPdu.NonRepeaters = 0;
            // Tune MaxRepetitions to the number best suited to retrive the data
            bulkPdu.MaxRepetitions = 100;
            // Current OID will keep track of the last retrieved OID and be used as 
            //  indication that we have reached end of table
            Oid curOid = (Oid)startOid.Clone();
            // Keep looping through results until end of table
            while (startOid.IsRootOf(curOid))
            {
                SnmpPacket res = null;
                try
                {
                    res = target.Request(bulkPdu, param);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Request failed: {0}", ex.Message);
                    target.Close();
                    return result;
                }
                // For GetBulk request response has to be version 2
                if (res.Version != SnmpVersion.Ver2)
                {
                    Console.WriteLine("Received wrong SNMP version response packet.");
                    target.Close();
                    return result;
                }
                // Check if there is an agent error returned in the reply
                if (res.Pdu.ErrorStatus != 0)
                {
                    Console.WriteLine("SNMP agent returned error {0} for request Vb index {1}",
                        res.Pdu.ErrorStatus, res.Pdu.ErrorIndex);
                    target.Close();
                    return result;
                }
                // Go through the VbList and check all replies
                foreach (Vb v in res.Pdu.VbList)
                {
                    curOid = (Oid)v.Oid.Clone();
                    // VbList could contain items that are past the end of the requested table.
                    // Make sure we are dealing with an OID that is part of the table
                    if (startOid.IsRootOf(v.Oid))
                    {
                        // Get child Id's from the OID (past the table.entry sequence)
                        uint[] childOids = Oid.GetChildIdentifiers(startOid, v.Oid);
                        // Get the value instance and converted it to a dotted decimal
                        //  string to use as key in result dictionary
                        uint[] instance = new uint[childOids.Length - 1];
                        Array.Copy(childOids, 1, instance, 0, childOids.Length - 1);
                        string strInst = InstanceToString(instance);
                        // Column id is the first value past <table oid>.entry in the response OID
                        uint column = childOids[0];
                        if (!tableColumns.Contains(column))
                            tableColumns.Add(column);
                        if (result.ContainsKey(strInst))
                        {
                            result[strInst][column] = (AsnType)v.Value.Clone();
                        }
                        else
                        {
                            result[strInst] = new Dictionary<uint, AsnType>();
                            result[strInst][column] = (AsnType)v.Value.Clone();
                        }
                    }
                    else
                    {
                        // We've reached the end of the table. No point continuing the loop
                        break;
                    }
                }
                // If last received OID is within the table, build next request
                if (startOid.IsRootOf(curOid))
                {
                    bulkPdu.VbList.Clear();
                    bulkPdu.VbList.Add(curOid);
                    bulkPdu.NonRepeaters = 0;
                    bulkPdu.MaxRepetitions = 100;
                }
            }
            target.Close();
            return result;
        }

        private static string InstanceToString(uint[] instance)
        {
            StringBuilder str = new StringBuilder();
            foreach (uint v in instance)
            {
                if (str.Length == 0)
                    str.Append(v);
                else
                    str.AppendFormat(".{0}", v);
            }
            return str.ToString();
        }
    }
}