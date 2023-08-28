using System.ServiceProcess;

namespace machineFilesInfo
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
#if !DEBUG
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new Service1()
                };
                ServiceBase.Run(ServicesToRun);
            }
#else
            {
                Service1 service1 = new Service1();
                service1.OnDebug();
            }
#endif
        }
    }
}