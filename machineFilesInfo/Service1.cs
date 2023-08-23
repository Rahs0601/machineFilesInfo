using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace machineFilesInfo
{
    public partial class Service1 : ServiceBase
    {

        string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        List<FileInformation> ProvenMachineProgramList = new List<FileInformation>();
        List<FileInformation> StandardSoftwareProgramList = new List<FileInformation>();
        var LocalFileList = new List<FileInformation>();
        Thread thread = null;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!Directory.Exists(appPath + "\\Logs\\"))
            {
                Directory.CreateDirectory(appPath + "\\Logs\\");
            }

            ThreadStart start = new ThreadStart(setAndGetFileInfo);
            thread = new Thread(start);
            thread.Start();
        }
        private void GetLocalFiles(string path)
        {
            try
            {

                foreach (string subDirectory in Directory.GetDirectories(path, "Proven Machine Program", SearchOption.AllDirectories))
                {
                    foreach (string file in Directory.GetFiles(subDirectory))
                    {
                        FileInformation localFile = new FileInformation();
                        FileInfo fileInfo = new FileInfo(file);
                        localFile.FileName = fileInfo.Name;
                        localFile.FileType = fileInfo.Extension;
                        localFile.FolderPath = fileInfo.DirectoryName;
                        localFile.FileSize = fileInfo.Length;
                        localFile.CreatedDate = fileInfo.CreationTime;
                        localFile.ModifiedDate = fileInfo.LastWriteTime;
                        localFile.Owner = "UnknownOwner";
                        localFile.ComputerName = Environment.MachineName;
                        ProvenMachineProgramList.Add(localFile);
                    }
                }
                foreach (string subDirectory in Directory.GetDirectories(path, "Standard Software Program", SearchOption.AllDirectories))
                {
                    foreach (string file in Directory.GetFiles(subDirectory))
                    {
                        FileInformation localFile = new FileInformation();
                        FileInfo fileInfo = new FileInfo(file);
                        localFile.FileName = fileInfo.Name;
                        localFile.FileType = fileInfo.Extension;
                        localFile.FolderPath = fileInfo.DirectoryName;
                        localFile.FileSize = fileInfo.Length;
                        localFile.CreatedDate = fileInfo.CreationTime;
                        localFile.ModifiedDate = fileInfo.LastWriteTime;
                        localFile.Owner = "UnknownOwner";
                        localFile.ComputerName = Environment.MachineName;
                        StandardSoftwareProgramList.Add(localFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
        }
        public void setAndGetFileInfo()
        {
            fileDataBaseAccess fdba = new fileDataBaseAccess();
            List<FileInformation> dblist = fdba.GetFileInformation();
            string LocalDirectory = ConfigurationManager.AppSettings["folderPath"].ToString();


            try
            {


                if (Directory.Exists(LocalDirectory))
                {
                    string[] files = Directory.GetFiles(LocalDirectory);

                    if (files.Length > 0)
                    {


                        var modifiedFiles = localFileList.Where(localfile => dblist.Any(dbFile => (dbFile.FileName == localfile.FileName) && (dbFile.ModifiedDate != localfile.ModifiedDate)));

                        foreach (FileInformation local in localFileList)
                        {
                            if (false)
                            {
                                fdba.SetFileInformation(local.FileName, local.FileSize, local.ModifiedDate);


                                try
                                {
                                    string updateQry = "UPDATE machineFileInfo " +
                                                   "SET fileDateModified = @ModifiedDate, fileSize = @FileSize" +
                                                   "WHERE fileName = @fileName";

                                    SqlConnection conn = ConnectionManager.GetConnection();

                                    using (SqlCommand cmd = new SqlCommand(updateQry, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@fileSize", local.FileSize);
                                        cmd.Parameters.AddWithValue("@modifiedDate", local.ModifiedDate);

                                        cmd.ExecuteNonQuery();

                                        Logger.WriteExtraLog($"File {local.FileName} information updated into the database." + DateTime.Now);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteErrorLog("Error while updating data: " + ex.Message);
                                }
                                Logger.WriteExtraLog($"File {local.FileName} information updated into the database." + DateTime.Now);
                            }
                            else
                            {

                                // Check if the file information is already in the database
                                SqlConnection conn = ConnectionManager.GetConnection();
                                SqlCommand checkCommand = new SqlCommand("SELECT fileName FROM machineFileInfo WHERE fileName = @file_Name", conn);
                                checkCommand.Parameters.AddWithValue("@file_Name", local.FileName);

                                int? existingId = checkCommand.ExecuteScalar() as int?;


                                if (existingId == null)
                                {
                                    string insertQry = "Insert into machineFileInfo(fileName, fileType, filePath, fileSize, fileDateCreated, fileDateModified, fileOwner, computer)  " +
                                                 "values  (@file_Name, @file_Type, @folder, @file_Size, @created_Date, @modified_Date , @owner, @computer_Name)";

                                    conn = ConnectionManager.GetConnection();


                                    using (SqlCommand cmd = new SqlCommand(insertQry, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@file_Name", local.FileName);
                                        cmd.Parameters.AddWithValue("@file_Type", local.FileType);
                                        cmd.Parameters.AddWithValue("@folder", local.FolderPath);
                                        cmd.Parameters.AddWithValue("@file_Size", local.FileSize);
                                        cmd.Parameters.AddWithValue("@created_Date", local.CreatedDate);
                                        cmd.Parameters.AddWithValue("@modified_Date", local.ModifiedDate);
                                        cmd.Parameters.AddWithValue("@owner", local.Owner);
                                        cmd.Parameters.AddWithValue("@computer_Name", local.ComputerName);

                                        cmd.ExecuteNonQuery();

                                        Logger.WriteExtraLog($"File {local.FileName} information inserted into the database." + DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger.WriteDebugLog("Invalid folder path: " + LocalDirectory);
                }
            }
            catch (DirectoryNotFoundException)
            {
                Logger.WriteDebugLog("The specified folder does not exist.");
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog($"An error occurred: {ex.Message}" + DateTime.Now);
            }

        }
        protected override void OnStop()
        {
            thread.Abort();
            Logger.WriteDebugLog($"Service Stop at: {DateTime.Now}");
        }

        public void OnDebug()
        {
            OnStart(null);
        }
    }
}
