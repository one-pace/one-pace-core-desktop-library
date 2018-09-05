using System;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ScriptPortal.Vegas;

namespace SonyVegas140.Scripts.VegasExporter
{
    public class VegasException : Exception
    {
        public string Title { get; set; }
        public VegasException(string title, string message) : base(message)
        {
            Title = title;
        }
    }
    public class EntryPoint
    {
        private const string SRT_TIME_FORMAT = "hh\\:mm\\:ss\\,fff";
        private const string VEGAS_TIME_FORMAT = "hh\\:mm\\:ss\\.fff";
        private const string VEGAS_TIME_FORMAT_COMMA = "hh\\:mm\\:ss\\,fff";
        private const string SRT_TIME_PATTERN = "[0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3}";

        private bool _closeonfinish = false;
        private bool _savewhendone = false;
        private bool _makeveg = false;
        private string _file = string.Empty;

        private Vegas _vegas;

        public void FromVegas(Vegas vegas)
        {
            try
            {
                _vegas = vegas;

                ScriptArgs args = Script.Args;
                if (args.Count > 0)
                {
                    _closeonfinish = System.Convert.ToBoolean(args.ValueOf("closeonfinish") ?? "false");
                    _savewhendone = System.Convert.ToBoolean(args.ValueOf("savewhendone") ?? "false");
                    _makeveg = System.Convert.ToBoolean(args.ValueOf("makeveg") ?? "false");
                    _file = args.ValueOf("file");
                }
                else
                {
                    var dialog = new OpenFileDialog { Filter = ".srt files (*.srt)|*.srt", CheckPathExists = true, InitialDirectory = vegas.Project.FilePath };
                    DialogResult result = dialog.ShowDialog();
                    vegas.UpdateUI();

                    if (result == DialogResult.OK)
                    {
                        _file = Path.GetFullPath(dialog.FileName);
                    }
                    else
                    {
                        return;
                    }
                }

                if (_makeveg)
                {
                    Media media = vegas.Project.MediaPool.AddMedia(Script.Args.ValueOf("media"));

                    foreach (MediaStream stream in media.Streams)
                    {
                        if (stream is VideoStream)
                        {
                            VideoTrack t = vegas.Project.AddVideoTrack();
                            VideoEvent e = t.AddVideoEvent(new Timecode(), media.Length);
                            e.ResampleMode = VideoResampleMode.Disable;
                            e.AddTake(stream);
                        }
                        else if (stream is AudioStream)
                        {
                            AudioTrack t = vegas.Project.AddAudioTrack();
                            AudioEvent e = t.AddAudioEvent(new Timecode(), media.Length);
                            e.AddTake(stream);
                        }
                    }

                    vegas.SaveProject(Script.Args.ValueOf("output"));
                }

                using (FileStream fs = new FileStream(_file, FileMode.Open, FileAccess.Read))
                using (StreamReader stream = new StreamReader(fs))
                {
                    while (!stream.EndOfStream)
                    {
                        string line = stream.ReadLine();

                        if (Regex.IsMatch(line, "^[0-9]+$"))
                        {
                            line = stream.ReadLine();

                            if (Regex.IsMatch(line, "^" + SRT_TIME_PATTERN + " --> " + SRT_TIME_PATTERN + "$"))
                            {
                                MatchCollection stamps = Regex.Matches(line, SRT_TIME_PATTERN);

                                TimeSpan s = TimeSpan.ParseExact(stamps[0].Value, SRT_TIME_FORMAT, null);
                                TimeSpan e = TimeSpan.ParseExact(stamps[1].Value, SRT_TIME_FORMAT, null);
                                string t = string.Empty;

                                while (!stream.EndOfStream)
                                {
                                    line = stream.ReadLine();
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        if (!string.IsNullOrEmpty(t))
                                        {
                                            t += "[br]";
                                        }

                                        t += line;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                Region r = Convert(s, e, t);
                                if (r != null) vegas.Project.Regions.Add(r);
                            }
                        }
                    }
                }

                if (_savewhendone)
                {
                    vegas.SaveProject();
                }
                if (_closeonfinish)
                {
                    vegas.Exit();
                }
            }
            catch (VegasException e)
            {
                Vegas.COM.ShowError(e.Title, e.Message);
            }
        }
        private Region Convert(TimeSpan start, TimeSpan end, string text)
        {
            Timecode position = Timecode.FromMilliseconds(start.TotalMilliseconds + 360);
            Timecode length = Timecode.FromMilliseconds((end - start).TotalMilliseconds);

            TimeSpan _;
            if (!TimeSpan.TryParseExact(position.ToPositionString(), VEGAS_TIME_FORMAT, null, out _) &&
                !TimeSpan.TryParseExact(position.ToPositionString(), VEGAS_TIME_FORMAT_COMMA, null, out _))
            {
                throw new VegasException("Incorrect time format!", "Can't import subtitles because your time format is incorrect. Please change your time format to \"Time\" (hh:mm:ss.fff).");
            }

            if (length.ToMilliseconds() > 0 && !string.IsNullOrWhiteSpace(text))
            {
                return new Region(position, length, text.Trim());
            }
            else
            {
                return null;
            }
        }

        private List<string> _log = new List<string>();
        private DialogResult ShowConsole()
        {
            Form dialog = new Form();
            dialog.Text = "Console window";
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.Width = 400;

            Label label = new Label();
            label.Text = "";
            foreach (string message in _log)
            {
                label.Text += message + Environment.NewLine;
            }

            label.Left = 80;
            label.Width = 400;
            label.Height = 500;
            dialog.Controls.Add(label);

            Button okButton = new Button();
            okButton.Text = "Close";
            okButton.Width = 80;
            okButton.Top = 40;
            okButton.DialogResult = DialogResult.OK;
            dialog.AcceptButton = okButton;
            dialog.Controls.Add(okButton);

            return dialog.ShowDialog(_vegas.MainWindow);
        }
    }
}
