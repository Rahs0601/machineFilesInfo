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
            GetLocalFiles(LocalDirectory);

            try
            {

                if (Directory.Exists(LocalDirectory))
                {
                    string[] files = Directory.GetFiles(LocalDirectory);

                    // Use Intersect to find files that are present in both lists
                    var commonFiles = ProvenMachineProgramList.Intersect(StandardSoftwareProgramList, new FileInformationComparer()).ToList();

                    // Use Except to find distinct files
                    var distinctFiles = ProvenMachineProgramList.Except(StandardSoftwareProgramList, new FileInformationComparer()).ToList();



                    if (commonFiles.Count > 0)
                    {
                        foreach (var file in commonFiles)
                        {
                            FileInformation dbfile = dblist[dblist.IndexOf(file)];
                            if (!dbfile.ModifiedDate.Equals(file.ModifiedDate))
                            {
                                fdba.updateDatabase(file);
                            }
                        }
                    }
                    else
                    {
                        foreach (FileInformation local in distinctFiles)
                        {
                            fdba.InsertIntoDatabase(local);
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
