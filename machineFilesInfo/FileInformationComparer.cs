using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace machineFilesInfo
{
    public class FileInformationComparer : IEqualityComparer<FileInformation>
    {
        public bool Equals(FileInformation x, FileInformation y)
        {
            // Customize your comparison logic here.
            // For example, you can compare based on FileName and FilePath.
            // Split the path by the directory separator character (backslash)
            string[] pathParts = x.FolderPath.Split('\\');

            // Get the last part of the split array, which is the folder name
            string xfolderName = pathParts[pathParts.Length - 2];
            pathParts = y.FolderPath.Split('\\');
            string yfolderName = pathParts[pathParts.Length - 2];

            return x.FileName == y.FileName && xfolderName == yfolderName;
        }

        public int GetHashCode(FileInformation obj)
        {
            // Customize your hash code generation here.
            // Ensure it matches the logic used in the Equals method.
            return obj.FileName.GetHashCode() ^ obj.FolderPath.GetHashCode();
        }
    }
}
