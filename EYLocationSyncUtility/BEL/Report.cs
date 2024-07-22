using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace EYLocationSyncUtility.BEL
{
    /// <summary>
    /// Consolidated Reports
    /// </summary>
    public class Report
    {
        #region Public properties
        //Counter for Client location created
        public static int clientCreated { get; set; } 

        //Counter for Client location updated
        public static int clientUpdated { get; set; }

        //Total Client details fetched from database for create
        public static int totalClientCreate { get; set; }

        //Total Client details fetched from database for update
        public static int totalClientUpdate { get; set; }

        //Counter for Client location not created
        public static int clientNotCreated { get; set; }

        //Counter for Client location not updated
        public static int clientNotUpdated { get; set; }

        //Counter for Client create IsUpdatedInRM failed 
        public static int clientIsUpdateRMCreate { get; set; }

        //Counter for Client update IsUpdatedInRM failed 
        public static int clientIsUpdateRMUpdate { get; set; }

        //Counter for duplicate Client found
        public static int duplicateClient { get; set; }

        //Flage to indicate Client Status table updation failure
        [DefaultValue(true)]
        public static bool IsClientStatusTableUpdated { get; set; }

        //Flage to indicate Client Creation process completed successfully
        [DefaultValue(true)]
        public static bool IsClientCreated { get; set; }

        //Flage to indicate Client Update process completed successfully
        [DefaultValue(true)]
        public static bool IsClientUpdated { get; set; }


        //Counter for User location created
        public static int userCreated { get; set; }

        //Counter for User location updated
        public static int userUpdated { get; set; }

        //Total User details fetched from database for create
        public static int totalUserCreate { get; set; }

        //Total User details fetched from database for update
        public static int totalUserUpdate { get; set; }

        //Counter for User location not created
        public static int userNotCreated { get; set; }

        //Counter for User location not updated
        public static int userNotUpdated { get; set; }

        //Counter for duplicate User found
        public static int duplicateUser { get; set; }     

        //Counter for User create IsUpdatedInRM failed 
        public static int userIsUpdateRMCreate { get; set; }

        //Counter for User update IsUpdatedInRM failed 
        public static int userIsUpdateRMUpdate { get; set; }

        //Flage to indicate User Status table updation failure
        [DefaultValue(true)]
        public static bool IsUserStatusTableUpdated { get; set; }

        //Flage to indicate User Creation process completed successfully
        [DefaultValue(true)]
        public static bool IsUserCreated { get; set; }

        //Flage to indicate User Update process completed successfully
        [DefaultValue(true)]
        public static bool IsUserUpdated { get; set; }

       
        
        //Counter for Employee Record created
        public static int EmployeeRecordCreated { get; set; }

        //Counter for Employee Record updated
        public static int EmployeeRecordUpdated { get; set; }

        //Total Employee Record details fetched from database for create
        public static int TotalEmployeeRecordCreate { get; set; }

        //Total Employee Record details fetched from database for update
        public static int TotalEmployeeRecordUpdate { get; set; }

        //Counter for Employee Record not created
        public static int EmployeeRecordNotCreated { get; set; }

        //Counter for Employee Record not updated
        public static int EmployeeRecordNotUpdated { get; set; }

        //Counter for duplicate Employee Record found
        public static int DuplicateEmployeeRecord { get; set; }

        //Counter for Employee Record create IsUpdatedInRM failed 
        public static int EmployeeRecordIsUpdateRMCreate { get; set; }

        //Counter for Employee Record update IsUpdatedInRM failed 
        public static int EmployeeRecordIsUpdateRMUpdate { get; set; }

        //Flage to indicate Employee Record Status table updation failure
        [DefaultValue(true)]
        public static bool IsEmployeeRecordStatusTableUpdated { get; set; }

        //Flage to indicate Employee Record Creation process completed successfully
        [DefaultValue(true)]
        public static bool IsEmployeeRecordCreated { get; set; }

        //Flage to indicate Employee Record Update process completed successfully
        [DefaultValue(true)]
        public static bool IsEmployeeRecordUpdated { get; set; }


        //Counter for Partner Record created
        public static int PartnerRecordCreated { get; set; }

        //Counter for Partner Record updated
        public static int PartnerRecordUpdated { get; set; }

        //Total Partner Record details fetched from database for create
        public static int TotalPartnerRecordCreate { get; set; }

        //Total Partner Record details fetched from database for update
        public static int TotalPartnerRecordUpdate { get; set; }

        //Counter for Partner Record not created
        public static int PartnerRecordNotCreated { get; set; }

        //Counter for Partner Record not updated
        public static int PartnerRecordNotUpdated { get; set; }

        //Counter for duplicate Partner Record found
        public static int DuplicatePartnerRecord { get; set; }

        //Counter for Partner Record create IsUpdatedInRM failed 
        public static int PartnerRecordIsUpdateRMCreate { get; set; }

        //Counter for Partner Record update IsUpdatedInRM failed 
        public static int PartnerRecordIsUpdateRMUpdate { get; set; }

        //Flage to indicate Partner Record Status table updation failure
        [DefaultValue(true)]
        public static bool IsPartnerRecordStatusTableUpdated { get; set; }

        //Flage to indicate Partner Record Creation process completed successfully
        [DefaultValue(true)]
        public static bool IsPartnerRecordCreated { get; set; }

        //Flage to indicate Partner Record Update process completed successfully
        [DefaultValue(true)]
        public static bool IsPartnerRecordUpdated { get; set; }


        //Counter for Client Record created
        public static int ClientRecordCreated { get; set; }

        //Counter for Client Record updated
        public static int ClientRecordUpdated { get; set; }

        //Total Client Record details fetched from database for create
        public static int TotalClientRecordCreate { get; set; }

        //Total Client Record details fetched from database for update
        public static int TotalClientRecordUpdate { get; set; }

        //Counter for Client Record not created
        public static int ClientRecordNotCreated { get; set; }

        //Counter for Client Record not updated
        public static int ClientRecordNotUpdated { get; set; }

        //Counter for duplicate Client Record found
        public static int DuplicateClientRecord { get; set; }

        //Counter for Client Record create IsUpdatedInRM failed 
        public static int ClientRecordIsUpdateRMCreate { get; set; }

        //Counter for Client Record update IsUpdatedInRM failed 
        public static int ClientRecordIsUpdateRMUpdate { get; set; }

        //Flage to indicate Client Record Status table updation failure
        [DefaultValue(true)]
        public static bool IsClientRecordStatusTableUpdated { get; set; }

        //Flage to indicate Client Record Creation process completed successfully
        [DefaultValue(true)]
        public static bool IsClientRecordCreated { get; set; }

        //Flage to indicate Client Record Update process completed successfully
        [DefaultValue(true)]
        public static bool IsClientRecordUpdated { get; set; }
        #endregion
    }
}
