using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EYLocationSyncUtility.BEL
{
    public class ClientRecord : BaseRecord
    {
        public string ClientID { get; set; }
        public string Client { get; set; }
        public string Partner1 { get; set; }
        public string Partner2 { get; set; }
        public string InactiveDate { get; set; }
        public string ClientStatus { get; set; }

    }
}
