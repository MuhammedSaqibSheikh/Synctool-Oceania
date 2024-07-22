using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EYLocationSyncUtility.BEL
{
    public class PartnerRecord : BaseRecord
    {
        public string Client { get; set; }
        public string PrimaryName { get; set; }
        public string Gender { get; set; }
        public string ServiceLine { get; set; }
        public string SubServiceLine { get; set; }
        public string OriginalHireDate { get; set; }
        public string TerminationDate { get; set; }
        public string GPN { get; set; }
        public string LPN { get; set; }
        public string EmplID { get; set; }
        public string GUI { get; set; }
        public string PayGroup { get; set; }
        public string CodeBlock { get; set; }
        public string CostCentre { get; set; }
        public string CompanyCode { get; set; }
        public string BusinessUnit { get; set; }
        public string OrganisationalUnit { get; set; }
        public string ManagementUnit { get; set; }
        public string SubManagementUnit { get; set; }
        public string HRStatus { get; set; }
    }
}
