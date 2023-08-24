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
                    file.ModifiedDate = (DateTime)reader["storedModifiedDate"];
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
                cmd.Parameters.AddWithValue("@file_Name", local.FileName);
                cmd.Parameters.AddWithValue("@file_Type", local.FileType);
                cmd.Parameters.AddWithValue("@folder", local.FolderPath);
                cmd.Parameters.AddWithValue("@file_Size", local.FileSize);
                cmd.Parameters.AddWithValue("@created_Date", Cdate);
                cmd.Parameters.AddWithValue("@modified_Date", Mdate);
                cmd.Parameters.AddWithValue("@owner", local.Owner);
                cmd.Parameters.AddWithValue("@computer_Name", local.ComputerName);
                cmd.Parameters.AddWithValue("@opId", opid);
                cmd.Parameters.AddWithValue("@opDescription", opdescription);
                cmd.Parameters.AddWithValue("@component", component);
                cmd.ExecuteNonQuery();
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
            string updateQry = $"UPDATE machineFileInfo SET provenModifiedDate = '{date}', isMoved = {val} WHERE fileName = '{File.FileName}' and filePath like '{folder}\\" + "%'";
            SqlConnection conn = ConnectionManager.GetConnection();

            using (SqlCommand cmd = new SqlCommand(updateQry, conn))
            {
                cmd.ExecuteNonQuery();
                Logger.WriteExtraLog($"File {File.FileName} information updated in  database." + DateTime.Now);
            }
        }
        public void updateDatabaseStandard(FileInformation SFile)
        {
            int val = 0;
            string date = SFile.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            SqlConnection conn = ConnectionManager.GetConnection();
            string updateQry = "UPDATE machineFileInfo SET storedModifiedDate = @date, isMoved = @val, fileSize = @fileSize WHERE fileName = @fileName AND filePath = @filePath";

            using (SqlCommand cmd = new SqlCommand(updateQry, conn))
            {
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@val", val);
                cmd.Parameters.AddWithValue("@fileName", SFile.FileName);
                cmd.Parameters.AddWithValue("@filePath", SFile.FolderPath);
                cmd.Parameters.AddWithValue("@fileSize", SFile.FileSize);

                cmd.ExecuteNonQuery();
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
