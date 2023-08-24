using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace machineFilesInfo
{
    class fileDataBaseAccess
    {

        public List<FileInformation> GetFileInformation()
        {
            List<FileInformation> files = new List<FileInformation>();

            string query = "select * from machineFileInfo order by fileName";
            SqlConnection conn = ConnectionManager.GetConnection();
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader = default(SqlDataReader);

            try
            {
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    FileInformation file = new FileInformation();
                    file.FileName = reader["fileName"].ToString().Trim();
                    file.FileType = reader["fileType"].ToString().Trim();
                    file.FolderPath = reader["filePath"].ToString().Trim();
                    file.FileSize = Int32.Parse(reader["fileSize"].ToString());
                    file.CreatedDate = (DateTime)reader["fileDateCreated"];
                    file.ModifiedDate = (DateTime)reader["provenModifiedDate"];
                    file.Owner = reader["fileOwner"].ToString().Trim();
                    file.ComputerName = reader["computer"].ToString().Trim();
                    files.Add(file);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (conn != null) conn.Close();
            }

            return files;
        }

        //string query = "Insert into machineFileInfo(fileName, fileType, filePath, fileSize, fileDateCreated, fileDateModified, fileOwner, computer)" +
        //                   "values  (@fileName, @fileType, @folder, @fileSize, @createdDate, @modifiedDate , @owner, @computerName)";


        public void InsertIntoDatabase(FileInformation local)
        {
            string Cdate = local.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            string Mdate = local.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            string insertQry = "Insert into machineFileInfo(fileName, fileType, filePath, fileSize, fileDateCreated, provenModifiedDate, fileOwner, computer)  " +
                                                "values  (@file_Name, @file_Type, @folder, @file_Size, @created_Date, @modified_Date , @owner, @computer_Name)";

            SqlConnection conn = ConnectionManager.GetConnection();

            using (SqlCommand cmd = new SqlCommand(insertQry, conn))
            {
                cmd.Parameters.AddWithValue("@file_Name", local.FileName);
                cmd.Parameters.AddWithValue("@file_Type", local.FileType);
                cmd.Parameters.AddWithValue("@folder", local.FolderPath);
                cmd.Parameters.AddWithValue("@file_Size", local.FileSize);
                cmd.Parameters.AddWithValue("@created_Date", Cdate);
                cmd.Parameters.AddWithValue("@modified_Date", Mdate);
                cmd.Parameters.AddWithValue("@owner", local.Owner);
                cmd.Parameters.AddWithValue("@computer_Name", local.ComputerName);

                cmd.ExecuteNonQuery();

                Logger.WriteExtraLog($"File {local.FileName} information inserted into the database." + DateTime.Now);
            }
        }
        public void updateDatabaseStandard(FileInformation File, FileInformation File2)
        {
            //int val = int.Parse(File.ModifiedDate.ToString().Equals(File2.ModifiedDate.ToString()).ToString());
            int val = 0;
            if (File.ModifiedDate.ToString().Equals(File2.ModifiedDate.ToString()))
            {
                val = 1;
            }
            string date = File.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            //remove last folder in folder path 
            string folder = File.FolderPath.Substring(0, File.FolderPath.LastIndexOf('\\'));
            string updateQry = $"UPDATE machineFileInfo SET storedModifiedDate = '{date}', isModified = {val} WHERE fileName = '{File.FileName}' and filePath like '{folder}%'";
            SqlConnection conn = ConnectionManager.GetConnection();

            using (SqlCommand cmd = new SqlCommand(updateQry, conn))
            {
                cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {File.FileName} information updated in  database." + DateTime.Now);
            }
        }
        public void updateDatabaseProven(FileInformation PFile)
        {
            int val = 0;
            string date = PFile.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            SqlConnection conn = ConnectionManager.GetConnection();
            string updateQry = "UPDATE machineFileInfo SET provenModifiedDate = @date, isModified = @val WHERE fileName = @fileName AND filePath = @filePath";

            using (SqlCommand cmd = new SqlCommand(updateQry, conn))
            {
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@val", val);
                cmd.Parameters.AddWithValue("@fileName", PFile.FileName);
                cmd.Parameters.AddWithValue("@filePath", PFile.FolderPath);

                cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {PFile.FileName} information updated in database." + DateTime.Now);
            }

        }
    }
}
