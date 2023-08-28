using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
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
        private readonly List<TimeSpan> shiftDetails = new List<TimeSpan>();
        private List<FileInformation> dblist = new List<FileInformation>();
        private readonly string synctype = ConfigurationManager.AppSettings["syncType"];
        private readonly Thread StartFunctionThread = null;
        private DateTime Target = DateTime.Now.AddHours(-1);
        private bool running;

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

            Thread.CurrentThread.Name = "Main";
            Thread StartFunctionThread = new Thread(new ThreadStart(StartFunction))
            {
                Name = "FileDataBaseInfo"
            };
            StartFunctionThread.Start();
        }

        private void StartFunction()
        {
            try
            {
                if (synctype.Equals("shiftend", StringComparison.OrdinalIgnoreCase))
                {
                    string query = "select * from shiftdetails where running = 1";
                    SqlConnection conn = ConnectionManager.GetConnection();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        //get time only from db
                        TimeSpan Shiftend = DateTime.Parse(reader["ToTime"].ToString()).TimeOfDay;
                        shiftDetails.Add(Shiftend);
                    }
                    shiftDetails.Sort();
                }
                else
                {
                    TimeSpan startTime = DateTime.Parse(ConfigurationManager.AppSettings["startTime"]).TimeOfDay;
                    shiftDetails.Add(startTime);
                }

                while (running)
                {
                    try
                    {
                        if (DateTime.Now >= Target)
                        {
                            Logger.WriteDebugLog("Process Started at" + DateTime.Now);
                            setAndGetFileInfo();
                            Logger.WriteDebugLog("Process Ended at" + DateTime.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteErrorLog(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
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
            SqlConnection conn = null;
            try
            {
                fileDataBaseAccess fdba = new fileDataBaseAccess();
                dblist = fdba.GetFileInformation();
                string LocalDirectory = ConfigurationManager.AppSettings["folderPath"].ToString();
                GetLocalFiles(LocalDirectory);
                conn = ConnectionManager.GetConnection();
                foreach (FileInformation sfile in StandardSoftwareProgramList)
                {
                    FileInformation dbfile = dblist.Find(x => x.FileName == sfile.FileName && x.FolderPath == sfile.FolderPath);
                    if (dbfile != null && !sfile.ModifiedDate.ToString().Equals(dbfile.ModifiedDate.ToString()))
                    {
                        fdba.updateDatabaseStandard(sfile, dbfile, conn);
                    }

                    if (dbfile == null)
                    {
                        fdba.InsertIntoDatabase(sfile, conn);
                    }
                }
                conn?.Close();

                conn = ConnectionManager.GetConnection();
                foreach (FileInformation pfile in ProvenMachineProgramList)
                {
                    string folder = pfile.FolderPath.Substring(0, pfile.FolderPath.LastIndexOf('\\'));

                    FileInformation sfile = StandardSoftwareProgramList.Find(x => x.FileName == pfile.FileName && x.FolderPath.StartsWith(folder));
                    if (sfile != null)
                    {
                        fdba.updateDatabaseProven(pfile, sfile, conn);
                    }
                }

                conn?.Close();

                conn = ConnectionManager.GetConnection();

                foreach (FileInformation dbfile in dblist)
                {
                    FileInformation sfile = StandardSoftwareProgramList.Find(x => x.FileName == dbfile.FileName && x.FolderPath == dbfile.FolderPath);
                    string folder = dbfile.FolderPath.Substring(0, dbfile.FolderPath.LastIndexOf('\\'));
                    FileInformation pfile = ProvenMachineProgramList.Find(x => x.FileName == dbfile.FileName && x.FolderPath.StartsWith(folder));

                    if (sfile == null && pfile == null)
                    {
                        fdba.DeleteFromDatabase(dbfile, conn);
                    }
                    if (sfile == null && pfile != null)
                    {
                        fdba.updateDatabaseStandard(sfile, dbfile, conn);
                    }
                    if (sfile != null && pfile == null)
                    {
                        fdba.updateDatabaseProven(pfile, sfile, conn);
                    }
                }

                // target time should be one of the shift end time which is getter the current time
                //get index of shift end time which is getter the current
                int idx = shiftDetails.FindIndex(x => x > DateTime.Now.TimeOfDay);
                if (idx == -1)
                {
                    idx = 0;
                }
                Target = DateTime.Now.Add(shiftDetails[idx]);
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog($"An error occurred: {ex.Message}" + DateTime.Now);
            }
            finally
            {
                conn?.Close();
                //clear the list
                ProvenMachineProgramList.Clear();
                StandardSoftwareProgramList.Clear();
                dblist.Clear();
            }
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