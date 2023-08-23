using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace machineFilesInfo
{
    class FileInformation
    {
        private string folderPath;
        private string fileName;
        private string fileType;
        private long fileSize;
        private DateTime createdDate;
        private DateTime modifiedDate;
        private string owner;
        private string computerName;

      
        public string FolderPath
        {
            get { return folderPath; }
            set { folderPath = value; }
        }

        public string FileName 
        { 
            get { return fileName; } 
            set {  fileName = value; } 
        }
        public string FileType 
        {
            get {  return fileType; } 
            set {  fileType = value; } 
        }
        public long FileSize 
        {
            get { return fileSize; } 
            set { fileSize = value; } 
        }
        public DateTime CreatedDate
        {
            get {  return createdDate; }
            set {  createdDate = value; }
        }
        public DateTime ModifiedDate 
        { 
            get { return modifiedDate; } 
            set {  modifiedDate = value; }
        }
        public string Owner 
        { 
            get { return owner; } 
            set { owner = value; } 
        }
        public string ComputerName 
        { 
            get {  return computerName; } 
            set {  computerName = value; } 
        }
        public override bool Equals(object other)
        {
            FileInformation oldFile = other as FileInformation;
            return this.FileSize.Equals(oldFile.FileSize) && this.ModifiedDate.Equals(oldFile.ModifiedDate);
        }
    }
}
