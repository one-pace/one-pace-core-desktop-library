using OnePaceCore.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace OnePaceCore.Networking
{
    public class FTPClient
    {
        public delegate void FTPTransferProgressEventHandler(double bytesPerSecond, double percentage);
        public delegate void FTPMultipleTransferProgressChangedEventHandler(int progress);
        public delegate void FTPTransferCanceledEventHandler(WebException exception);
        private Uri _uri;
        private NetworkCredential _credentials;
        private Stopwatch _stopWatch;
        public FTPClient(string host, string userName, string password)
        {
            _uri = new Uri($"ftp://{host}");
            _credentials = new NetworkCredential(userName, password);
        }
        public void Upload(string localPath, string remoteDirectory, string remoteFileName, FileExistsAction fileExistsAction)
        {
            
            Upload(localPath, remoteDirectory, remoteFileName, fileExistsAction, null, null, null);
        }
        public void Upload(string localPath, string remoteDirectory, string remoteFileName, FileExistsAction fileExistsAction, FTPTransferProgressEventHandler progressChanged, FTPTransferCanceledEventHandler uploadCanceled, CancellationTokenSource cts)
        {
            try
            {
                Upload(localPath, remoteDirectory, remoteFileName, fileExistsAction, progressChanged, cts);
            }
            catch (WebException e)
            {
                uploadCanceled?.Invoke(e);
            }
        }
        public byte[] Upload(string localPath, string remoteDirectory, string remoteFileName, FileExistsAction fileExistsAction, FTPTransferProgressEventHandler progressChanged, CancellationTokenSource cts)
        {
            if (fileExistsAction == FileExistsAction.Overwrite)
            {
                try
                {
                    Delete(Path.Combine(remoteDirectory, remoteFileName));
                }
                catch { }
            }
            using (WebClient client = new WebClient())
            {
                client.Credentials = _credentials;
                client.UploadProgressChanged += (sender, e) =>
                {
                    double percent = (double)e.BytesSent / (double)e.TotalBytesToSend;
                    double seconds = _stopWatch.ElapsedMilliseconds / 1000;
                    double bps = 0;
                    if (seconds > 0)
                    {
                        bps = e.BytesSent / seconds;
                    }
                    progressChanged?.Invoke(bps, percent * 100);
                };
                cts?.Token.Register(client.CancelAsync);

                try
                {
                    GetResponse(WebRequestMethods.Ftp.MakeDirectory, remoteDirectory);
                }
                catch { }

                string url = $"{_uri.ToString()}{directory}{(string.IsNullOrWhiteSpace(fileName) ? "" : "/" + fileName)}".Replace("//", "/").Replace("ftp:/", "ftp://");
                Uri uri = new Uri(url);
                _stopWatch = Stopwatch.StartNew();
                byte[] task = client.UploadFileTaskAsync(uri, "STOR", localPath.ToString()).GetAwaiter().GetResult();
                _stopWatch.Stop();
                return task;
            }
        }
        public void UploadMultiple(List<Tuple<string, string, string>> filesWithPaths, FileExistsAction fileExistsAction, FTPMultipleTransferProgressChangedEventHandler uploadProgressChanged, FTPTransferProgressEventHandler progressChanged, FTPTransferCanceledEventHandler uploadCanceled, CancellationTokenSource cts)
        {
            try
            {
                for (int i = 1; i <= filesWithPaths.Count; i++)
                {
                    var item = filesWithPaths[i - 1];

                    Upload(item.Item1, item.Item2, item.Item3, fileExistsAction, progressChanged, cts);
                    uploadProgressChanged.Invoke(i);
                }
            }
            catch (WebException exception)
            {
                uploadCanceled.Invoke(exception);
            }
        }
        public void Download(string remotePath, string localPath, SSHClient sshClient, FTPTransferProgressEventHandler progressChanged, FTPTransferCanceledEventHandler downloadCanceled, CancellationTokenSource cts)
        {
            try
            {
                long totalBytes = 0;
                using (var client = new WebClient())
                {
                    totalBytes = sshClient.GetFileSize(sshClient.Root + remotePath);

                    cts.Token.Register(client.CancelAsync);

                    client.Credentials = _credentials;
                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        double percentage = (double)e.BytesReceived / (double)totalBytes;
                        percentage *= 100;
                        double seconds = _stopWatch.ElapsedMilliseconds / 1000;
                        double bps = 0;
                        if (seconds > 0)
                        {
                            bps = e.BytesReceived / seconds;
                        }
                        progressChanged.Invoke(bps, percentage);
                    };

                    if (_uri.ToString().EndsWith("/") && remotePath.StartsWith("/"))
                    {
                        remotePath = remotePath.Length > 1 ? remotePath.Substring(1) : string.Empty;
                    }

                    Uri uri = new Uri($"{_uri.ToString()}{remotePath}");

                    _stopWatch = Stopwatch.StartNew();

                    client.DownloadFileTaskAsync(uri, localPath).GetAwaiter().GetResult();

                    _stopWatch.Stop();
                }
            }
            catch (WebException exception)
            {
                downloadCanceled?.Invoke(exception);
            }

            return;
        }
        public void Delete(string path)
        {
            var response = GetResponse(WebRequestMethods.Ftp.DeleteFile, path);
            if (response.StatusCode != FtpStatusCode.FileActionOK)
            {
                throw new Exception(response.StatusDescription);
            }
        }
        {
        public IList<FTPDirectoryDetails> ListDirectoryDetails(string path)
            var details = new List<FTPDirectoryDetails>();
            string permissionPattern = "([-ldrwx]{10}) {1,3}";
            string idPattern = @"\d+ ";
            string usernamePattern = @"([a-z0-9]+) ([a-z0-9]+) +";
            string sizePattern = @"(\d+) ";
            string datePattern = @"([A-z]{3})\s{1,2}(\d{1,2}) {1,2}(\d{4}|\d{2}:\d{2}) ";
            string pattern = "^" + permissionPattern + idPattern + usernamePattern + sizePattern + datePattern + @"([^ ]*(?= -> ))?( -> )?(.+)$";
            Regex regex = new Regex(pattern);
            DateTime now = DateTime.UtcNow;
            IFormatProvider culture = CultureInfo.GetCultureInfo("en-us");

            IList<string> infos = GetLines(WebRequestMethods.Ftp.ListDirectoryDetails, path);
            foreach (string info in infos)
            {
                Match match = regex.Match(info);
                string permissions = match.Groups[1].Value;
                string yearOrTime = match.Groups[7].Value;
                bool isTime = Regex.IsMatch(yearOrTime, @"\d{2}:\d{2}");
                yearOrTime = isTime ? now.Year.ToString() + " " + yearOrTime : yearOrTime;
                bool isDirectory = permissions.StartsWith("d");
                string dateFormat = isTime ? "MMM d yyyy HH:mm" : "MMM d yyyy";
                DateTime modified = DateTime.ParseExact($"{match.Groups[5].Value} {match.Groups[6].Value} {yearOrTime}", dateFormat, culture, DateTimeStyles.None);
                long size = isDirectory ? 0 : long.Parse(match.Groups[4].Value);
                bool isLink = permissions.StartsWith("l");
                string name = string.Empty;
                string p = string.Empty;

                if (isLink)
                {
                    name = match.Groups[8].Value;
                    p = match.Groups[10].Value;
                }
                else
                {
                    name = match.Groups[10].Value;
                    p = path + "/" + name;
                }

                details.Add(new FTPDirectoryDetails
                {
                    DateModified = modified,
                    Name = name,
                    Permissions = permissions,
                    Path = p,
                    Size = size,
                    Raw = info
                });
            }

            return details;
        }
        public FtpWebResponse GetResponse(string method, string path)
        {
            if (!Regex.IsMatch(path, "[^\\/]"))
            {
                path = "";
            }
            while (Regex.IsMatch(path, "(\\/\\/)"))
            {
                path = Regex.Replace(path, "(\\/\\/)", "/");
            }
            if (path.StartsWith("/") && path.Length >= 2)
            {
                path = path.Substring(1);
            }

            Uri uri = new Uri($"{_uri.ToString()}{path}");
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Credentials = _credentials;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            return response;
        }
        public IList<string> GetLines(string method, string path)
        {
            IList<string> lines = new List<string>();

            FtpWebResponse response = GetResponse(method, path);

            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    lines.Add(line);
                }

                return lines;
            }
        }
    }

    public static class FTPClientExtension
    {
        public static byte[] GetBytes(this FtpWebResponse response, string method, string path)
        {
            int count = 0;
            int bufferSize = (int)Math.Pow(2, 10);
            using (Stream stream = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                do
                {
                    byte[] block = new byte[1024];
                    count = stream.Read(block, 0, 1024);
                    byte[] block = new byte[bufferSize];
                    count = stream.Read(block, 0, bufferSize);
                    memoryStream.Write(block, 0, count);
                } while (stream.CanRead && count > 0);

                return memoryStream.ToArray();
            }
        }
    }
}
