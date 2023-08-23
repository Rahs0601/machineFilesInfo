using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace machineFilesInfo
{
    public partial class Service1 : ServiceBase
    {
        private readonly string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private readonly List<FileInformation> ProvenMachineProgramList = new List<FileInformation>();
        private readonly List<FileInformation> StandardSoftwareProgramList = new List<FileInformation>();
        private Thread thread = null;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!Directory.Exists(appPath + "\\Logs\\"))
            {
                _ = Directory.CreateDirectory(appPath + "\\Logs\\");
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
                    List<FileInformation> commonFiles = new List<FileInformation>();

                    // Use Except to find distinct files
                    List<FileInformation> distinctFiles = new List<FileInformation>();

                    // Get common files which are present in both lists dblist and StandardSoftwareProgramList with filename and parent folder path of its parent folder remains same

                    foreach (FileInformation file in StandardSoftwareProgramList)
                    {
                        //get 2nd last folder name
                        string PrentdirectoryName = file.FolderPath.Split('\\').Reverse().Skip(1).First();

                        //get file from dblist with same filename and parent folder name
                        FileInformation dbfile = dblist.Find(x => x.FileName == file.FileName && x.FolderPath.Contains(PrentdirectoryName));

                        //if file is present in dblist then add it to commonFiles list
                        if (dbfile != null)
                        {
                            commonFiles.Add(dbfile);
                        }

                        //if file is not present in dblist then add it to distinctFiles list
                        else
                        {
                            distinctFiles.Add(file);
                        }

                    }


                    if (commonFiles.Count > 0)
                    {
                        foreach (FileInformation file in commonFiles)
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
