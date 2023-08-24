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

            string query = "select * from machineFileInfo order by fileName";
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
                        ModifiedDate = (DateTime)reader["storedModifiedDate"],
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


        public void InsertIntoDatabase(FileInformation local)
        {
            string operation = local.FolderPath.Split('\\').Reverse().Skip(1).First();
            string component = local.FolderPath.Split('\\').Reverse().Skip(3).First();
            string opid = operation.Split('_').First();
            string opdescription = operation.Split('_').Last();
            string Cdate = local.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            string Mdate = local.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            string insertQry = "Insert into machineFileInfo(fileName, fileType, filePath, fileSize, fileDateCreated, storedModifiedDate, fileOwner, computer , operationID  ,operationDespcription , component)  " +
                                                "values  (@file_Name, @file_Type, @folder, @file_Size, @created_Date, @modified_Date , @owner, @computer_Name ,@opId , @opDescription , @component)";

            SqlConnection conn = ConnectionManager.GetConnection();

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

        public void updateDatabaseProven(FileInformation File, FileInformation File2)
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
            string updateQry = $"UPDATE machineFileInfo SET provenModifiedDate = '{date}', isMoved = {val}, UpdatedTS = GETDATE() WHERE fileName = '{File.FileName}' and filePath like '{folder}\\" + "%'";
            SqlConnection conn = ConnectionManager.GetConnection();

            using (SqlCommand cmd = new SqlCommand(updateQry, conn))
            {
                _ = cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {File.FileName} information updated in  database." + DateTime.Now);
            }
        }
        public void updateDatabaseStandard(FileInformation SFile)
        {
            int val = 0;
            string date = SFile.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            SqlConnection conn = ConnectionManager.GetConnection();
            string updateQry = "UPDATE machineFileInfo SET storedModifiedDate = @date, isMoved = @val, fileSize = @fileSize , UpdatedTS = GETDATE() WHERE fileName = @fileName AND filePath = @filePath";

            using (SqlCommand cmd = new SqlCommand(updateQry, conn))
            {
                _ = cmd.Parameters.AddWithValue("@date", date);
                _ = cmd.Parameters.AddWithValue("@val", val);
                _ = cmd.Parameters.AddWithValue("@fileName", SFile.FileName);
                _ = cmd.Parameters.AddWithValue("@filePath", SFile.FolderPath);
                _ = cmd.Parameters.AddWithValue("@fileSize", SFile.FileSize);

                _ = cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {SFile.FileName} information updated in database." + DateTime.Now);
            }

        }

        //   @"IF EXISTS (SELECT * from MachineRunningHMIStatus_Cumi where MachineInterfaceId=@machineInfaceId )
        //	BEGIN
        //	  UPDATE MachineRunningHMIStatus_Cumi SET CompInterface = @PowderGrade+':'+@ProductDescription,
        //	  OrderNumber= @OrderNumber, PlanQty = @PlanQty , ActualQty = @ActualQty, Status= @Status,
        //	  BatchTS = @BatchTS,  UpdatedTS = GETDATE(),ExpectedFinishTime = @ExpectedFinishTime
        //                     where MachineInterfaceId=@machineInfaceId
        //	END
        //ELSE
        //	BEGIN
        //	INSERT INTO MachineRunningHMIStatus_Cumi(MachineInterfaceId,CompInterface,OrderNumber,PlanQty,ActualQty,Status,BatchTS,UpdatedTS,ExpectedFinishTime)
        //	VALUES(@machineInfaceId,@PowderGrade+':'+@ProductDescription,@OrderNumber,@PlanQty,@ActualQty,@Status,@BatchTS,GETDATE(),@ExpectedFinishTime)
        //END"


    }
}
