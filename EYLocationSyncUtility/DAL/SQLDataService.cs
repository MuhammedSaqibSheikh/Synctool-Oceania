using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EYLocationSyncUtility.DAL
{
    /// <summary>
    /// Provides public methods to perform database related tasks
    /// </summary>
    public class SQLDataService
    {
        #region Private member

        // Sql conneciton to Database
        private SqlConnection dbConnection;
        private DateTime SyncLastRunDate;
        private bool IsSyncLastRunDateEmpty = false;
        #endregion

        #region Constructor

        /// <summary>
        /// Create the conneciton to Database
        /// </summary>
        public SQLDataService()
        {
            // Creating connection to the database
            dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ToString());

            // Openning the connection to the database
            dbConnection.Open();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Return the Client Set
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public DataSet GetClientSet(bool IsForUpdate)
        {
            string selectQuery = "";

            //Select query to get client details to be update/create
            selectQuery = "Select * From RMS.v_Client where  ";
            if (!IsSyncLastRunDateEmpty)
            {
                selectQuery = selectQuery + "EntryDate>='" + SyncLastRunDate + "' AND ";
            }
            selectQuery = selectQuery + "IsForUpdate=" + (IsForUpdate == true ? "1" : "0") + " AND IsUpdatedInRM=0";

            //Execute select query to get result set
            return ExecuteSelectQuery(selectQuery);
        }

        /// <summary>
        /// Return the User Set
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public DataSet GetUserSet(bool IsForUpdate)
        {
            string selectQuery = "";

            //Select query to get client details to be update
            selectQuery = "Select * From RMS.v_Employee where ";
            if (!IsSyncLastRunDateEmpty)
            {
                selectQuery = selectQuery + "EntryDate>='" + SyncLastRunDate + "' AND ";
            }
            selectQuery = selectQuery + "IsForUpdate=" + (IsForUpdate == true ? "1" : "0") + "  AND IsUpdatedInRM=0";

            //Execute select query to get result set
            return ExecuteSelectQuery(selectQuery);
        }

        /// <summary>
        /// Return the Employee Record DataSet
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public DataSet GetEmployeeRecordDataSet(bool IsForUpdate)
        {
            string selectQuery = "";

            //Select query to get client details to be update
            selectQuery = "Select * From RMS.v_EmployeeRecord where ";
            if (!IsSyncLastRunDateEmpty)
            {
                selectQuery = selectQuery + "EntryDate>='" + SyncLastRunDate + "' AND ";
            }
            selectQuery = selectQuery + "IsForUpdate=" + (IsForUpdate == true ? "1" : "0") + "  AND IsUpdatedInRM=0";

            //Execute select query to get result set
            return ExecuteSelectQuery(selectQuery);
        }

        /// <summary>
        /// Return the Partner Record DataSet
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public DataSet GetPartnerRecordDataSet(bool IsForUpdate)
        {
            string selectQuery = "";

            //Select query to get client details to be update
            selectQuery = "Select * From RMS.v_PartnerRecord where ";
            if (!IsSyncLastRunDateEmpty)
            {
                selectQuery = selectQuery + "EntryDate>='" + SyncLastRunDate + "' AND ";
            }
            selectQuery = selectQuery + "IsForUpdate=" + (IsForUpdate == true ? "1" : "0") + "  AND IsUpdatedInRM=0";

            //Execute select query to get result set
            return ExecuteSelectQuery(selectQuery);
        }

        /// <summary>
        /// Return the Client Record DataSet
        /// </summary>
        /// <param name="IsForUpdate"></param>
        /// <returns></returns>
        public DataSet GetClientRecordDataSet(bool IsForUpdate)
        {
            string selectQuery = "";

            //Select query to get client details to be update
            selectQuery = "Select * From RMS.v_ClientRecord where ";
            if (!IsSyncLastRunDateEmpty)
            {
                selectQuery = selectQuery + "EntryDate>='" + SyncLastRunDate + "' AND ";
            }
            selectQuery = selectQuery + "IsForUpdate=" + (IsForUpdate == true ? "1" : "0") + "  AND IsUpdatedInRM=0";

            //Execute select query to get result set
            return ExecuteSelectQuery(selectQuery);
        }

        /// <summary>
        /// Closes the connection to Database
        /// </summary>
        public void CloseConnection()
        {
            dbConnection.Close();
        }

        /// <summary>
        /// Update in staging table that Client Location is upadted/Craeted
        /// </summary>
        /// <param name="ClientId"></param>
        public void UpdateClientLocStatus(string ClientId)
        {
            //update query
            // added ' before n after ClientID  By Ketan on 3-02-2017
            string query = "UPDATE RMS.v_Client Set IsUpdatedInRM=1 where ClientID='" + ClientId +"'";

            //Execute Update query
            ExecuteUpdateQuery(query);
        }

        /// <summary>
        /// Update in staging table that Client Record is upadted/Craeted
        /// </summary>
        /// <param name="ClientId"></param>
        public void UpdateClientRecordStatus(string ClientId)
        {
            //update query
            string query = "UPDATE RMS.v_ClientRecord Set IsUpdatedInRM=1 where ClientID='" + ClientId + "'";

            //Execute Update query
            ExecuteUpdateQuery(query);
        }

        /// <summary>
        /// Update in staging table that user Location is upadted/Craeted
        /// </summary>
        /// <param name="GPN"></param>
        public void UpdateUserLocStatus(string GPN)
        {
            //update query
            string query = "UPDATE RMS.v_Employee Set IsUpdatedInRM=1 where GPN='" + GPN + "'";

            //Execute Update query
            ExecuteUpdateQuery(query);
        }

        /// <summary>
        /// Update in staging table that Employee Record is upadted/Craeted
        /// </summary>
        /// <param name="GPN"></param>
        public void UpdateEmployeeRecordStatus(string GPN)
        {
            //update query
            string query = "UPDATE RMS.v_EmployeeRecord Set IsUpdatedInRM=1 where GPN='" + GPN + "'";

            //Execute Update query
            ExecuteUpdateQuery(query);
        }

        /// <summary>
        /// Update in staging table that Partner Record is upadted/Craeted
        /// </summary>
        /// <param name="GPN"></param>
        public void UpdatePartnerRecordStatus(string GPN)
        {
            //update query
            string query = "UPDATE RMS.v_PartnerRecord Set IsUpdatedInRM=1 where GPN='" + GPN + "'";

            //Execute Update query
            ExecuteUpdateQuery(query);
        }

        /// <summary>
        /// Update in Status table that Client/User processing is over
        /// </summary>
        /// <param name="interfaceType"></param>
        public void UpdateProcessStatus(string interfaceType)
        {
            //update query
            string query = "UPDATE RMS.v_Status Set IsReadyForProcessing=0,SyncLastRunTime='" + DateTime.Now + "' where InterfaceType='" + interfaceType + "'";

            //Execute Update query
            ExecuteUpdateQuery(query);
        }

        /// <summary>
        /// Method to Check Client is ready for processing
        /// </summary>
        /// <param name="">InterfaceType</param>
        /// <returns>bool</returns>
        public bool IsDataReadyFor(string interfaceType)
        {
            return IsReadyForProcessing(interfaceType);
        }

        /// <summary>
        /// Method to Update duplicate User 
        /// </summary>
        /// <param name="">GPN</param>
        /// <returns></returns>
        public void UpdateDuplicateUser(string GPN)
        {
            //update query
            string query = "UPDATE RMS.v_Employee Set IsUpdatedInRM=0,IsForUpdate=1 where GPN='" + GPN +"'";

            //Execute Update query
            ExecuteUpdateQuery(query);

        }

        /// <summary>
        /// Method to Update duplicate Client 
        /// </summary>
        /// <param name="">ClientId</param>
        /// <returns></returns>
        public void UpdateDuplicateClient(string ClientId)
        {
            //update query
            string query = "UPDATE RMS.v_Client Set IsUpdatedInRM=0,IsForUpdate=1 where ClientID='" + ClientId +"'";

            //Execute Update query
            ExecuteUpdateQuery(query);

        }

        /// <summary>
        /// Method to Update duplicate Client Record
        /// </summary>
        /// <param name="">ClientId</param>
        /// <returns></returns>
        public void UpdateDuplicateClientRecord(string ClientId)
        {
            //update query
            string query = "UPDATE RMS.v_ClientRecord Set IsUpdatedInRM=0,IsForUpdate=1 where ClientID='" + ClientId +"'";

            //Execute Update query
            ExecuteUpdateQuery(query);

        }

        /// <summary>
        /// Method to Update duplicate Partner Record
        /// </summary>
        /// <param name="">GPN</param>
        /// <returns></returns>
        public void UpdateDuplicatePartnerRecord(string GPN)
        {
            //update query
            string query = "UPDATE RMS.v_PartnerRecord Set IsUpdatedInRM=0,IsForUpdate=1 where GPN='" + GPN + "'";

            //Execute Update query
            ExecuteUpdateQuery(query);

        }

        /// <summary>
        /// Method to Update duplicate Employee Record
        /// </summary>
        /// <param name="">GPN</param>
        /// <returns></returns>
        public void UpdateDuplicateEmployeeRecord(string GPN)
        {
            //update query
            string query = "UPDATE RMS.v_EmployeeRecord Set IsUpdatedInRM=0,IsForUpdate=1 where GPN='" + GPN + "'";

            //Execute Update query
            ExecuteUpdateQuery(query);

        }
        #endregion

        #region Helper method

        /// <summary>
        /// Executes Select Query and returns the result
        /// </summary>
        /// <param name="select query"></param>
        /// <returns></returns>
        private DataSet ExecuteSelectQuery(string selectQuery)
        {
            // Empty Dataset
            DataSet result = new DataSet();

            // Creating Command
            SqlCommand cmdRetrieve = new SqlCommand();
            cmdRetrieve.CommandType = CommandType.Text;
            cmdRetrieve.CommandText = selectQuery;
            cmdRetrieve.Connection = dbConnection;

            // Creating the Adapter
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = cmdRetrieve;

            // Fill the Dataset
            adapter.Fill(result);

            // Return the result
            return result;
        }
        /// <summary>
        /// Executes Update Query
        /// </summary>
        /// <param name="select query"></param>
        /// <returns></returns>
        private bool ExecuteUpdateQuery(string updateQuery)
        {
            // Empty Dataset
            DataSet result = new DataSet();

            // Creating Command
            SqlCommand cmdRetrieve = new SqlCommand();
            cmdRetrieve.CommandType = CommandType.Text;
            cmdRetrieve.CommandText = updateQuery;
            cmdRetrieve.Connection = dbConnection;

            // Executing command
            if (cmdRetrieve.ExecuteNonQuery() == 1)
            {
                Console.WriteLine("Command executed successfully.");
                return true;
            }
            else
            {
                Console.WriteLine("Command failed to execute.");
                return false;
            }
            
        }

        /// <summary>
        /// Get bool value weather interface type is redy for processing
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns>bool</returns>
        private bool IsReadyForProcessing(string interfaceType)
        {
            //Select query to get the Last run date
            string query = "Select * from RMS.v_Status where InterfaceType='" + interfaceType + "'";

            //Get datetime from the dataset
            DataSet ds = ExecuteSelectQuery(query);

            //Checking SynchLastRunTime value
            if (!String.IsNullOrEmpty(ds.Tables[0].Rows[0]["SyncLastRunTime"].ToString()))
            {
                SyncLastRunDate = DateTime.Parse(ds.Tables[0].Rows[0]["SyncLastRunTime"].ToString());
                IsSyncLastRunDateEmpty=false;
            }
            else
            {
                IsSyncLastRunDateEmpty = true;
            }
            bool flag = Boolean.Parse(ds.Tables[0].Rows[0]["IsReadyForProcessing"].ToString());

            //return datetime
            return flag;

        }
        #endregion
    }
}
