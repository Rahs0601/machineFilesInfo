using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading;

namespace machineFilesInfo
{
    public static class ConnectionManager
    {
        private static readonly string conString = ConfigurationManager.ConnectionStrings["SQLConnectionString"].ToString();
        private static readonly string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static SqlConnection GetConnection()
        {
            bool writeDown = false;
            DateTime dt = DateTime.Now;
            SqlConnection conn = new SqlConnection(conString);
            do
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    if (writeDown == false)
                    {
                        dt = DateTime.Now.AddHours(2);
                        Logger.WriteErrorLog(ex.Message);
                        writeDown = true;
                    }
                    if (dt < DateTime.Now)
                    {
                        Logger.WriteErrorLog(ex.Message);
                        writeDown = false;
                    }
                    Thread.Sleep(1000);
                }
            } while (conn.State != ConnectionState.Open);
            return conn;
        }
    }
}