using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EYLocationSyncUtility.BEL
{
    /// <summary>
    /// User Location 
    /// </summary>
    public class UserLocation
    {
        #region Public properties

        // First Name
        public string FirstName { get; set; }

        //Preferred Name
        public string PreferredName { get; set; }

        //Middel Name
        public string MiddleName { get; set; }

        // Last Name
        public string LastName { get; set; }

        //GUI
        public string GUI { get; set; }

        //GPN
        public string GPN { get; set; }

        //Network Login Name
        public string NetworkLogin { get; set; }

        //Active From Date
        public string FromDate { get; set; }

        //Active To Date
        public string ToDate { get; set; }

        //Email Address of type Internet
        public string SMTPEmail { get; set; }

        //Domain Name
        public string DomainName { get; set; }

        //Service Line
        public string ServiceLine { get; set; }

        //Sub Service Line
        public string SubServiceLine { get; set; }

        //Location
        public string Location { get; set; }
        #endregion
    }
}
