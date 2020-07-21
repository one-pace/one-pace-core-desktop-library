using OnePaceCore.Utils;
using Renci.SshNet;
using System;

namespace OnePaceCore.Networking
{
    public class SSHClient : IDisposable
    {
        private readonly SshClient _client;
        public string Root { get; set; }

        public SSHClient(string host, string root, string username, string password)
        {
            _client = new SshClient(host, username, password);
            Root = root;
        }

        public void MakeTorrent(string dataPath, string torrentPath, string trackers)
        {
            long size = GetFileSize(dataPath);
            int log = Math.Min(28, Math.Max(15, (int)Math.Log(size / 1700, 2)));
            string command = "mktorrent -l " + log + " -a \"" + trackers + "\" -o \"" + torrentPath + "\" \"" + dataPath + "\"";

            RunCommand(command);
        }
        public SshCommand RunCommand(string command)
        {
            _client.Connect();
            SshCommand response = _client.RunCommand(command);
            _client.Disconnect();

            if (!string.IsNullOrWhiteSpace(response.Error))
            {
                throw new Exception(response.Error);
            }
            else if (response.ExitStatus != 0)
            {
                if (!string.IsNullOrWhiteSpace(response.Result))
                {
                    throw new Exception(response.Result);
                }
                else
                {
                    throw new Exception("Something went wrong");
                }
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options">-a list all files including hidden file starting with '.'
        /// --color colored list[= always / never / auto]
        /// -d list directories - with ' */'
        /// -F add one char of */=>@| to enteries
        /// -i list file's inode index number
        /// -l list with long format - show permissions
        /// -la list long format including hidden files
        /// -lh list long format with readable file size
        /// -ls list with long format with file size
        /// -r list in reverse order
        /// -R list recursively directory tree
        /// -s list file size
        /// -S sort by file size
        /// -t sort by time & date
        /// -X sort by ext</param>
        /// <returns></returns>
        public string[] GetFilePaths(string path, string options)
        {
            SshCommand response = RunCommand($"ls {options} \"{path}\"");
            string result = response.Result;
            return result.Split(Environment.NewLine.ToCharArray());
        }
        public long GetFileSize(string path)
        {
            SshCommand response = RunCommand($"du -s -B1 \"{path}\"");

            string result = response.Result;
            if (!string.IsNullOrWhiteSpace(response.Result))
            {
                string number = result.Substring(0, result.IndexOf('\t'));

                if (long.TryParse(number, out long size))
                {
                    return size;
                }
            }

            return 0;
        }

        public string Delete(string path)
        {
            string unixPath = UnixPathUtils.SanitizePath(path);
            unixPath = unixPath.Replace(" ", "\\ ");
            SshCommand response = RunCommand($"rm {unixPath}");
            return response.Result;
        }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
