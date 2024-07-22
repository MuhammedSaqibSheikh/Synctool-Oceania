using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRIM.SDK;
using EYLocationSyncUtility.BLL;
using log4net;
using System.Reflection;
using EYLocationSyncUtility.BEL;

namespace EYLocationSyncUtility
{
    class Program
    {        
        private static ILog logger = null;

        static void Main(string[] args)
        {
            //Business service object
            BusinessService service = new BusinessService();

            //Helper object
            Helper objHelper = new Helper();

            //Logger object
            logger = LogManager.GetLogger("Job");

            //Intialize all the flag
            service.InitializeFlag();

            try
            {
                //Check SQL connection
                service.IsSQLConnected();

            }
            catch (Exception ex)
            {
                //log the error
                logger.Error("SQL connection Error : "+ ex.Message);

                //Send EMail to Admin to notify the error
                objHelper.ExceptionEmail(ex.Message);

                //stop processing
                return;
            }

            try
            {
                //Check HPRM connection
                service.IsHPRMConnected();

            }
            catch (Exception ex)
            {
                //log the error
                logger.Error("HPRM connection Error : " + ex.Message);

                //Send EMail to Admin to notify the error
                objHelper.ExceptionEmail(ex.Message);

                //stop processing
                return;
            }
            
            logger.Info("HPCM Synch Utility " + Assembly.GetExecutingAssembly().GetName().Version + " started.");

            //Send Mail to admin - Sync Utility started
            objHelper.AdminEmail("Started");
            
            //Check weather client data is ready for processing
            if (service.IsDataReadyFor("Client"))
            {
                logger.Info("Client location data processing started.....");

                //Create Client Location in CM
                service.ClientLocationOperation(false);
                logger.Info("Location creation process for clients completed.");

                //Update Client Location in CM
                service.ClientLocationOperation(true);
                logger.Info("Location update process for clients completed.");

                //Update LastSyncDate in Database
                if (EYLocationSyncUtility.BEL.Report.IsClientCreated && EYLocationSyncUtility.BEL.Report.IsClientUpdated)
                service.ProcessOverFor("Client");

            }
            else
            {
                objHelper.ProcessNotInReadyStateFor("Client");
                logger.Info("Client Data is not ready for processing");
            }

            //Check weather User data is ready for processing
            if (service.IsDataReadyFor("User"))
            {
                logger.Info("User data processing started....");

                //Create User Location in CM
                service.UserLocationOperation(false);
                logger.Info("Location creation process for users completed.");

                //Update User Location in CM
                service.UserLocationOperation(true);
                logger.Info("Location update process for users completed.");

                //Update LastSyncDate in Database
                if (EYLocationSyncUtility.BEL.Report.IsUserCreated && EYLocationSyncUtility.BEL.Report.IsUserUpdated)
                service.ProcessOverFor("User");
            }
            else
            {
                objHelper.ProcessNotInReadyStateFor("User");
                logger.Info("User Data is not ready for processing");
            }

            //Check weather Employee Record data is ready for processing
            if (service.IsDataReadyFor("EmployeeRecord"))
            {
                logger.Info("Employee Record data processing started....");

                //Create Employee Record in CM
                service.EmployeeRecordOperation(false);
                logger.Info("Record creation process for Employees completed.");

                //Update Employee Record in CM
                service.EmployeeRecordOperation(true);
                logger.Info("Record update process for Employees completed.");

                //Update LastSyncDate in Database
                if (EYLocationSyncUtility.BEL.Report.IsEmployeeRecordCreated && EYLocationSyncUtility.BEL.Report.IsEmployeeRecordUpdated)
                    service.ProcessOverFor("EmployeeRecord");
            }
            else
            {
                objHelper.ProcessNotInReadyStateFor("EmployeeRecord");
                logger.Info("Employee Record Data is not ready for processing");
            }

            //Check weather Partner Record data is ready for processing
            if (service.IsDataReadyFor("PartnerRecord"))
            {
                logger.Info("Partner Record data processing started....");

                //Create Partner Record in CM
                service.PartnerRecordOperation(false);
                logger.Info("Record creation process for Partners completed.");

                //Update Partner Record in CM
                service.PartnerRecordOperation(true);
                logger.Info("Record update process for Partners completed.");

                //Update LastSyncDate in Database
                if (EYLocationSyncUtility.BEL.Report.IsPartnerRecordCreated && EYLocationSyncUtility.BEL.Report.IsPartnerRecordUpdated)
                    service.ProcessOverFor("PartnerRecord");
            }
            else
            {
                objHelper.ProcessNotInReadyStateFor("PartnerRecord");
                logger.Info("Partner Record Data is not ready for processing");
            }

            //Check weather Client Record data is ready for processing
            if (service.IsDataReadyFor("ClientRecord"))
            {
                logger.Info("Client Record data processing started....");

                //Create Client Record in CM
                service.ClientRecordOperation(false);
                logger.Info("Record creation process for Clients completed.");

                //Update Client Record in CM
                service.ClientRecordOperation(true);
                logger.Info("Record update process for Clients completed.");

                //Update LastSyncDate in Database
                if (EYLocationSyncUtility.BEL.Report.IsClientRecordCreated && EYLocationSyncUtility.BEL.Report.IsClientRecordUpdated)
                    service.ProcessOverFor("ClientRecord");
            }
            else
            {
                objHelper.ProcessNotInReadyStateFor("ClientRecord");
                logger.Info("Client Record Data is not ready for processing");
            }


            //Close All connection
            service.CloseConnections();

            //Send Mail to admin - Sync Utility completed 
            //objHelper.AdminEmail("Completed");
            objHelper.ReportEmail();
            logger.Info("HPRM Synch Location Utility " + Assembly.GetExecutingAssembly().GetName().Version + " process completed execution.");
        }
        private static void CreateUser()
        {
            Database db = new Database();
            db.Id = "ED";
            db.Connect();
            string preferedName = "Bhalakiya";
            string middleName = "Patel";
            Location objLoc = new Location(db,"Ketan,Bhalakiya");
            objLoc.TypeOfLocation = LocationType.Person;
            // Name
            objLoc.Surname = "Ketan";
            if (string.IsNullOrEmpty(middleName))
                objLoc.GivenNames = preferedName;
            else
                objLoc.GivenNames = preferedName + " " + middleName[0];

            // Personal & Administrative
            objLoc.IsWithin = true; // Internal to the org
            objLoc.IdNumber = "7939061";
            
            // Login details
            objLoc.CanLogin = true;
            objLoc.LogsInAs = "Asiapacific\\bhalakiy"; // Network login id
            

            //User Profile
            objLoc.UserType = UserTypes.EndUser;

            //Active from and To
            TrimDateTime fromDate=new TrimDateTime(DateTime.Now);
            TrimDateTime toDate = new TrimDateTime(DateTime.Now.AddYears(1));
            objLoc.SetActiveDateRange(fromDate,toDate);

            //Email Address
            LocationEAddresses eAddsses = objLoc.ChildEAddresses;
            LocationEAddress eAddss = eAddsses.getItem(0);
            eAddss.ElectronicAddressType = EAddressType.Mail;
            //eAddss.SetEmailAddress("kbhalakiya@gmail.com", "Kbhalaiya", "SMTP");
            
            //Association member of
            Location loc = new Location(db, "EY Perth Office");
            Location serviceLine = null;
            TrimMainObjectSearch locs = new TrimMainObjectSearch(db, BaseObjectTypes.Location);
            locs.SetSearchString("name:TAX");
            foreach (Location l in locs)
            {
                if (l.AllMemberships.Contains(loc.Name))
                    serviceLine = l;
            }

            //Location subServiceLine = new Location(db, "AUDIT");
            string allRel = objLoc.AllMemberships;

            objLoc.AddRelationship(loc, LocRelationshipType.MemberOf, false);
            objLoc.AddRelationship(serviceLine, LocRelationshipType.MemberOf, false);
            //objLoc.AddRelationship(subServiceLine, LocRelationshipType.MemberOf, false);
            
           

            // Confirm & Save
            if (objLoc.Verify(true))
            {
                objLoc.Save();
                Console.Write(objLoc.FormattedName + " created.");
            }
            else
                Console.Write(objLoc.ErrorMessage);
            db.Save();

            db.Disconnect();
            db.Dispose();
        }
        private static void CraeteCLient()
        {
            Database db = new Database();
            db.Id = "ED";
            db.Connect();

            Location objLoc = new Location(db);
            objLoc.TypeOfLocation = LocationType.Organization;
            // Name
            objLoc.SortName = "CRAY VALLEY SA";
            
            objLoc.IdNumber = DateTime.Now.ToString();

            // Confirm & Save
            if (objLoc.Verify(true))
            {
                objLoc.Save();
                Console.Write(objLoc.FormattedName + " created.");
            }
            else
                Console.Write(objLoc.ErrorMessage);
            db.Save();

            db.Disconnect();
            db.Dispose();
        }
    }
}
