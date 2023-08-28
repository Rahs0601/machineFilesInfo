using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace machineFilesInfo
{
    internal class fileDataBaseAccess
    {
        public List<FileInformation> GetFileInformation()
        {
            List<FileInformation> files = new List<FileInformation>();

            string query = "select * from machineFileInfo";
            SqlConnection conn = ConnectionManager.GetConnection();
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader = default;

            try
            {
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    FileInformation file = new FileInformation
                    {
                        FileName = reader["fileName"].ToString().Trim(),
                        FileType = reader["fileType"].ToString().Trim(),
                        FolderPath = reader["filePath"].ToString().Trim(),
                        FileSize = int.Parse(reader["fileSize"].ToString()),
                        CreatedDate = (DateTime)reader["fileDateCreated"],
                        ModifiedDate = (DateTime)reader["StandardModifiedDate"],
                        Owner = reader["fileOwner"].ToString().Trim(),
                        ComputerName = reader["computer"].ToString().Trim()
                    };
                    files.Add(file);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
            finally
            {
                reader?.Close();
                conn?.Close();
            }

            return files;
        }

        //string query = "Insert into machineFileInfo(fileName, fileType, filePath, fileSize, fileDateCreated, fileDateModified, fileOwner, computer)" +
        //                   "values  (@fileName, @fileType, @folder, @fileSize, @createdDate, @modifiedDate , @owner, @computerName)";

        public void InsertIntoDatabase(FileInformation local, SqlConnection conn)
        {
            string operation = local.FolderPath.Split('\\').Reverse().Skip(1).First();
            string component = local.FolderPath.Split('\\').Reverse().Skip(3).First();
            string opid = operation.Split('_').First();
            string opdescription = operation.Split('_').Last();
            string Cdate = local.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            string Mdate = local.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            string insertQry = "Insert into machineFileInfo(fileName, fileType, filePath, fileSize, fileDateCreated, StandardModifiedDate, fileOwner, computer , operationID  ,operationDespcription , component)  " +
                                                "values  (@file_Name, @file_Type, @folder, @file_Size, @created_Date, @modified_Date , @owner, @computer_Name ,@opId , @opDescription , @component)";

            using (SqlCommand cmd = new SqlCommand(insertQry, conn))
            {
                _ = cmd.Parameters.AddWithValue("@file_Name", local.FileName);
                _ = cmd.Parameters.AddWithValue("@file_Type", local.FileType);
                _ = cmd.Parameters.AddWithValue("@folder", local.FolderPath);
                _ = cmd.Parameters.AddWithValue("@file_Size", local.FileSize);
                _ = cmd.Parameters.AddWithValue("@created_Date", Cdate);
                _ = cmd.Parameters.AddWithValue("@modified_Date", Mdate);
                _ = cmd.Parameters.AddWithValue("@owner", local.Owner);
                _ = cmd.Parameters.AddWithValue("@computer_Name", local.ComputerName);
                _ = cmd.Parameters.AddWithValue("@opId", opid);
                _ = cmd.Parameters.AddWithValue("@opDescription", opdescription);
                _ = cmd.Parameters.AddWithValue("@component", component);

                _ = cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {local.FileName} information inserted into the database." + DateTime.Now);
            }
        }

        public void updateDatabaseProven(FileInformation pFile, FileInformation sFile, SqlConnection conn)
        {
            int val = 0;
            string date = null;
            string folder = sFile.FolderPath.Substring(0, sFile.FolderPath.LastIndexOf('\\'));
            string file = sFile.FileName;
            if (pFile != null)
            {
                if (pFile.ModifiedDate.ToString().Equals(sFile.ModifiedDate.ToString()))
                {
                    val = 1;
                }
                date = pFile.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
                folder = pFile.FolderPath.Substring(0, pFile.FolderPath.LastIndexOf('\\'));
                file = pFile.FileName;
            }

            string updateQry = "UPDATE machineFileInfo SET provenModifiedDate = @date, isMoved = @val, UpdatedTS = GETDATE() WHERE fileName = @file AND filePath LIKE @folder";

            using (SqlCommand cmd = new SqlCommand(updateQry, conn))
            {
                _ = cmd.Parameters.AddWithValue("@date", date);
                _ = cmd.Parameters.AddWithValue("@val", val);
                _ = cmd.Parameters.AddWithValue("@file", file);
                _ = cmd.Parameters.AddWithValue("@folder", folder + "\\%");

                _ = cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {file} information updated in database." + DateTime.Now);
            }
        }

        public void updateDatabaseStandard(FileInformation sFile, FileInformation dbFile, SqlConnection conn)
        {
            int val = 0;
            string date = null;
            string file = dbFile.FileName;
            string folder = dbFile.FolderPath;
            string size = dbFile.FileSize.ToString();
            if (sFile != null)
            {
                date = sFile.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
                file = sFile.FileName;
                folder = sFile.FolderPath;
                size = sFile.FileSize.ToString();
            }

            string updateQry = "UPDATE machineFileInfo SET StandardModifiedDate = @date, isMoved = @val, fileSize = @fileSize , UpdatedTS = GETDATE() WHERE fileName = @fileName AND filePath = @filePath";

            using (SqlCommand cmd = new SqlCommand(updateQry, conn))
            {
                _ = cmd.Parameters.AddWithValue("@date", date);
                _ = cmd.Parameters.AddWithValue("@val", val);
                _ = cmd.Parameters.AddWithValue("@fileName", file);
                _ = cmd.Parameters.AddWithValue("@filePath", folder);
                _ = cmd.Parameters.AddWithValue("@fileSize", size);

                _ = cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {sFile.FileName} information updated in database." + DateTime.Now);
            }
        }

        internal void DeleteFromDatabase(FileInformation dbfile, SqlConnection conn)
        {
            string deleteQry = "DELETE FROM machineFileInfo WHERE fileName = @fileName AND filePath = @filePath";

            using (SqlCommand cmd = new SqlCommand(deleteQry, conn))
            {
                _ = cmd.Parameters.AddWithValue("@fileName", dbfile.FileName);
                _ = cmd.Parameters.AddWithValue("@filePath", dbfile.FolderPath);

                _ = cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {dbfile.FileName} information deleted from database." + DateTime.Now);
            }
        }
    }
}