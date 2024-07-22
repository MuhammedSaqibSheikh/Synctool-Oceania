using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRIM.SDK;
using System.Configuration;
using EYLocationSyncUtility.BEL;
using log4net;
namespace EYLocationSyncUtility.DAL
{
    /// <summary>
    /// Provides methods to perform operations in HPE CM
    /// </summary>
    public class RMDataService
    {
        
        #region Private member

        // HPECM configuration
        private string workgroupServerName = ConfigurationManager.AppSettings["WorkgroupServerName"];
        private int workgroupServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["WorkgroupServerPort"]);
        private string databaseId = ConfigurationManager.AppSettings["DatabaseId"];

        // HPECM database
        private Database db = null;

        //Logger object
        private static ILog logger = null;
        

        #endregion
        #region Constructor

        /// <summary>
        /// Constructor method to connect to HPECM
        /// </summary>
        public RMDataService()
        {
            // Create a database connection
            db = new Database();
            db.WorkgroupServerName = workgroupServerName;
            //db.WorkgroupServerPort = workgroupServerPort;
            db.Id = databaseId;
            //Dev
            //db.AuthenticationMethod = ClientAuthenticationMechanism.ExplicitWindows;
            //db.SetAuthenticationCredentials("IMPSO\\cmadmin", "Password@123");
            db.Connect();

            if (!db.IsConnected)
            {
                throw new Exception(db.ErrorMessage);
            }

            logger = LogManager.GetLogger("Job");
            UtilityException.Message = string.Empty;
        }

        #endregion
        #region Public methods
        /// <summary>
        /// Call method to create or Update Client Location in HPE CM of type Organization
        /// </summary>
        /// <param name="">ClientLocation</param>
        /// <param name="">IsForUpdate</param>
        /// <returns></returns>
        public bool ClientOperation(ClientLocation Client,bool IsForUpdate)
        {
            return ClientLocationOperation(Client, IsForUpdate); 
        }
        /// <summary>
        /// Call method to create or Update User Location in HPE CM of type Person
        /// </summary>
        /// <param name="">UserLocation</param>
        /// <param name="">IsForUpdate</param>
        /// <returns></returns>
        public bool UserOperation(UserLocation User, bool IsForUpdate)
        {
            if (IsForUpdate)
                return UpdateUserLocationOperation(User);
            else
                return UserLocationOperation(User, IsForUpdate);
        }

        
        /// <summary>
        /// Call method to create or Update Client Records in HPE CM
        /// </summary>
        /// <param name="">ClientRecord</param>
        /// <param name="">IsForUpdate</param>
        /// <returns></returns>
        public bool ClientRecordOperation(ClientRecord Client, bool IsForUpdate)
        {
            return ClientRecordsOperation(Client, IsForUpdate);
        }

        public bool PartnerRecordOperation(PartnerRecord Partner, bool IsForUpdate)
        {
            return PartnerRecordsOperation(Partner,IsForUpdate);
        }

        public bool EmployeeRecordOperation(EmployeeRecord Employee, bool IsForUpdate)
        {
            return EmployeeRecordsOperation(Employee,IsForUpdate);
        }

        /// <summary>
        /// Close DB connection
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void CloseConnection()
        {
            //Close HPRM db connection
            db.Disconnect();
            db.Dispose();
        }
        #endregion
        #region Private methods

