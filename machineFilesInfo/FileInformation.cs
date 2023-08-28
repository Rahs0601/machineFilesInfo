using System;

namespace machineFilesInfo
{
    public class FileInformation
    {
        public string FolderPath { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Owner { get; set; }
        public string ComputerName { get; set; }
    }
}