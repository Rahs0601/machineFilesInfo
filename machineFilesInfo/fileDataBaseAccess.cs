using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace machineFilesInfo
{
    class fileDataBaseAccess
    {
        FileInformation file = new FileInformation();
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
                    file.FileName = reader["fileName"].ToString().Trim();
                    file.FileType = reader["fileType"].ToString().Trim();
                    file.FolderPath = reader["filePath"].ToString().Trim();
                    file.FileSize = Int32.Parse(reader["fileSize"].ToString());
                    file.CreatedDate = (DateTime)reader["fileDateCreated"];
                    file.ModifiedDate = (DateTime)reader["fileDateModified"];
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
        public void SetFileInformation(string changeFileName, long size, DateTime newModifiedDate)
        {            
            file.FileName = changeFileName;
            file.FileSize = size;   
            file.CreatedDate = newModifiedDate;
        }
    }
}
