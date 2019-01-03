using Renci.SshNet;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OnePaceCore.Networking
{
    public class DelugeClient
    {
        private string _host;
        private string _username;
        private string _password;
        private string ConnectionString { get { return $"connect {_host} {_username} {_password}; "; } }
        public DelugeClient(string host, string username, string password)
        {
            _host = host;
            _username = username;
            _password = password;
        }
        public void PauseTorrent(string torrentid)
        {
            RunCommand($"pause \"{torrentid}\"");
        }
        public void GetTorrentInfo(string torrentid)
        {
            RunCommand($"info \"{torrentid}\"");
        }
        public string GetTorrentInfoCommand(string torrentid)
        {
            return GetCommand($"info \\\"{torrentid}\\\"");
        }
        public string GetPauseTorrentCommand(string torrentid)
        {
            return GetCommand($"pause \\\"{torrentid}\\\"");
        }
        public void AddTorrent(string saveLocation, string torrentPath)
        {
            RunCommand($"add -p \"{saveLocation}\" \"{torrentPath}\"");
        }
        public string GetAddTorrentCommand(string saveLocation, string torrentPath)
        {
            return GetCommand($"add -p \\\"{saveLocation}\\\" \\\"{torrentPath}\\\"");
        }
        public string GetCommand(string command)
        {
            return $"deluge-console \"{ConnectionString + command}\"";
        }
        public void RunCommand(string command)
        {
            Process.Start("deluge-console", ConnectionString + command);
        }
        public TorrentInfo GetTorrentInfo(SSHClient sshClient, string torrentid)
        {
            string command = GetTorrentInfoCommand(torrentid);
            SshCommand response = sshClient.RunCommand(command);
            string result = response.Result;
            if (response.Result.StartsWith("Failed to connect"))
            {
                throw new Exception(response.Result);
            }
            TorrentInfo info = new TorrentInfo();
            foreach (string item in result.Split('\n'))
            {
                if (item.StartsWith("Name: "))
                {
                    info.Name = item.Substring(5).TrimStart();
                }
                else if (item.StartsWith("ID: "))
                {
                    info.ID = item.Substring(3).TrimStart();
                }
                else if (item.StartsWith("State: "))
                {
                    info.State = item.Substring(6).TrimStart();
                }
                else if (item.StartsWith("Size: "))
                {
                    info.Size = item.Substring(5).TrimStart();
                }
                else if (item.StartsWith("Seed time: "))
                {
                    info.SeedTime = item.Substring(10).TrimStart();
                }
                else if (item.StartsWith("Tracker status: "))
                {
                    info.TrackerStatus = item.Substring(15).TrimStart();
                }
                else if (item.StartsWith("Progress: "))
                {
                    info.Progress = item.Substring(9).TrimStart();
                }
                else if (item.StartsWith("Seeds: "))
                {
                    info.Seeds = item.Substring(6).TrimStart();
                }
            }
            return info;
        }
        public class TorrentInfo
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public string State { get; set; }
            public string Size { get; set; }
            public string SeedTime { get; set; }
            public string TrackerStatus { get; set; }
            public string Progress { get; set; }
            public string Seeds { get; set; }
        }
        public string GetTorrentHash(SSHClient sshClient, string torrentid)
        {
            TorrentInfo info = GetTorrentInfo(sshClient, torrentid);
            if (info == null)
            {
                throw new Exception("Something went wrong getting the torrent info, check that the torrent exists.");
            }
            if (string.IsNullOrWhiteSpace(info.ID) || !Regex.IsMatch(info.ID, "[A-Fa-f0-9]{40}"))
            {
                throw new Exception("Incorrect torrent hash format, check that the torrent exists and isn't corrupted.");
            }
            return info.ID;
        }
        public void AddTorrent(SSHClient sshClient, string saveLocation, string torrentPath)
        {
            string command = GetAddTorrentCommand(saveLocation, torrentPath);
            sshClient.RunCommand(command);
        }
    }
}
