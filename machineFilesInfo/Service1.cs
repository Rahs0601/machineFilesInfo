using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
        private List<FileInformation> ProvenMachineProgramList = new List<FileInformation>();
        private List<FileInformation> StandardSoftwareProgramList = new List<FileInformation>();
        Dictionary<string, DateTime> shiftDetails = new Dictionary<string, DateTime>();
        private List<FileInformation> dblist = new List<FileInformation>();
        string synctype = ConfigurationManager.AppSettings["syncType"];
        DateTime startTime = DateTime.Parse(ConfigurationManager.AppSettings["startTime"]);
        private Thread StartFunctionThread = null;
        bool running;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            running = true;
            if (!Directory.Exists(appPath + "\\Logs\\"))
            {
                _ = Directory.CreateDirectory(appPath + "\\Logs\\");
            }

            if (synctype.Equals("shiftend", StringComparison.OrdinalIgnoreCase))
            {
                string query = "select * from shiftdetails where running = 1";
                SqlConnection conn = ConnectionManager.GetConnection();
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = default(SqlDataReader);

                while (reader.Read())
                {
                    shiftDetails.Add(reader["shiftName"].ToString(), DateTime.Parse(reader["shiftEndTime"].ToString()));
                }
            }
            else
            {
                shiftDetails.Add("default", startTime);
            }
            Thread.CurrentThread.Name = "Main";
            Thread StartFunctionThread = new Thread(new ThreadStart(StartFunction));
            StartFunctionThread.Name = "FileDataBaseInfo";
            StartFunctionThread.Start();
        }

        private void StartFunction()
        {
            while (running)
            {
#if DEBUG
                setAndGetFileInfo();
#endif
                foreach (var date in shiftDetails)
                {

                    if (date.Value.TimeOfDay == DateTime.Now.TimeOfDay)
                    {
                        setAndGetFileInfo();
#if !DEBUG
                        Thread.Sleep(1000 * 60 * 60 * 4);
#endif
                    }
                }



            }
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
            dblist = fdba.GetFileInformation();
            string LocalDirectory = ConfigurationManager.AppSettings["folderPath"].ToString();
            GetLocalFiles(LocalDirectory);

            try
            {

                foreach (FileInformation file in StandardSoftwareProgramList)
                {
                    string PrentdirectoryName = file.FolderPath.Split('\\').Reverse().Skip(1).First(); // operation 

                    FileInformation dbfile = dblist.Find(x => x.FileName == file.FileName && x.FolderPath.Contains(PrentdirectoryName));
                    if (dbfile != null && !(file.ModifiedDate.ToString().Equals(dbfile.ModifiedDate.ToString())))
                    {
                        fdba.updateDatabaseStandard(file);
                    }

                    if (dbfile == null)
                    {
                        fdba.InsertIntoDatabase(file);
                    }
                }
                //string[] files = Directory.GetFiles(LocalDirectory);


                // Get common files which are present in both lists dblist and StandardSoftwareProgramList with filename and parent folder path of its parent folder remains same

                // if db == program 

                foreach (FileInformation file in ProvenMachineProgramList)
                {
                    string PrentdirectoryName = file.FolderPath.Split('\\').Reverse().Skip(1).First();

                    FileInformation pfile = StandardSoftwareProgramList.Find(x => x.FileName == file.FileName && x.FolderPath.Contains(PrentdirectoryName));

                    if (pfile != null) //&& file.ModifiedDate.ToString().Equals(pfile.ModifiedDate.ToString()))
                    {
                        fdba.updateDatabaseProven(file, pfile);
                    }

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
            //clear the list
            ProvenMachineProgramList.Clear();
            StandardSoftwareProgramList.Clear();
            dblist.Clear();
        }

        protected override void OnStop()
        {
            running = false;
            StartFunctionThread.Abort();
            Thread.CurrentThread.Name = "Main";
            Logger.WriteDebugLog($"Service Stop at: {DateTime.Now}");
        }

        public void OnDebug()
        {
            OnStart(null);
        }
    }
}
