using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

//genericized data layer so conversion to Sql Server is not difficult.
namespace ReviewRetriever
{
    class DbUtil
    {

        //returns true if connection was successful.
        public int GetSqlDataSet(string sql, string connectionString, ref DataSet ds, string tableName = "QueryResults")
        {
            SqlConnection sqliteConn;
            SqlDataAdapter adap;
            try
            {
                sqliteConn = CreateConnection(connectionString);
                adap = new SqlDataAdapter(sql, sqliteConn);
                adap.Fill(ds);
                sqliteConn.Close();
            }
            catch (Exception e)
            {
                Logging.Log("GetSqliteDataSet() -> Error executing sql: " + e.Message, LogLevel.LogFile);
            }
            if (ds.Tables.Count > 0)
                return ds.Tables[0].Rows.Count;
            return 0;
        }

        public bool ExecuteSql(string sql, string connectionString) 
        {
            bool bRet = false;
 
            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {

                using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConn))
                {
                    try
                    {
                        sqlConn.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlConn.Close();
                        bRet = true;
                    }
                    catch (Exception e)
                    {
                        Logging.Log("ExecuteSql() -> Error Executing non-query " + e.Message, LogLevel.LogFile);
                        bRet = false;
                    }
                    finally 
                    {
                        sqlConn.Close();
                    } 
                }               
            }
            return bRet;
        }

        static SqlConnection CreateConnection(string connectionString)
        {

            SqlConnection sqlite_conn;
            sqlite_conn = new SqlConnection(connectionString);
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Logging.Log("CreateConnection() -> Error establishing a connection to " + connectionString, LogLevel.LogFile);
            }
            return sqlite_conn;
        }
    }
}