        /// <summary>
        /// Create or update the Client Location in HPE CM of type Organization
        /// </summary>
        /// <param name="">ClientLocation,IsForUpdate</param>
        /// <returns></returns>
        private bool ClientLocationOperation(ClientLocation Client, bool IsForUpdate)
        {
            //Create Location object
            Location objLoc = null;

            //flage to update association
            bool isEYClientsUpdated = false;
            try
            {
                if (IsForUpdate)
                {
                    //Get existing location from Name
                    logger.Info(Client.ClientID + " : Obtaining existing Client");
                    objLoc = GetLocationByID(Client.ClientID);
                    if (objLoc == null)
                    {
                        logger.Error(Client.ClientID + " : Client Location does not exist.");
                        return false;
                    }
                }
                else
                {
                    //intialize Location object
                    logger.Info(Client.ClientID + " : Creating New Client object");
                    objLoc = new Location(db);
                }

                //Assign Type of Location
                logger.Info(Client.ClientID + " : Assigning Type of Location - Orgnization");
                objLoc.TypeOfLocation = LocationType.Organization;
                
                if (!IsForUpdate)
                {
                    //Assign Client Name
                    logger.Info(Client.ClientID + " : Assigning Client Name");
                    objLoc.SortName = Client.Name;

                    //Assign Client ID number
                    logger.Info(Client.ClientID + " : Assigning Client ID");
                    objLoc.IdNumber = Client.ClientID;

                    //Setting Security Level to UnClassified
                    objLoc.SecurityString = ConfigurationManager.AppSettings["SecurityLevel"];

                    //set Internal checkbox to False
                    logger.Info(Client.ClientID + " : Setting Internal Location");
                    objLoc.IsWithin = false;

                    //set Unique Name
                    logger.Info(Client.ClientID + " : Setting Unique Name");
                    objLoc.NickName = Client.ClientID;
                }
                else if (objLoc.SortName.ToUpper() != Client.Name.ToUpper())
                {
                    //Update Client Name
                    logger.Info(Client.ClientID + " : Updating Client Name");
                    objLoc.SortName = Client.Name;

                    //Set Unique Name
                    if(objLoc.NickName!=Client.ClientID)
                    {
                        //set Unique Name
                        logger.Info(Client.ClientID + " : Updating Unique Name");
                        objLoc.NickName = Client.ClientID;
                    }
                }


                //Get the Location of EY Clients
                logger.Info(Client.ClientID + " : Getting Location object of EY Client");
                Location locEYClient = new Location(db, ConfigurationManager.AppSettings["ClientAssociation"]);

                if (IsForUpdate && objLoc.AllMemberships.Contains(locEYClient.SortName))
                    isEYClientsUpdated = true;

                if (!isEYClientsUpdated)
                {
                    //Set Association - Member of for EY Clients 
                    logger.Info(Client.ClientID + " : Setting Association to EY Clients");
                    objLoc.AddRelationship(locEYClient, LocRelationshipType.MemberOf, false);
                }

                // Confirm & Save Client in CM
                if (objLoc.Verify(true))
                {
                    objLoc.Save();
                    db.Save();
                    logger.Info(Client.ClientID + " : Client creation/updation successful");
                    return true;
                }
                else
                {
                    Console.Write(objLoc.ErrorMessage);
                    logger.Error(Client.ClientID + " : "+ objLoc.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                UtilityException.Message = ex.Message;
                logger.Error(Client.ClientID + " : " + ex.Message);
                return false;
            }
        }

        // <summary>
        /// Create or update the Client Record in HPE CM
        /// </summary>
        /// <param name="">ClientRecord,IsForUpdate</param>
        /// <returns></returns>
        private bool ClientRecordsOperation(ClientRecord Client, bool IsForUpdate)
        {
            //Create Location object
            Location objLoc = null;

            try
            {
                //Check Client record exists
                if (!IsForUpdate)
                {
                    if(GetClientRecord(Client.ClientID) != null)
                    {
                        //Update for duplicate
                        UtilityException.Message = "Duplicate Record";
                        logger.Info(Client.ClientID + " : Duplicate Client Record found.");
                        return false;
                    }
                    //Create new Record Object for Create
                    logger.Info(Client.ClientID + " : Creating New Client Record.");
                    Client.Record = new Record(new RecordType(db, ConfigurationManager.AppSettings["ClientRecord"].ToString()));

                    Client.NewRecord = true;
                }
                else 
                {
                    //Get Existing Record for update
                    logger.Info(Client.ClientID + " : Obtaining existing client Record.");
                    Client.Record = GetClientRecord(Client.ClientID);
                    Client.NewRecord = false;
                    if(Client.Record==null)
                    {
                        logger.Error(Client.ClientID + " : Client Record does not exist.");
                        return false;
                    }
                }

                //Get existing Client Location
                logger.Info(Client.ClientID + " : Getting Existing Client.");
                objLoc = GetLocationByID(Client.ClientID);

                //Set Client Location
                if (objLoc != null)
                {
                    logger.Info(Client.ClientID + " : Setting Client.");
                    Client.SetClient(objLoc);
                }else
                {
                    logger.Error(Client.ClientID + " : Client not exists.");
                    return false;
                }

                //Set Client Record Title
                logger.Info(Client.ClientID + " : Setting Client Record Title.");
                Client.SetTitle(objLoc.Name);

                //Set Client Record Number
                logger.Info(Client.ClientID + " : Setting Record number.");
                Client.SetRecordNumber(ConfigurationManager.AppSettings["ClientRecordPrefix"].ToString() + Client.ClientID);


                //Set Client ID field
                logger.Info(Client.ClientID + " : Setting Client ID.");
                Client.SetStringField(ConfigurationManager.AppSettings["ClientID"].ToString(), Client.ClientID);

                if (!string.IsNullOrWhiteSpace(Client.Partner1))
                {
                    //Set Partner 1 field
                    Location partner1 = GetLocationByName(Client.Partner1);
                    if (partner1 != null)
                    {
                        logger.Info(Client.ClientID + " : Setting Client Partner 1.");
                        Client.SetLocationField(ConfigurationManager.AppSettings["Partner1"].ToString(), partner1);
                    }
                    else
                    {
                        //Partner 1 Location not found
                        logger.Info(Client.ClientID + " : Partner 1 Location not found.");
                        return false;
                    }
                }
                
                //Set Partner 2 field
                if (!string.IsNullOrWhiteSpace(Client.Partner2))
                {
                    Location partner2 = GetLocationByName(Client.Partner2);
                    if (partner2 != null)
                    {
                        logger.Info(Client.ClientID + " : Setting Client Partner 2.");
                        Client.SetLocationField(ConfigurationManager.AppSettings["Partner2"].ToString(), partner2);
                    }
                    else
                    {
                        //Partner 2 Location not found
                        logger.Info(Client.ClientID + " : Partner 2 Location not found.");
                        return false;
                    }

                }

                //Set Client Status
                logger.Info(Client.ClientID + " : Setting Client Status.");
                Client.SetStringField(ConfigurationManager.AppSettings["ClientStatus"].ToString(), Client.ClientStatus);

                //Set InactiveDate field
                logger.Info(Client.ClientID + " : Setting Inactive Date.");
                Client.SetInactiveDateField(TrimDateTime.Parse(Client.InactiveDate).Date);


                // Confirm & Save Client Record in CM
                //if (Client.Record.Verify(true))
                //{
                    Client.Record.Save();
                    db.Save();
                    logger.Info(Client.ClientID + " : Client Record creation/updation successful");
                    return true;
                //}
                //else
                //{
                //    Console.Write(Client.Record.ErrorMessage);
                //    logger.Error(Client.ClientID + " : " + Client.Record.ErrorMessage);
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                UtilityException.Message = ex.Message;
                logger.Error(Client.ClientID + " : " + ex.Message);
                return false;
            }
        }

        // <summary>
        /// Create or update the Partner Record in HPE CM
        /// </summary>
        /// <param name="">PartnerRecord,IsForUpdate</param>
        /// <returns></returns>
        private bool PartnerRecordsOperation(PartnerRecord Partner,bool IsForUpdate)
        {
            //Create Location object
            Location objLoc = null;

            try
            {
                //Check Partner record exists
                if (!IsForUpdate)
                {
                    if (GetPartnerRecord(Partner.GPN) != null)
                    {
                        //Update for duplicate
                        UtilityException.Message = "Duplicate Record";
                        logger.Info(Partner.GPN + " : Duplicate Partner Record found.");
                        return false;
                    }
                    //Create new Record Object for Partner
                    logger.Info(Partner.GPN + " : Creating New Partner Record.");
                    Partner.Record = new Record(new RecordType(db, ConfigurationManager.AppSettings["PartnerRecord"].ToString()));

                    Partner.NewRecord = true;
                }
                else
                {
                    //Get Existing Record for update
                    logger.Info(Partner.GPN + " : Obtaining existing Partner Record.");
                    Partner.Record = GetPartnerRecord(Partner.GPN);
                    Partner.NewRecord = false;
                    if (Partner.Record == null)
                    {
                        logger.Error(Partner.GPN + " : Partner Record does not exist.");
                        return false;
                    }
                }

                //Get existing Client
                logger.Info(Partner.GPN + " : Getting Existing Client.");
                objLoc = GetLocationByID(Partner.GPN);

                //Set Client Location
                if (objLoc != null)
                {
                    logger.Info(Partner.GPN + " : Setting Client.");
                    Partner.SetClient(objLoc);
                }
                else
                {
                    logger.Error(Partner.GPN + " : Client not exists.");
                    return false;
                }

                //Set Partner Record Title
                logger.Info(Partner.GPN + " : Setting Partner Record Title.");
                Partner.SetTitle(objLoc.Name);

                //Set Partner Record Number
                logger.Info(Partner.GPN + " : Setting Partner Record number.");
                Partner.SetRecordNumber(ConfigurationManager.AppSettings["PartnerRecordPrefix"].ToString() + Partner.GPN);

                //Set Partner Record Gender
                logger.Info(Partner.GPN + " : Setting Partner Record gender.");
                Partner.SetStringField(ConfigurationManager.AppSettings["Gender"].ToString(), Partner.Gender);

                //Set Partner Record Primary Name
                logger.Info(Partner.GPN + " : Setting Partner Primary Name.");
                Partner.SetStringField(ConfigurationManager.AppSettings["PrimaryName"].ToString(), Partner.PrimaryName);

                //Set Partner Record Service Line
                logger.Info(Partner.GPN + " : Setting Partner Record Service Line.");
                Partner.SetStringField(ConfigurationManager.AppSettings["ServiceLine"].ToString(), Partner.ServiceLine);

                //Set Partner Record Sub Service Line
                logger.Info(Partner.GPN + " : Setting Partner Record Sub Service Line.");
                Partner.SetStringField(ConfigurationManager.AppSettings["SubServiceLine"].ToString(), Partner.SubServiceLine);

                //Set Partner Record Original Hire Date
                logger.Info(Partner.GPN + " : Setting Partner Record Original Hire Date.");
                Partner.SetDateTimeField(ConfigurationManager.AppSettings["OriginalHireDate"].ToString(), TrimDateTime.Parse(Partner.OriginalHireDate).Date);

                //Set Partner Record Termination Date
                logger.Info(Partner.GPN + " : Setting Partner Record Original Termination Date.");
                Partner.SetDateTimeField(ConfigurationManager.AppSettings["TerminationDate"].ToString(), TrimDateTime.Parse(Partner.TerminationDate).Date);

                //Set Partner Record Global Personnel Number (GPN)
                logger.Info(Partner.GPN + " : Setting Partner Record Global Personnel Number (GPN).");
                Partner.SetStringField(ConfigurationManager.AppSettings["GPN"].ToString(), Partner.GPN);

                //Set Partner Record "Local Personnel Number (LPN)
                logger.Info(Partner.GPN + " : Setting Partner Record Local Personnel Number (LPN).");
                Partner.SetStringField(ConfigurationManager.AppSettings["LPN"].ToString(), Partner.LPN);

                //Set Partner Record Employee ID
                logger.Info(Partner.GPN + " : Setting Partner Record Employee ID.");
                Partner.SetStringField(ConfigurationManager.AppSettings["EmplID"].ToString(), Partner.EmplID);

                //Set Partner Record Global Unique Identifier (GUI)
                logger.Info(Partner.GPN + " : Setting Partner Record Global Unique Identifier (GUI).");
                Partner.SetStringField(ConfigurationManager.AppSettings["GUI"].ToString(), Partner.GUI);

                //Set Partner Record Pay Group
                logger.Info(Partner.GPN + " : Setting Partner Record Pay Group.");
                Partner.SetStringField(ConfigurationManager.AppSettings["PayGroup"].ToString(), Partner.PayGroup);

                //Set Partner Record Code Block
                logger.Info(Partner.GPN + " : Setting Partner Record Code Block.");
                Partner.SetStringField(ConfigurationManager.AppSettings["CodeBlock"].ToString(), Partner.CodeBlock);

                //Set Partner Record Cost Centre
                logger.Info(Partner.GPN + " : Setting Partner Record Cost Centre.");
                Partner.SetStringField(ConfigurationManager.AppSettings["CostCentre"].ToString(), Partner.CostCentre);

                //Set Partner Record Company Code
                logger.Info(Partner.GPN + " : Setting Partner Record Company Code.");
                Partner.SetStringField(ConfigurationManager.AppSettings["CompanyCode"].ToString(), Partner.CompanyCode);

                //Set Partner Record Business Unit
                logger.Info(Partner.GPN + " : Setting Partner Record Business Unit.");
                Partner.SetStringField(ConfigurationManager.AppSettings["BusinessUnit"].ToString(), Partner.BusinessUnit);

                //Set Partner Record Organisational Unit
                logger.Info(Partner.GPN + " : Setting Partner Record Organisational Unit.");
                Partner.SetStringField(ConfigurationManager.AppSettings["OrganisationalUnit"].ToString(), Partner.OrganisationalUnit);

                //Set Partner Record Management Unit
                logger.Info(Partner.GPN + " : Setting Partner Record Management Unit.");
                Partner.SetStringField(ConfigurationManager.AppSettings["ManagementUnit"].ToString(), Partner.ManagementUnit);

                //Set Partner Record Sub Management Unit
                logger.Info(Partner.GPN + " : Setting Partner Record Sub Management Unit.");
                Partner.SetStringField(ConfigurationManager.AppSettings["SubManagementUnit"].ToString(), Partner.SubManagementUnit);

                //Set Partner Record HR Status
                logger.Info(Partner.GPN + " : Setting Partner Record HR Status.");
                Partner.SetStringField(ConfigurationManager.AppSettings["HRStatus"].ToString(), Partner.HRStatus);

                // Confirm & Save Partner Record in CM
                //if (Partner.Record.Verify(true))
                //{
                    Partner.Record.Save();
                    db.Save();
                    logger.Info(Partner.GPN + " : Partner Record creation/updation successful");
                    return true;
                //}
                //else
                //{
                //    Console.Write(Partner.Record.ErrorMessage);
                //    logger.Error(Partner.GPN + " : " + Partner.Record.ErrorMessage);
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                UtilityException.Message = ex.Message;
                logger.Error(Partner.GPN + " : " + ex.Message);
                return false;
            }
        }

        // <summary>
        /// Create or update the Employee Record in HPE CM
        /// </summary>
        /// <param name="">EmployeeRecord,IsForUpdate</param>
        /// <returns></returns>
        private bool EmployeeRecordsOperation(EmployeeRecord Employee,bool IsForUpdate)
        {
            //Create Location object
            Location objLoc = null;

            try
            {
                //Check Employee record exists
                if (!IsForUpdate)
                {
                    if (GetEmployeeRecord(Employee.GPN) != null)
                    {
                        //Update for duplicate
                        UtilityException.Message = "Duplicate Record";
                        logger.Info(Employee.GPN + " : Duplicate Employee Record found.");
                        return false;
                    }
                    //Create new Record Object for Employee
                    logger.Info(Employee.GPN + " : Creating New Employee Record.");
                    Employee.Record = new Record(new RecordType(db, ConfigurationManager.AppSettings["EmployeeRecord"].ToString()));

                    Employee.NewRecord = true;
                }
                else
                {
                    //Get Existing Record for update
                    logger.Info(Employee.GPN + " : Obtaining existing Employee Record.");
                    Employee.Record = GetEmployeeRecord(Employee.GPN);
                    Employee.NewRecord = false;
                    if (Employee.Record == null)
                    {
                        logger.Error(Employee.GPN + " : Employee Record does not exist.");
                        return false;
                    }
                }

                //Get existing Client
                logger.Info(Employee.GPN + " : Getting Existing Client.");
                objLoc = GetLocationByID(Employee.GPN);

                //Set Client Location
                if (objLoc != null)
                {
                    logger.Info(Employee.GPN + " : Setting Client.");
                    Employee.SetClient(objLoc);
                }
                else
                {
                    logger.Error(Employee.GPN + " : Client not exists.");
                    return false;
                }

                //Set Employee Record Title
                logger.Info(Employee.GPN + " : Setting Employee Record Title.");
                Employee.SetTitle(objLoc.Name);

                //Set Employee Record Number
                logger.Info(Employee.GPN + " : Setting Employee Record number.");
                Employee.SetRecordNumber(ConfigurationManager.AppSettings["EmployeeRecordPrefix"].ToString() + Employee.GPN);

                //Set Employee Record Primary Name
                logger.Info(Employee.GPN + " : Setting Employee Primary Name.");
                Employee.SetStringField(ConfigurationManager.AppSettings["PrimaryName"].ToString(), Employee.PrimaryName);

                //Set Employee Record
                logger.Info(Employee.GPN + " : Setting Employee Record Employee Type.");
                Employee.SetStringField(ConfigurationManager.AppSettings["EmployeeType"].ToString(), Employee.EmployeeType);

                //Set Employee Record
                logger.Info(Employee.GPN + " : Setting Employee Record EY Rank.");
                Employee.SetStringField(ConfigurationManager.AppSettings["EYRank"].ToString(),Employee.EyRank);

                //Set Employee Record Gender
                logger.Info(Employee.GPN + " : Setting Employee Record gender.");
                Employee.SetStringField(ConfigurationManager.AppSettings["Gender"].ToString(), Employee.Gender);

                //Set Employee Record Service Line
                logger.Info(Employee.GPN + " : Setting Employee Record Service Line.");
                Employee.SetStringField(ConfigurationManager.AppSettings["ServiceLine"].ToString(), Employee.ServiceLine);

                //Set Employee Record Sub Service Line
                logger.Info(Employee.GPN + " : Setting Employee Record Sub Service Line.");
                Employee.SetStringField(ConfigurationManager.AppSettings["SubServiceLine"].ToString(), Employee.SubServiceLine);

                //Set Employee Record Company Seniority Date
                logger.Info(Employee.GPN + " : Setting Employee Record Company Seniority Date.");
                Employee.SetDateTimeField(ConfigurationManager.AppSettings["CompanySeniorityDate"].ToString(), TrimDateTime.Parse(Employee.CompanySeniorityDate).Date);

                //Set Employee Record Termination Date
                logger.Info(Employee.GPN + " : Setting Employee Record Termination Date.");
                Employee.SetDateTimeField(ConfigurationManager.AppSettings["TerminationDate"].ToString(), TrimDateTime.Parse(Employee.TerminationDate).Date);

                //Set Employee Employee Global Personnel Number (GPN)
                logger.Info(Employee.GPN + " : Setting Employee Record Global Personnel Number (GPN).");
                Employee.SetStringField(ConfigurationManager.AppSettings["GPN"].ToString(), Employee.GPN);

                //Set Employee Record "Local Personnel Number (LPN)
                logger.Info(Employee.GPN + " : Setting Employee Record Local Personnel Number (LPN).");
                Employee.SetStringField(ConfigurationManager.AppSettings["LPN"].ToString(), Employee.LPN);

                //Set Employee Record Employee ID
                logger.Info(Employee.GPN + " : Setting Employee Record Employee ID.");
                Employee.SetStringField(ConfigurationManager.AppSettings["EmplID"].ToString(), Employee.EmplID);

                //Set Employee Record Global Unique Identifier (GUI)
                logger.Info(Employee.GPN + " : Setting Employee Record Global Unique Identifier (GUI).");
                Employee.SetStringField(ConfigurationManager.AppSettings["GUI"].ToString(), Employee.GUI);

                //Set Employee Record Pay Group
                logger.Info(Employee.GPN + " : Setting Employee Record Pay Group.");
                Employee.SetStringField(ConfigurationManager.AppSettings["PayGroup"].ToString(), Employee.PayGroup);

                //Set Employee Record Code Block
                logger.Info(Employee.GPN + " : Setting Employee Record Code Block.");
                Employee.SetStringField(ConfigurationManager.AppSettings["CodeBlock"].ToString(), Employee.CodeBlock);

                //Set Employee Record Cost Centre
                logger.Info(Employee.GPN + " : Setting Employee Record Cost Centre.");
                Employee.SetStringField(ConfigurationManager.AppSettings["CostCentre"].ToString(), Employee.CostCentre);

                //Set Employee Record Company Code
                logger.Info(Employee.GPN + " : Setting Employee Record Company Code.");
                Employee.SetStringField(ConfigurationManager.AppSettings["CompanyCode"].ToString(), Employee.CompanyCode);

                //Set Employee Record Business Unit
                logger.Info(Employee.GPN + " : Setting Employee Record Business Unit.");
                Employee.SetStringField(ConfigurationManager.AppSettings["BusinessUnit"].ToString(), Employee.BusinessUnit);

                //Set Employee Record Organisational Unit
                logger.Info(Employee.GPN + " : Setting Employee Record Organisational Unit.");
                Employee.SetStringField(ConfigurationManager.AppSettings["OrganisationalUnit"].ToString(), Employee.OrganisationalUnit);

                //Set Employee Record Management Unit
                logger.Info(Employee.GPN + " : Setting Employee Record Management Unit.");
                Employee.SetStringField(ConfigurationManager.AppSettings["ManagementUnit"].ToString(), Employee.ManagementUnit);

                //Set Employee Record Sub Management Unit
                logger.Info(Employee.GPN + " : Setting Employee Record Sub Management Unit.");
                Employee.SetStringField(ConfigurationManager.AppSettings["SubManagementUnit"].ToString(), Employee.SubManagementUnit);

                //Set Employee Record HR Status
                logger.Info(Employee.GPN + " : Setting Employee Record HR Status.");
                Employee.SetStringField(ConfigurationManager.AppSettings["HRStatus"].ToString(), Employee.HRStatus);

                //Set Employee Record Rehire Date
                logger.Info(Employee.GPN + " : Setting Employee Record Rehire Date.");
                Employee.SetDateTimeField(ConfigurationManager.AppSettings["RehireDate"].ToString(), TrimDateTime.Parse(Employee.RehireDate).Date);

                //Set Employee Record Dept ID
                logger.Info(Employee.GPN + " : Setting Employee Record Dept ID.");
                Employee.SetStringField(ConfigurationManager.AppSettings["DeptID"].ToString(), Employee.DeptID);

                //Set Employee Record Geographic Country
                logger.Info(Employee.GPN + " : Setting Employee Record Geographic Country.");
                Employee.SetStringField(ConfigurationManager.AppSettings["GeographicCountry"].ToString(), Employee.GeographicCountry);

                // Confirm & Save Employee Record in CM
                //if (Employee.Record.Verify(true))
                //{
                    Employee.Record.Save();
                    db.Save();
                    logger.Info(Employee.GPN + " : Employee Record creation/updation successful");
                    return true;
                //}
                //else
                //{
                //    Console.Write(Employee.Record.ErrorMessage);
                //    logger.Error(Employee.GPN + " : " + Employee.Record.ErrorMessage);
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                UtilityException.Message = ex.Message;
                logger.Error(Employee.GPN + " : " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create the User Location in HPE CM of type Person
        /// </summary>
        /// <param name="">UserLocation,IsForUpdate</param>
        /// <returns></returns>
        private bool UserLocationOperation(UserLocation User, bool IsForUpdate)
        {
            //Create Location object
            Location objLoc = null;

            try
            {
                //Given Name
                string givenName = string.Empty;
                if (string.IsNullOrEmpty(User.MiddleName))
                    givenName = User.PreferredName;
                else
                    givenName = User.PreferredName + " " + User.MiddleName[0];

                if (IsForUpdate)
                {
                    //Get existing location from Name
                    logger.Info(User.GPN + " : Obtaining existing User object");
                    objLoc = GetLocationByID(User.GPN);
                    if (objLoc == null)
                    {
                        logger.Error(User.GPN + " : User Location does not exist."); 
                        return false;
                    }
                }
                else
                {
                    //Intilize Location object
                    logger.Info(User.GPN + " : Creating New User object");
                    objLoc = new Location(db);
                }

                
                if (!IsForUpdate)
                {
                    //Assign type of Location
                    logger.Info(User.GPN + " : Assigning Type of Location - Person");
                    objLoc.TypeOfLocation = LocationType.Person;

                    // Assign GPN and set Internal checkbox to true
                    logger.Info(User.GPN + " : Setting Internal Location");
                    objLoc.IsWithin = true;

                    logger.Info(User.GPN + " : Assigning ID number to User");
                    objLoc.IdNumber = User.GPN;

                    //Set the profile of EndUser
                    logger.Info(User.GPN + " : Setting UserType of Location to EndUser");
                    objLoc.UserType = UserTypes.EndUser;

                    //Setting Security Level to UnClassified
                    objLoc.SecurityString = ConfigurationManager.AppSettings["SecurityLevel"];

                    //Setting Unique Name
                    logger.Info(User.GPN + " : Setting Unique Name");
                    objLoc.NickName = User.GPN;
                }

                //Assign Employee First and Last Name
                logger.Info(User.GPN + " : Assigning  First and Last name of User");
                objLoc.Surname = User.LastName;
                objLoc.GivenNames = givenName;

                //Set Active from and To date
                logger.Info(User.GPN + " : Setting Active Date Range for User");
                TrimDateTime fromDate = new TrimDateTime(User.FromDate);
                TrimDateTime toDate = new TrimDateTime(User.ToDate);
                TrimDateTime currentDate=new TrimDateTime(DateTime.Now);
                objLoc.SetActiveDateRange(fromDate, toDate);

                if (string.IsNullOrEmpty(User.ToDate) || toDate > currentDate)
                {
                    //Assign NetworkLogin details
                    objLoc.CanLogin = false;
                    logger.Info(User.GPN + " : Setting Network Login ID");
                    if (User.DomainName == string.Empty || User.NetworkLogin == string.Empty)
                    {
                        logger.Error(User.GPN + " : AD credentials of the user - domain name and/or network login are not available.");
                        return false;
                    }
                    objLoc.LogsInAs = User.DomainName + "\\" + User.NetworkLogin; // Network login id
                }

                //Get the EAddresses
                logger.Info(User.GPN + " : Setting Email Address for User");
                LocationEAddresses eAddsses = objLoc.ChildEAddresses;
                LocationEAddress eAddss = null;

                //Set Email address of type Internet
                if (!IsForUpdate || eAddsses.Count<=0)
                {
                    eAddss = eAddsses.New();
                }
                else
                {
                    for(uint i=0;i<eAddsses.Count;i++)
                    {
                        eAddss=eAddsses.getItem(i);
                        if (eAddss.ElectronicAddressData == User.SMTPEmail)
                            break;
                    }
                }
                eAddss.ElectronicAddressType = EAddressType.Mail;
                eAddss.SetEmailAddress(User.SMTPEmail, "", "SMTP");

                //Get the Location of Service Line,Sub Service Line,Location
                logger.Info(User.GPN + " : Getting Location object of User");
                Location location = new Location(db, "EY " + User.Location + " Office");

                //Get the Location of Location Record Service
                logger.Info(User.GPN + " : Getting Location Record Services object of User");
                Location locSer = new Location(db, User.Location + " Records Services");

                Location serviceLine = new Location(db);
                Location subServiceLine = new Location(db);

                ServiceSubServiceLineHandler(User, location, ref serviceLine, ref subServiceLine);

                if (IsForUpdate)
                {
                    //AssociationHandler(objLoc);
                }
                //Set Association - Member of for Location 
                logger.Info(User.GPN + " : Setting Association to Location Record Services of User");
                objLoc.AddRelationship(locSer, LocRelationshipType.MemberOf, false);
                
                logger.Info(User.GPN + " : Setting Association to SubServiceline of User");
                objLoc.AddRelationship(subServiceLine, LocRelationshipType.MemberOf, false);
                

                // Confirm & Save User in CM
                if (objLoc.Verify(true))
                {
                    objLoc.Save();
                    db.Save();
                    logger.Info(User.GPN + " : User creation/updation Successful");
                    return true;
                }
                else
                {
                    logger.Error(User.GPN + " : " + objLoc.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                UtilityException.Message = ex.Message;
                logger.Error(User.GPN + " : " + ex.Message);
                return false;
            }
        }

        private void ServiceSubServiceLineHandler(UserLocation User, Location location, ref Location serviceLine, ref Location subServiceLine)
        {
            logger.Info(User.GPN + " : Getting Serviceline object of User");
            serviceLine = GetServiceLocation(location, User.ServiceLine);
            if (serviceLine == null)
            {
                //ServiceLine is not found so creating new serviceLine location
                logger.Info(User.GPN + " : ServiceLine does not exist, creating new ServiceLine : " + User.ServiceLine);
                serviceLine = new Location(db, LocationType.Organization);

                //assiging name to serviceLine object
                serviceLine.Surname = User.ServiceLine;

                //Set internal flag to true
                serviceLine.IsWithin = true;

                //Setting Security Level to UnClassified
                serviceLine.SecurityString = ConfigurationManager.AppSettings["SecurityLevel"];

                //Setting association to EY Location office for serviceLine
                logger.Info(User.GPN + " : For ServiceLine setting member of assoiciation to " + location.Name);
                serviceLine.AddRelationship(location, LocRelationshipType.MemberOf, false);

                //save serviceLine location
                serviceLine.Save();
                logger.Info(User.GPN + " : ServiceLine location created.");

            }

            logger.Info(User.GPN + " : Getting SubServiceline object of User");
            subServiceLine = GetSubServiceLocation(location, serviceLine, User.SubServiceLine);
            if (subServiceLine == null)
            {
                //SubServiceLine is not found so creating new subServiceLine location
                logger.Info(User.GPN + " : SubServiceLine does not exist, Creating new SubServiceLine : " + User.SubServiceLine);
                subServiceLine = new Location(db, LocationType.Organization);

                //assiging name to subServiceLine object
                subServiceLine.Surname = User.SubServiceLine;

                //Set internal flag to true
                subServiceLine.IsWithin = true;

                //Setting Security Level to UnClassified
                subServiceLine.SecurityString = ConfigurationManager.AppSettings["SecurityLevel"];

                //Setting association to EY Location office for subServiceLine
                logger.Info(User.GPN + " : For SubServiceLine setting member of assoiciation to " + serviceLine.Name);
                subServiceLine.AddRelationship(serviceLine, LocRelationshipType.MemberOf, false);

                //save serviceLine location
                subServiceLine.Save();
                logger.Info(User.GPN + " : SubServiceLine location created.");
            }
        }

        /// <summary>
        /// Update user Location
        /// </summary>
        /// <param name="">location</param>
        /// <returns>bool</returns>
        private bool UpdateUserLocationOperation(UserLocation User)
        {
            //Create Location object
            Location objLoc = null;
            try
            {
                //Given Name
                string givenName = string.Empty;
                if (string.IsNullOrEmpty(User.MiddleName))
                    givenName = User.PreferredName;
                else
                    givenName = User.PreferredName + " " + User.MiddleName[0];


                //Get existing location from Name
                logger.Info(User.GPN + " : Obtaining existing User object");
                objLoc = GetLocationByID(User.GPN);
                if (objLoc == null)
                {
                    logger.Error(User.GPN + " : User Location does not exist.");
                    return false;
                }

                //Assign Employee First and Last Name
                if (objLoc.Surname.ToUpper() != User.LastName.ToUpper())
                {
                    logger.Info(User.GPN + " : Updating Last name of User");
                    objLoc.Surname = User.LastName;
                }
                if (objLoc.GivenNames.ToUpper() != givenName.ToUpper())
                {
                    logger.Info(User.GPN + " : Updating First name of User");
                    objLoc.GivenNames = givenName;
                }

                //Set Unique Name
                if(objLoc.NickName!=User.GPN)
                {
                    logger.Info(User.GPN + " : Updating Unique Name of User");
                    objLoc.NickName = User.GPN;
                }

                //Set Active from and To date
                
                TrimDateTime fromDate = new TrimDateTime(User.FromDate);
                TrimDateTime toDate = new TrimDateTime(User.ToDate);
                TrimDateTime currentDate = new TrimDateTime(DateTime.Now);

                if (objLoc.DateActiveFrom != fromDate || objLoc.DateActiveTo != toDate)
                {
                    logger.Info(User.GPN + " : Updating Active Date Range for User");
                    objLoc.SetActiveDateRange(fromDate, toDate);
                }

                //Assign NetworkLogin details
                if (string.IsNullOrEmpty(User.ToDate) || toDate > currentDate)
                {
                    
                    objLoc.CanLogin = false;
                    
                    if (User.DomainName == string.Empty || User.NetworkLogin == string.Empty)
                    {
                        logger.Error(User.GPN + " : AD credentials of the user - domain name and/or network login are not available.");
                        return false;
                    }
                    else if (objLoc.LogsInAs.ToUpper() != User.DomainName.ToUpper() + "\\" + User.NetworkLogin.ToUpper())
                    {
                        objLoc.LogsInAs = User.DomainName + "\\" + User.NetworkLogin; // Network login id
                        logger.Info(User.GPN + " : Updating Network Login ID");
                    }
                }


                //Get the EAddresses
                
                LocationEAddresses eAddsses = objLoc.ChildEAddresses;
                LocationEAddress eAddss = null;
                bool isEmailIdExists = false;
                //Set Email address of type Internet
                if (eAddsses.Count <= 0)
                {
                    eAddss = eAddsses.New();
                }
                else
                {
                    for (uint i = 0; i < eAddsses.Count; i++)
                    {
                        eAddss = eAddsses.getItem(i);
                        if (eAddss.ElectronicAddressData.ToUpper() == User.SMTPEmail.ToUpper())
                        {
                            isEmailIdExists = true;
                            break;
                        }
                    }
                }
                if (!isEmailIdExists)
                {
                    eAddss.ElectronicAddressType = EAddressType.Mail;
                    eAddss.SetEmailAddress(User.SMTPEmail, "", "SMTP");
                    logger.Info(User.GPN + " : Updating Email Address for User");
                }

                //Get the Location of Service Line,Sub Service Line,Location
                Location location = new Location(db, "EY " + User.Location + " Office");

                //Get the Location of Location Record Service
                Location locSer = new Location(db, User.Location + " Records Services");

                Location serviceLine = new Location(db);
                Location subServiceLine = new Location(db);

                ServiceSubServiceLineHandler(User, location, ref serviceLine, ref subServiceLine);

                //Set Association - Member of for Location
                if (!objLoc.HasRelationship(locSer, LocRelationshipType.MemberOf))
                {
                    AssociationHandler(objLoc,LocationType.Position);
                    logger.Info(User.GPN + " : Updating Association to Location Record Services of User");
                    objLoc.AddRelationship(locSer, LocRelationshipType.MemberOf, false);
                }

                if (!objLoc.HasRelationship(subServiceLine, LocRelationshipType.MemberOf))
                {
                    AssociationHandler(objLoc, LocationType.Organization);
                    logger.Info(User.GPN + " : Updating Association to SubServiceline of User");
                    objLoc.AddRelationship(subServiceLine, LocRelationshipType.MemberOf, false);
                }

                

                // Confirm & Save User in CM
                if (objLoc.Verify(true))
                {
                    objLoc.Save();
                    db.Save();
                    logger.Info(User.GPN + " : User updation Successful");
                    return true;
                }
                else
                {
                    logger.Error(User.GPN + " : " + objLoc.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                UtilityException.Message = ex.Message;
                logger.Error(User.GPN + " : " + ex.Message);
                return false;
            }
 
        }

        #endregion
        #region private methods
        /// <summary>
        /// Get Service Line Location
        /// </summary>
        /// <param name="">location</param>
        /// <param name="">serviceLine</param>
        /// <returns>Location</returns>
        private Location GetServiceLocation(Location location,string serviceLine)
        {
            TrimMainObjectSearch searchLocs = new TrimMainObjectSearch(db, BaseObjectTypes.Location);
            searchLocs.SetSearchString("name:\""+serviceLine+"\"");
            foreach (Location loc in searchLocs)
            {
                if (loc.AllMemberships.Contains(location.Name))
                    return loc;
            }
            return null;
        }

        /// <summary>
        /// Get Sub service Line Location
        /// </summary>
        /// <param name="">serviceLine</param>
        /// <param name="">subServiceLine</param>
        /// <returns>Location</returns>
        private Location GetSubServiceLocation(Location location,Location serviceLine, string subServiceLine)
        {
            TrimMainObjectSearch searchLocs = new TrimMainObjectSearch(db, BaseObjectTypes.Location);
            searchLocs.SetSearchString("members:[members:"+location.Uri+" and name:"+serviceLine.Name+"] and name:\""+subServiceLine+"\"");
            foreach (Location loc in searchLocs)
            {
                if(serviceLine.AllMembers.Contains(loc.Name) && loc.AllMemberships.Contains(serviceLine.Name))
                return loc;
            }
            return null;
        }
        /// <summary>
        /// Get Location from ID number
        /// </summary>
        /// <param name="">location</param>
        /// <returns>Location</returns>
        private Location GetLocationByID(string IDNumber)
        {
            TrimMainObjectSearch searchLocs = new TrimMainObjectSearch(db, BaseObjectTypes.Location);
            searchLocs.SetSearchString("ID:\"" + IDNumber + "\"");
            if(searchLocs.FastCount>0)
                foreach (Location loc in searchLocs)
                {
                    return loc;
                }
            
            return null;
        }
        /// <summary>
        /// Get Location from Name
        /// </summary>
        /// <param name="">location</param>
        /// <returns>Location</returns>
        private Location GetLocationByName(string name)
        {
            var search = new TrimMainObjectSearch(db, BaseObjectTypes.Location);
            search.SetSearchString("default:\""+ name + "\"");

            Location location = null;

            if (search.FastCount > 0)
            {
                foreach (Location l in search)
                {
                    location = l;
                    break;
                }
            }
            return location;
        }

        /// <summary>
        /// Method to maitain Association of User Location
        /// </summary>
        /// <param name="">location</param>
        /// <param name=""></param>
        /// <returns></returns>
        private void AssociationHandler(Location objLoc,LocationType locRelType)
        {
            //Code to Maitain Previous Association

            //Get All memberships
            string[] allMembers = objLoc.AllMemberships.Split(';');
            foreach (string member in allMembers)
            {
                TrimMainObjectSearch searchLocs = new TrimMainObjectSearch(db, BaseObjectTypes.Location);
                searchLocs.SetSearchString("name:\"" + member.Trim() + "\"");
                foreach (Location memberLoc in searchLocs)
                {
                    //For updating User remove Subservice line and Location Record Service Relationships
                    if (memberLoc.TypeOfLocation == locRelType)
                    {
                        objLoc.RemoveRelationship(memberLoc);
                    }
                }
            }
        }

        /// <summary>
        /// Method to get Employee Record
        /// </summary>
        /// <param name="">GPN</param>
        /// 
        /// <returns></returns>
        private Record GetEmployeeRecord(string GPN)
        {
            Record result = null;

            var search = new TrimMainObjectSearch(db, BaseObjectTypes.Record);
            search.SetSearchString("number:EMP-" + GPN);
            search.SetFilterString("type:[Employee Record]");

            foreach (Record record in search)
            {
                result = record;
                break;
            }

            return result;
        }

        /// <summary>
        /// Method to get Partner Record
        /// </summary>
        /// <param name="">GPN</param>
        /// 
        /// <returns></returns>
        private Record GetPartnerRecord(string GPN)
        {
            Record result = null;

            var search = new TrimMainObjectSearch(db, BaseObjectTypes.Record);
            search.SetSearchString("number:PAR-" + GPN);
            search.SetFilterString("type:[Partner Record]");

            foreach (Record record in search)
            {
                result = record;
                break;
            }

            return result;
        }

        /// <summary>
        /// Method to get Client Record
        /// </summary>
        /// <param name="">ClientID</param>
        /// 
        /// <returns></returns>
        private Record GetClientRecord(string ClientID)
        {
            Record result = null;

            var search = new TrimMainObjectSearch(db, BaseObjectTypes.Record);
            search.SetSearchString("number:C-" + ClientID);
            search.SetFilterString("type:[Client]");

            foreach (Record record in search)
            {
                result = record;
                break;
            }

            return result;
        }
        #endregion

    }
}
