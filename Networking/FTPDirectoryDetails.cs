using System;

namespace OnePaceCore.Networking
{
    public struct FTPDirectoryDetails
    {
        public bool IsDirectory
        {
            get
            {
                return Permissions.StartsWith("d");
            }
        }
        public bool IsLink
        {
            get
            {
                return Permissions.StartsWith("l");
            }
        }
        public string Raw { get; set; }
        public string Permissions { get; set; }
        public DateTime DateModified { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
    }
}
