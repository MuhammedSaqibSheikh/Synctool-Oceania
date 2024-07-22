using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EYLocationSyncUtility.DAL;
using EYLocationSyncUtility.BEL;
using System.Data;
using log4net;

namespace EYLocationSyncUtility.BLL
{
    
    
    /// <summary>
    /// Provides methods containing all business logic for the Location Sych 
    /// </summary>
    public class BusinessService
    {
        #region Private Members
        SQLDataService sqlDAL = null;
        RMDataService RMDAL = null;
        
        //Logger object
        private static ILog logger = LogManager.GetLogger("Job");
        #endregion

        #region Public method
        /// <summary>
        /// Method to Creaet or update Client Location
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public void ClientLocationOperation(bool IsForUpdate)
        {
            //Initiate RMSataService Object
            if(RMDAL==null)
            RMDAL = new RMDataService();

            //Initiate SQLData service object
            if (sqlDAL == null)
            sqlDAL = new SQLDataService();

            //Create DataSet object
            DataSet ds = null;
            
            //Get the dataset of Client to be update
            try
            {
                ds = sqlDAL.GetClientSet(IsForUpdate);
            }
            catch (Exception ex)
            {
                logger.Warn("While fetching the client data from staging table SQL server error occurred");
                logger.Error("SQL Error : " + ex.Message);
                if (IsForUpdate)
                    Report.IsClientUpdated = false;
                else
                    Report.IsClientCreated = false;
                return;
            }

            //Create Client Location Object
            ClientLocation client = new ClientLocation();

            //Get the Data Table
            DataTable dt = ds.Tables[0];

            if (IsForUpdate)
            {
                //Total Client Location to be update
                Report.totalClientUpdate = dt.Rows.Count;
                Report.clientUpdated = 0;
                Report.clientNotUpdated = 0;
                Report.clientIsUpdateRMUpdate = 0;
            }
            else
            {
                //Total Client Location to be create
                Report.totalClientCreate = dt.Rows.Count;
                Report.clientUpdated = 0;
                Report.clientNotCreated = 0;
                Report.duplicateClient = 0;
                Report.clientIsUpdateRMCreate = 0;
            }
            //Iterate through all rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //Get the client object from row
                client = CreateClientLocObject(dt.Rows[i]);

                //Reset The UtilityException Message
                UtilityException.Message = string.Empty;

                //Create or update the Client Location
                if (RMDAL.ClientOperation(client, IsForUpdate))
                {
                    //Update isUpdatedInRM field to true in staging table
                    try
                    {
                        sqlDAL.UpdateClientLocStatus(client.ClientID);
                    }
                    catch (Exception ex)
                    {
                        if (IsForUpdate)
                        {
                            logger.Warn(client.ClientID + " : " + "Client Location Updated in HPRM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Info("SQL Error : " + ex.Message);
                            //Increase Client update IsUpdatedInRM failed
                            Report.clientIsUpdateRMUpdate++;
                        }
                        else
                        {
                            logger.Warn(client.ClientID + " : " + "Client Location Created in HPRM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Info("SQL Error : " + ex.Message);
                            //Increase Client Created IsUpdatedInRM failed
                            Report.clientIsUpdateRMCreate++;
                        }
                    }
                    //Consolidate Report 
                    if (IsForUpdate)
                    {
                        //Increase Client Updated counter
                        Report.clientUpdated++;
                    }
                    else
                    {
                        //Increase Client Created counter
                        Report.clientCreated++;
                    }
                }
                else
                {
                    //log that location is not created.
                    
                    if (IsForUpdate)
                    {
                        logger.Error(client.ClientID + " : " + "Client location could not be updated.");
                        //Increase Client not Updated counter
                        Report.clientNotUpdated++;
                    }
                    else if (UtilityException.Message.Contains("'ID Number' must be unique. Please enter a different ID Number"))
                    {
                        logger.Error(client.ClientID + " : " + "Client location could not be created.");
                        UpdateDuplicateClient(client.ClientID);
                        Report.duplicateClient++;
                    }
                    else
                    {
                        logger.Error(client.ClientID + " : " + "Client location could not be created.");
                        //Increase Client not Created counter
                        Report.clientNotCreated++;
                    }
                }
            }
        }

        /// <summary>
        /// Method to Creaet or update Client Records
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public void ClientRecordOperation(bool IsForUpdate)
        {
            //Initiate RMSataService Object
            if (RMDAL == null)
                RMDAL = new RMDataService();

            //Initiate SQLData service object
            if (sqlDAL == null)
                sqlDAL = new SQLDataService();

            //Create DataSet object
            DataSet ds = null;

            //Get the dataset of Client to be update
            try
            {
                ds = sqlDAL.GetClientRecordDataSet(IsForUpdate);
            }
            catch (Exception ex)
            {
                logger.Warn("While fetching the client record data from staging table SQL server error occurred");
                logger.Error("SQL Error : " + ex.Message);
                if (IsForUpdate)
                    Report.IsClientRecordUpdated = false;
                else
                    Report.IsClientRecordCreated = false;
                return;
            }

            //Create Client Record Object
            ClientRecord client = new ClientRecord();

            //Get the Data Table
            DataTable dt = ds.Tables[0];

            if (IsForUpdate)
            {
                //Total Client Record to be update
                Report.TotalClientRecordUpdate = dt.Rows.Count;
                Report.ClientRecordUpdated = 0;
                Report.ClientRecordNotUpdated = 0;
                Report.ClientRecordIsUpdateRMUpdate = 0;
            }
            else
            {
                //Total Client Record to be create
                Report.TotalClientRecordCreate = dt.Rows.Count;
                Report.ClientRecordCreated = 0;
                Report.ClientRecordNotCreated = 0;
                Report.DuplicateClientRecord = 0;
                Report.ClientRecordIsUpdateRMCreate = 0;
            }
            //Iterate through all rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //Get the client object from row
                client = CreateClientRecordObject(dt.Rows[i]);

                //Reset The UtilityException Message
                UtilityException.Message = string.Empty;

                //Create or update the Client Record
                if (RMDAL.ClientRecordOperation(client, IsForUpdate))
                {
                    //Update isUpdatedInRM field to true in staging table
                    try
                    {
                        sqlDAL.UpdateClientRecordStatus(client.ClientID);
                    }
                    catch (Exception ex)
                    {
                        if (IsForUpdate)
                        {
                            logger.Warn(client.ClientID + " : " + "Client Record Updated in HPCM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Info("SQL Error : " + ex.Message);
                            //Increase Client update IsUpdatedInRM failed
                            Report.ClientRecordIsUpdateRMUpdate++;
                        }
                        else
                        {
                            logger.Warn(client.ClientID + " : " + "Client Location Created in HPRM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Info("SQL Error : " + ex.Message);
                            //Increase Client Created IsUpdatedInRM failed
                            Report.ClientRecordIsUpdateRMCreate++;
                        }
                    }
                    //Consolidate Report 
                    if (IsForUpdate)
                    {
                        //Increase Client record Updated counter
                        Report.ClientRecordUpdated++;
                    }
                    else
                    {
                        //Increase Client Created counter
                        Report.ClientRecordCreated++;
                    }
                }
                else
                {
                    //log that location is not created.

                    if (IsForUpdate)
                    {
                        logger.Error(client.ClientID + " : " + "Client Record could not be updated.");
                        //Increase Client not Updated counter
                        Report.ClientRecordNotUpdated++;
                    }
                    else if (UtilityException.Message.Contains("Duplicate Record"))
                    {
                        logger.Error(client.ClientID + " : " + "Client Record could not be created.");
                        UpdateDuplicateClientRecord(client.ClientID);
                        Report.DuplicateClientRecord++;
                    }
                    else
                    {
                        logger.Error(client.ClientID + " : " + "Client Record could not be created.");
                        //Increase Client not Created counter
                        Report.ClientRecordNotCreated++;
                    }
                }
            }
        }

        /// <summary>
        /// Method to Creaet or update Employee Records
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public void EmployeeRecordOperation(bool IsForUpdate)
        {
            //Initiate RMSataService Object
            if (RMDAL == null)
                RMDAL = new RMDataService();

            //Initiate SQLData service object
            if (sqlDAL == null)
                sqlDAL = new SQLDataService();

            //Create DataSet object
            DataSet ds = null;

            //Get the dataset of Employee Record to be update
            try
            {
                ds = sqlDAL.GetEmployeeRecordDataSet(IsForUpdate);
            }
            catch (Exception ex)
            {
                logger.Warn("While fetching the Employee record data from staging table SQL server error occurred");
                logger.Error("SQL Error : " + ex.Message);
                if (IsForUpdate)
                    Report.IsEmployeeRecordUpdated = false;
                else
                    Report.IsEmployeeRecordCreated = false;
                return;
            }

            //Create Employee Record Object
            EmployeeRecord employee = new EmployeeRecord();

            //Get the Data Table
            DataTable dt = ds.Tables[0];

            if (IsForUpdate)
            {
                //Total Employee Record to be update
                Report.TotalEmployeeRecordUpdate = dt.Rows.Count;
                Report.EmployeeRecordUpdated = 0;
                Report.EmployeeRecordNotUpdated = 0;
                Report.EmployeeRecordIsUpdateRMUpdate = 0;
            }
            else
            {
                //Total Employee Record to be create
                Report.TotalEmployeeRecordCreate = dt.Rows.Count;
                Report.EmployeeRecordCreated = 0;
                Report.EmployeeRecordNotCreated = 0;
                Report.DuplicateEmployeeRecord = 0;
                Report.EmployeeRecordIsUpdateRMCreate = 0;
            }
            //Iterate through all rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //Get the Employee record object from row
                employee = CreateEmployeeRecordObject(dt.Rows[i]);

                //Reset The UtilityException Message
                UtilityException.Message = string.Empty;

                //Create or update the Employee Record
                if (RMDAL.EmployeeRecordOperation(employee, IsForUpdate))
                {
                    //Update isUpdatedInRM field to true in staging table
                    try
                    {
                        sqlDAL.UpdateEmployeeRecordStatus(employee.GPN);
                    }
                    catch (Exception ex)
                    {
                        if (IsForUpdate)
                        {
                            logger.Warn(employee.GPN + " : " + "Employee Record Updated in HPECM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Info("SQL Error : " + ex.Message);
                            //Increase Employee update IsUpdatedInRM failed
                            Report.EmployeeRecordIsUpdateRMUpdate++;
                        }
                        else
                        {
                            logger.Warn(employee.GPN + " : " + "Employee Record Created in HPECM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Info("SQL Error : " + ex.Message);
                            //Increase Employee Created IsUpdatedInRM failed
                            Report.EmployeeRecordIsUpdateRMCreate++;
                        }
                    }
                    //Consolidate Report 
                    if (IsForUpdate)
                    {
                        //Increase Employee record Updated counter
                        Report.EmployeeRecordUpdated++;
                    }
                    else
                    {
                        //Increase Employee Created counter
                        Report.EmployeeRecordCreated++;
                    }
                }
                else
                {
                    //log that location is not created.

                    if (IsForUpdate)
                    {
                        logger.Error(employee.GPN + " : " + "Employee Record could not be updated.");
                        //Increase Employee Record not Updated counter
                        Report.EmployeeRecordNotUpdated++;
                    }
                    else if (UtilityException.Message.Contains("Duplicate Record"))
                    {
                        logger.Error(employee.GPN + " : " + "Employee Record could not be created.");
                        UpdateDuplicateEmployeeRecord(employee.GPN);
                        Report.DuplicateEmployeeRecord++;
                    }
                    else
                    {
                        logger.Error(employee.GPN + " : " + "Employee Record could not be created.");
                        //Increase Employee Record not Created counter
                        Report.EmployeeRecordNotCreated++;
                    }
                }
            }
        }

        /// <summary>
        /// Method to Creaet or update Partner Records
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public void PartnerRecordOperation(bool IsForUpdate)
        {
            //Initiate RMSataService Object
            if (RMDAL == null)
                RMDAL = new RMDataService();

            //Initiate SQLData service object
            if (sqlDAL == null)
                sqlDAL = new SQLDataService();

            //Create DataSet object
            DataSet ds = null;

            //Get the dataset of Partner Record to be update
            try
            {
                ds = sqlDAL.GetPartnerRecordDataSet(IsForUpdate);
            }
            catch (Exception ex)
            {
                logger.Warn("While fetching the Partner record data from staging table SQL server error occurred");
                logger.Error("SQL Error : " + ex.Message);
                if (IsForUpdate)
                    Report.IsPartnerRecordUpdated = false;
                else
                    Report.IsPartnerRecordCreated = false;
                return;
            }

            //Create Partner Record Object
            PartnerRecord partner = new PartnerRecord();

            //Get the Data Table
            DataTable dt = ds.Tables[0];

            if (IsForUpdate)
            {
                //Total Partner Record to be update
                Report.TotalPartnerRecordUpdate = dt.Rows.Count;
                Report.PartnerRecordUpdated = 0;
                Report.PartnerRecordNotUpdated = 0;
                Report.PartnerRecordIsUpdateRMUpdate = 0;
            }
            else
            {
                //Total Partner Record to be create
                Report.TotalPartnerRecordCreate = dt.Rows.Count;
                Report.PartnerRecordCreated = 0;
                Report.PartnerRecordNotCreated = 0;
                Report.DuplicatePartnerRecord = 0;
                Report.PartnerRecordIsUpdateRMCreate = 0;
            }
            //Iterate through all rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //Get the Partner record object from row
                partner = CreatePartnerRecordObject(dt.Rows[i]);

                //Reset The UtilityException Message
                UtilityException.Message = string.Empty;

                //Create or update the Partner Record
                if (RMDAL.PartnerRecordOperation(partner, IsForUpdate))
                {
                    //Update isUpdatedInRM field to true in staging table
                    try
                    {
                        sqlDAL.UpdatePartnerRecordStatus(partner.GPN);
                    }
                    catch (Exception ex)
                    {
                        if (IsForUpdate)
                        {
                            logger.Warn(partner.GPN + " : " + "Partner Record Updated in HPECM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Info("SQL Error : " + ex.Message);
                            //Increase Partner update IsUpdatedInRM failed
                            Report.PartnerRecordIsUpdateRMUpdate++;
                        }
                        else
                        {
                            logger.Warn(partner.GPN + " : " + "Partner Record Created in HPECM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Info("SQL Error : " + ex.Message);
                            //Increase Partner Created IsUpdatedInRM failed
                            Report.PartnerRecordIsUpdateRMCreate++;
                        }
                    }
                    //Consolidate Report 
                    if (IsForUpdate)
                    {
                        //Increase Partner record Updated counter
                        Report.PartnerRecordUpdated++;
                    }
                    else
                    {
                        //Increase Partner Created counter
                        Report.PartnerRecordCreated++;
                    }
                }
                else
                {
                    //log that location is not created.

                    if (IsForUpdate)
                    {
                        logger.Error(partner.GPN + " : " + "Partner Record could not be updated.");
                        //Increase Partner Record not Updated counter
                        Report.PartnerRecordNotUpdated++;
                    }
                    else if (UtilityException.Message.Contains("Duplicate Record"))
                    {
                        logger.Error(partner.GPN + " : " + "Partner Record could not be created.");
                        UpdateDuplicatePartnerRecord(partner.GPN);
                        Report.DuplicatePartnerRecord++;
                    }
                    else
                    {
                        logger.Error(partner.GPN + " : " + "Partner Record could not be created.");
                        //Increase Partner Record not Created counter
                        Report.PartnerRecordNotCreated++;
                    }
                }
            }
        }

        /// <summary>
        /// Method to Creaet or update User Location
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public void UserLocationOperation(bool IsForUpdate)
        {
            //Intilize Logger
            logger = LogManager.GetLogger("Job");

            //Initiate RMSataService Object
            if (RMDAL == null)
                RMDAL = new RMDataService();

            //Initiate SQLData service object
            if (sqlDAL == null)
                sqlDAL = new SQLDataService();

            //Create Dataset object
            DataSet ds = null;

            //Get the dataset of Client to be update
            try
            {
                ds = sqlDAL.GetUserSet(IsForUpdate);
            }
            catch (Exception ex)
            {
                logger.Warn("While fetching the user data from staging table SQL server error occurred");
                logger.Error("SQL Error : " + ex.Message);
                if (IsForUpdate)
                    Report.IsUserUpdated = false;
                else
                    Report.IsUserCreated = false;
                return;
            }
            //Create Client Location Object
            UserLocation user = new UserLocation();

            //Get the Data Table
            DataTable dt = ds.Tables[0];

            if (IsForUpdate)
            {
                //Total User Location to be update
                Report.totalUserUpdate = dt.Rows.Count;
                Report.userUpdated = 0;
                Report.userNotUpdated = 0;
                Report.userIsUpdateRMUpdate = 0;
            }
            else
            { 
                //Total user Location to be create
                Report.totalUserCreate = dt.Rows.Count;
                Report.userCreated = 0;
                Report.userNotCreated = 0;
                Report.duplicateUser = 0;
                Report.userIsUpdateRMCreate = 0;
            }
            //Iterate through all rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //Get the client object from row
                user = CreateUserLocObject(dt.Rows[i]);

                //Reset The UtilityException Message
                UtilityException.Message = string.Empty;

                //Create or Update the Client Location
                if (RMDAL.UserOperation(user, IsForUpdate))
                {
                    //Update isUpdatedInRM field to true in staging table
                    try
                    {
                        sqlDAL.UpdateUserLocStatus(user.GPN);
                    }
                    catch (Exception ex)
                    {
                        
                        if (IsForUpdate)
                        {
                            logger.Warn(user.GPN + " : " + "User Location Updated in HPRM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Error("SQL Error : " + ex.Message);
                            //Increase User Updated IsUpatedInRM failed counter
                            Report.userIsUpdateRMUpdate++;
                        }
                        else
                        {
                            logger.Warn(user.GPN + " : " + "User Location Created in HPRM but flag IsUpdatedInRM is not set to true due to below error.");
                            logger.Error("SQL Error : " + ex.Message);
                            //Increase User Created IsUpatedInRM failed counter
                            Report.userIsUpdateRMCreate++;
                        }

                    }
                    if (IsForUpdate)
                    {
                        //Increase User Updated counter
                        Report.userUpdated++;
                    }
                    else
                    {
                        //Increase User Created counter
                        Report.userCreated++;
                    }
                }
                else
                {
                    //log that location is not created.
                    
                    if (IsForUpdate)
                    {
                        logger.Error(user.GPN + " : " + "User location could not be updated.");
                        //Increase User not Updated counter
                        Report.userNotUpdated++;
                    }
                    //We are reading specific exception message to determine duplicates, this method is adopted 
                    //so that we don't have to check every location for duplicates.
                    else if (UtilityException.Message.Contains("'ID Number' must be unique. Please enter a different ID Number"))
                    {
                        logger.Error(user.GPN + " : " + "User location could not be created.");
                        UpdateDuplicateUser(user.GPN);
                        Report.duplicateUser++;
                    }
                    else
                    {
                        logger.Error(user.GPN + " : " + "User location could not be created.");
                        //Increase User not Created counter
                        Report.userNotCreated++;
                    }
                }
            }
        }

        /// <summary>
        /// Method to Check Data is ready for processing
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool</returns>
        public bool IsDataReadyFor(string interfaceType)
        {
            try
            {
                return sqlDAL.IsDataReadyFor(interfaceType);
            }
            catch (Exception ex)
            {
                logger.Warn("Sql error has occured while checking weather "+ interfaceType+" data is ready or not");
                logger.Error("SQL Error : " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Method to Check SQL Connection
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool</returns>
        public bool IsSQLConnected()
        {
            sqlDAL = new SQLDataService();
            return true;
        }
        /// <summary>
        /// Method to Check HP RM Connection
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool</returns>
        public bool IsHPRMConnected()
        {
            RMDAL = new RMDataService();
            return true;
        }

        /// <summary>
        /// Method to Upadate IsReadyForProcessing in Client/User Table
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void ProcessOverFor(string interfaceType)
        {
            try
            {
                sqlDAL.UpdateProcessStatus(interfaceType);
            }
            catch (Exception ex)
            {
                logger.Warn("Though the " + interfaceType + " data has been processed successfully in HP RM, but after processing sync utility could not set flags 'IsReadyForProcessing' and 'SyncLastRunTime'. Please set these flags manually in 'StatusTable' with values False and " + DateTime.Now + " respectively.");
                logger.Error(interfaceType + " : " + ex.Message);

                switch(interfaceType)
                {
                    case "Client" :
                        Report.IsClientStatusTableUpdated = false;
                        break;
                    case "User":
                        Report.IsUserStatusTableUpdated = false;
                        break;
                    case "EmployeeRecord":
                        Report.IsEmployeeRecordStatusTableUpdated = false;
                        break;
                    case "PartnerRecord":
                        Report.IsPartnerRecordStatusTableUpdated = false;
                        break;
                    case "ClientRecord":
                        Report.IsClientRecordStatusTableUpdated = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Method to Close the connections
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void CloseConnections()
        {
            //Close SQL connection
            sqlDAL.CloseConnection();

            //Close HPRM Connection
            RMDAL.CloseConnection();
        }

        /// <summary>
        /// Method to Update duplicate User 
        /// </summary>
        /// <param name="">GPN</param>
        /// <returns></returns>
        public void UpdateDuplicateUser(string GPN)
        {
            sqlDAL.UpdateDuplicateUser(GPN);
        }

        /// <summary>
        /// Method to Update duplicate Employee Record 
        /// </summary>
        /// <param name="">GPN</param>
        /// <returns></returns>
        public void UpdateDuplicateEmployeeRecord(string GPN)
        {
            sqlDAL.UpdateDuplicateEmployeeRecord(GPN);
        }

        /// <summary>
        /// Method to Update duplicate Partner Record 
        /// </summary>
        /// <param name="">GPN</param>
        /// <returns></returns>
        public void UpdateDuplicatePartnerRecord(string GPN)
        {
            sqlDAL.UpdateDuplicatePartnerRecord(GPN);
        }

        /// <summary>
        /// Method to Update duplicate Client 
        /// </summary>
        /// <param name="">ClientId</param>
        /// <returns></returns>
        public void UpdateDuplicateClient(string ClientId)
        {
            sqlDAL.UpdateDuplicateClient(ClientId);
        }

        /// <summary>
        /// Method to Update duplicate Client Record 
        /// </summary>
        /// <param name="">ClientId</param>
        /// <returns></returns>
        public void UpdateDuplicateClientRecord(string ClientId)
        {
            sqlDAL.UpdateDuplicateClientRecord(ClientId);
        }

        /// <summary>
        /// Method to Intialize flag to true
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void InitializeFlag()
        {
            Report.IsClientCreated = true;
            Report.IsClientStatusTableUpdated = true;
            Report.IsClientUpdated = true;
            Report.IsUserCreated = true;
            Report.IsUserStatusTableUpdated = true;
            Report.IsUserUpdated = true;
            Report.IsEmployeeRecordCreated = true;
            Report.IsEmployeeRecordStatusTableUpdated = true;
            Report.IsEmployeeRecordUpdated = true;
            Report.IsPartnerRecordCreated = true;
            Report.IsPartnerRecordStatusTableUpdated = true;
            Report.IsPartnerRecordUpdated = true;
            Report.IsClientRecordCreated = true;
            Report.IsClientRecordStatusTableUpdated = true;
            Report.IsClientRecordUpdated = true;

        }
        #endregion

        #region private method

        /// <summary>
        /// Method to Creaet Client details object
        /// </summary>
        /// <param name="ClientLocation,DataRow"></param>
        /// <returns></returns>

        private ClientLocation CreateClientLocObject(DataRow dr)
        {
            //Create ClientLocation object
            ClientLocation client = new ClientLocation();

            //get ClientId
            client.ClientID = dr["ClientID"].ToString();

            //Get Client Name
            client.Name = dr["ClientName"].ToString();

            //return client object
            return client;
        }
        
        /// <summary>
        /// Method to Creat User details object
        /// </summary>
        /// <param name="ClientLocation,DataRow"></param>
        /// <returns></returns>

        private UserLocation CreateUserLocObject(DataRow dr)
        {
            //Create UserLocation object
            UserLocation user = new UserLocation();

            //set User GPN
            user.GPN = dr["GPN"].ToString();

            //Set user firstName
            user.FirstName = dr["FirstName"].ToString();

            //Set Preferred Name
            user.PreferredName = dr["PreferredName"].ToString();

            //Set Middel Name
            user.MiddleName = dr["MiddleName"].ToString();

            //Set user LastName
            user.LastName = dr["LastName"].ToString();

            //set user EYEmailAddress
            user.SMTPEmail = dr["EmailAddress"].ToString();

            //set user EYNetworkLogin
            user.NetworkLogin = dr["NetworkLogin"].ToString();

            //set Domain Name
            user.DomainName = dr["DomainName"].ToString();

            //set user ServiceLine
            user.ServiceLine = dr["ServiceLine"].ToString();

            //set user SubServiceLine
            user.SubServiceLine = dr["SubServiceLine"].ToString();

            //set user Location
            user.Location = dr["Location"].ToString();

            //set user StartDate
            user.FromDate = dr["StartDate"].ToString();

            //set user TerminatedDate
            user.ToDate = dr["TerminatedDate"].ToString();

            //return User
            return user;
        }

        /// <summary>
        /// Method to Creat Employee Record details object
        /// </summary>
        /// <param name="DataRow"></param>
        /// <returns></returns>

        private EmployeeRecord CreateEmployeeRecordObject(DataRow dr)
        {
            //Create EmployeeRecord Object
            EmployeeRecord employee = new EmployeeRecord() {

                //Set PrimaryName
                PrimaryName = dr["PrimaryName"].ToString(),
            
                //Set Employee Type
               EmployeeType = dr["EmployeeType"].ToString(),

                //Set Employee Rank
               EyRank  = dr["EYRank"].ToString(),

                //Set Gender
               Gender = dr["Gender"].ToString(),

               //Set Service line
               ServiceLine = dr["ServiceLine"].ToString(),

               //Set Sub Service line
               SubServiceLine = dr["SubServiceLine"].ToString(),

               //Set Company Seniority Date date
               CompanySeniorityDate = dr["CompanySeniorityDate"].ToString(),

               //Set Termination Date
               TerminationDate = dr["TerminationDate"].ToString(),

               //Set GPN
               GPN = dr["GPN"].ToString(),

                //Set LPN
               LPN = dr["LPN"].ToString(),

                //Set EmplID
               EmplID = dr["EmplID"].ToString(),

                //Set GUI
               GUI = dr["GUI"].ToString(),

                //Set Pay Group
               PayGroup = dr["Paygroup"].ToString(),

               //Set Code block
               CodeBlock = dr["Codeblock"].ToString(),

               //Set CostCentre
               CostCentre = dr["CostCentre"].ToString(),

               //Set CompanyCode
               CompanyCode = dr["CompanyCode"].ToString(),

               //Set BusinessUnit
               BusinessUnit = dr["BusinessUnit"].ToString(),

               //Set Orginisation Unit
               OrganisationalUnit = dr["OrganisationalUnit"].ToString(),

               //Set Management Unit
               ManagementUnit = dr["ManagementUnit"].ToString(),

               //Set Sub Management Unit
               SubManagementUnit = dr["SubManagementUnit"].ToString(),

                //Set HRStatus
                HRStatus = dr["HRStatus"].ToString(),

                //Set RehireDate
                RehireDate = dr["RehireDate"].ToString(),

                //Set 
                DeptID = dr["DeptID"].ToString(),

                //Set 
                GeographicCountry = dr["GeographicCountry"].ToString()
            };

            //return Employee
            return employee;
        }

        /// <summary>
        /// Method to Creat Partner Record details object
        /// </summary>
        /// <param name="DataRow"></param>
        /// <returns></returns>

        private PartnerRecord CreatePartnerRecordObject(DataRow dr)
        {
            //Create PartnerRecord Object
            PartnerRecord partner = new PartnerRecord()
            {
                //Set PrimaryName
                PrimaryName = dr["PrimaryName"].ToString(),

                //Set Gender
                Gender = dr["Gender"].ToString(),

                //Set Service line
                ServiceLine = dr["ServiceLine"].ToString(),

                //Set Sub Service line
                SubServiceLine = dr["SubServiceLine"].ToString(),

                //Set Original Hire date
                OriginalHireDate = dr["OriginalHireDate"].ToString(),

                //Set Termination Date
                TerminationDate = dr["TerminationDate"].ToString(),

                //Set GPN
                GPN = dr["GPN"].ToString(),

                //Set LPN
                LPN = dr["LPN"].ToString(),

                //Set EmplID
                EmplID = dr["EmplID"].ToString(),

                //Set GUI
                GUI = dr["GUI"].ToString(),

                //Set Pay Group
                PayGroup = dr["Paygroup"].ToString(),

                //Set Code block
                CodeBlock = dr["Codeblock"].ToString(),

                //Set CostCentre
                CostCentre = dr["CostCentre"].ToString(),

                //Set CompanyCode
                CompanyCode = dr["CompanyCode"].ToString(),

                //Set BusinessUnit
                BusinessUnit = dr["BusinessUnit"].ToString(),

                //Set Orginisation Unit
                OrganisationalUnit = dr["OrganisationalUnit"].ToString(),

                //Set Management Unit
                ManagementUnit = dr["ManagementUnit"].ToString(),

                //Set Sub Management Unit
                SubManagementUnit = dr["SubManagementUnit"].ToString(),

                //Set HRStatus
                HRStatus = dr["HRStatus"].ToString()
            };

            //return partner
            return partner;
        }

        /// <summary>
        /// Method to Creat Client Record details object
        /// </summary>
        /// <param name="DataRow"></param>
        /// <returns></returns>

        private ClientRecord CreateClientRecordObject(DataRow dr)
        {
            //Create ClientRecord Object
            ClientRecord client = new ClientRecord()
            {
                //Set Client Id
                ClientID = dr["ClientID"].ToString(),

                //Set Partner 1
                Partner1 = dr["PartnerName1"].ToString(),

                //Set Partner 1
                Partner2 = dr["PartnerName2"].ToString(),

                //Set InactiveDate
                InactiveDate = dr["EffectiveDate"].ToString(),

                //Set ClientStatus
                ClientStatus = dr["ClientStatus"].ToString()
            };

            //return client
            return client;
        }

        #endregion
    }

}
