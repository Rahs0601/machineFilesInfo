using System;

namespace machineFilesInfo
{
    public class FileInformation
    {
        private string fileType;
        private DateTime modifiedDate;
        private string computerName;


        public string FolderPath { get; set; }

        public string FileName { get; set; }
        public string FileType
        {
            get => fileType;
            set => fileType = value;
        }
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate
        {
            get => modifiedDate;
            set => modifiedDate = value;
        }
        public string Owner { get; set; }
        public string ComputerName
        {
            get => computerName;
            set => computerName = value;
        }

    }
}
