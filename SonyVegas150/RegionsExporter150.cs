using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ScriptPortal.Vegas.RegionsExporter
{
    static class TimecodeExtensions
    {
        public static string ToString(this Timecode tc, string format)
        {
            TimeSpan ts = new TimeSpan((long)tc.ToMilliseconds() * TimeSpan.TicksPerMillisecond);
            return ts.ToString(format);
        }
    }
    public class EntryPoint
    {
        private const string SRT_TIME_FORMAT = "hh\\:mm\\:ss\\,fff";
        private Vegas vegas;
        public void FromVegas(Vegas vegas)
        {
            this.vegas = vegas;

            string projName;

            string projFile = this.vegas.Project.FilePath;
            if (string.IsNullOrEmpty(projFile))
            {
                projName = "Untitled";
            }
            else
            {
                projName = Path.GetFileNameWithoutExtension(projFile);
            }

            string file = null;

            if (this.ShowSaveDialog("Exports regions to .srt", projName + ".regions", out file, ".srt file (*.srt)|*.srt") == DialogResult.OK)
            {
                if (Path.GetExtension(file) != null)
                {
                    if (Path.GetExtension(file).ToUpper() == ".SRT")
                    {
                        this.ExportRegions(file);
                    }
                }
            }
        }

        void ExportRegions(string file)
        {
            try
            {
                using (FileStream stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.Unicode))
                {
                    int lineNumber = 1;
                    foreach (Track track in this.vegas.Project.Tracks)
                    {
                        if (track.IsAudio())
                        {
                            foreach (TrackEvent @event in track.Events)
                            {
                                Timecode pstart = @event.Start;                             // project start timestamp of event
                                Timecode pend = @event.End;                               // project end timestamp of event
                                Timecode mstart = @event.ActiveTake.Offset;                 // source media start timestamp
                                Timecode mend = @event.ActiveTake.Offset + @event.Length; // source media end timestamp

                                foreach (MediaRegion mregion in @event.ActiveTake.Media.Regions)
                                {
                                    Timecode mregionstart = mregion.Position;
                                    Timecode mregionend = mregion.End;

                                    if (
                                        (mstart <= mregionstart && mregionstart < mend) ||       // [media start]<=[media region start]<[media end]
                                        (mstart < mregionend && mregionend <= mend))
                                    {           // [media start]<[media region end]<=[media end]
                                        Timecode mregionoffset = mregionstart - mstart;          // [    |----| ] length between "[" and the first "|"
                                        Timecode pregionstart = pstart + mregionoffset;         // actual project timestamp of region start
                                        Timecode pregionend = pregionstart + mregion.Length;  // actual project timestamp of region end

                                        string line = GetSRTLine(lineNumber, pregionstart, pregionend, mregion.Label);

                                        streamWriter.Write(line + Environment.NewLine + Environment.NewLine);
                                        lineNumber++;
                                    }
                                }
                            }
                        }
                    }

                    foreach (Region pregion in vegas.Project.Regions)
                    {
                        string line = this.GetSRTLineFromRegion(lineNumber, pregion);
                        streamWriter.WriteLine(line);
                        streamWriter.WriteLine();
                        lineNumber++;
                    }
                }
            }
            finally { }
        }

        string GetSRTLineFromRegion(int lineNumber, Region region)
        {
            return this.GetSRTLine(lineNumber, region.Position, region.End, region.Label);
        }
        string GetSRTLine(int lineNumber, Timecode start, Timecode end, string text)
        {
            string line = string.Empty;
            line += lineNumber + Environment.NewLine;
            line += Timecode.FromMilliseconds(start.ToMilliseconds()).ToString(SRT_TIME_FORMAT) + " --> " + end.ToString(SRT_TIME_FORMAT) + Environment.NewLine;
            line += Regex.Replace(text, "\\[br\\]", Environment.NewLine);

            return line;
        }

        DialogResult ShowSaveDialog(string title, string defaultFilename, out string file, string filter = "All Files (*.*)|*.*")
        {
            file = null;

            SaveFileDialog d = new SaveFileDialog
            {
                Filter = filter,
                Title = title,
                InitialDirectory = this.vegas.Project.FilePath,
                CheckPathExists = true,
                AddExtension = true
            };


            if (defaultFilename != null)
            {
                d.DefaultExt = Path.GetExtension(defaultFilename);
                d.FileName = Path.GetFileName(defaultFilename);
            }

            DialogResult result = d.ShowDialog();
            if (result == DialogResult.OK)
            {
                file = Path.GetFullPath(d.FileName);
            }

            return result;
        }
    }
}
