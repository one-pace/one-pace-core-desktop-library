using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ScriptPortal.Vegas.RegionsToTextbox
{
    public class EntryPoint
    {
        private Vegas _vegas;
        public void FromVegas(Vegas vegas)
        {
            try
            {
                _vegas = vegas;

                Track track = vegas.Project.AddVideoTrack();
                PlugInNode generator = vegas.Generators.GetChildByName("VEGAS Titles & Text");

                foreach (var region in vegas.Project.Regions)
                {
                    string text = region.Label;

                    VideoEvent e = new VideoEvent(vegas.Project, Timecode.FromMilliseconds(region.Position.ToMilliseconds()), region.Length, null);
                    track.Events.Add(e);

                    e.FadeIn.Curve = CurveType.Linear;
                    e.FadeOut.Curve = CurveType.Linear;
                    text = HandleAegisubTags(e, text);

                    CreateTextPreset("RegionsToTextbox", "Impress BT", 14, 0.5, 0.08, 10.0, text);
                    Media media = new Media(generator, "RegionsToTextbox");
                    MediaStream stream = media.Streams[0];
                    Take take = new Take(stream);
                    e.Takes.Add(take);
                }
            }
            catch (Exception e)
            {
                vegas.ShowError(e);
            }
        }
        public static string HandleAegisubTags(VideoEvent e, string text)
        {
            MatchCollection tagmatches = Regex.Matches(text, @"\{\\[^\}]+\}");
            foreach (Match tagmatch in tagmatches)
            {
                MatchCollection functionmatches = Regex.Matches(tagmatch.Value, @"\\[\w\d][^\}\\]*");
                foreach (Match functionmatch in functionmatches)
                {
                    Match fad = Regex.Match(functionmatch.Value, @"fad\(\d+(,\d+)?\)");

                    if (fad.Success)
                    {
                        MatchCollection values = Regex.Matches(fad.Value, @"\d+");

                        e.FadeIn.Length = new Timecode(int.Parse(values[0].Value));
                        if (values.Count == 2)
                        {
                            e.FadeOut.Length = new Timecode(int.Parse(values[1].Value));
                        }
                    }
                }
            }

            return Regex.Replace(text, @"\{\\[^\}]+\}", "");
        }
        public static void CreateTextPreset(string name, string font, int fontsize, double locationx, double locationy, double outlineWidth, string text)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OFX Presets\\com.sonycreativesoftware_titlesandtext\\Generator\\" + name + ".xml";

            text = text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Replace("[br]", "\\par" + Environment.NewLine);
            int lines = 0;
            text = BreakLine(text, 500 / fontsize, out lines);
            locationy = locationy + (0.05 * lines);

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            builder.AppendLine("<OfxPreset plugin=\"com.sonycreativesoftware:titlesandtext\" context=\"Generator\" name=\"" + name + "\">");
            builder.AppendLine("<OfxPlugin>com.sonycreativesoftware:titlesandtext</OfxPlugin>");
            builder.AppendLine("<OfxPluginVersion>1 0</OfxPluginVersion>");
            builder.AppendLine("<OfxParamTypeString name=\"Text\">        <OfxParamValue>{\\rtf1\\ansi\\ansicpg1252\\deff0{\\fonttbl{\\f0\\fnil\\fcharset0 " + font + ";}{\\f1\\fnil\\fcharset0 Verdana;}}");
            builder.AppendLine("\\viewkind4\\uc1\\pard\\qc\\lang1033\\f0\\fs" + (fontsize * 2).ToString() + " " + text + "\\f1\\fs96\\par");
            builder.AppendLine("</OfxParamValue>");
            builder.AppendLine("</OfxParamTypeString>");
            builder.AppendLine("<OfxParamTypeRGBA name=\"TextColor\"><OfxParamValue>1.000000 1.000000 1.000000 1.000000</OfxParamValue></OfxParamTypeRGBA>");
            builder.AppendLine("<OfxParamTypeString name=\"AnimationName\">        <OfxParamValue>_None</OfxParamValue>");
            builder.AppendLine("</OfxParamTypeString>");
            builder.AppendLine("<OfxParamTypeDouble name=\"Scale\"><OfxParamValue>1.000000</OfxParamValue></OfxParamTypeDouble>");
            builder.AppendLine("<OfxParamTypeDouble2D name=\"Location\"><OfxParamValue>" + locationx.ToString(nfi) + " " + locationy.ToString(nfi) + "</OfxParamValue></OfxParamTypeDouble2D>");
            builder.AppendLine("<OfxParamTypeChoice name=\"Alignment\"><OfxParamValue>4</OfxParamValue></OfxParamTypeChoice>");
            builder.AppendLine("<OfxParamTypeRGBA name=\"Background\"><OfxParamValue>0.000000 0.000000 0.000000 0.000000</OfxParamValue></OfxParamTypeRGBA>");
            builder.AppendLine("<OfxParamTypeDouble name=\"Tracking\"><OfxParamValue>0.000000</OfxParamValue></OfxParamTypeDouble>");
            builder.AppendLine("<OfxParamTypeDouble name=\"LineSpacing\"><OfxParamValue>1.000000</OfxParamValue></OfxParamTypeDouble>");
            builder.AppendLine("<OfxParamTypeDouble name=\"OutlineWidth\"><OfxParamValue>" + outlineWidth.ToString(nfi) + "</OfxParamValue></OfxParamTypeDouble>");
            builder.AppendLine("<OfxParamTypeRGBA name=\"OutlineColor\"><OfxParamValue>0.000000 0.000000 0.000000 1.000000</OfxParamValue></OfxParamTypeRGBA>");
            builder.AppendLine("<OfxParamTypeBoolean name=\"ShadowEnable\"><OfxParamValue>false</OfxParamValue></OfxParamTypeBoolean>");
            builder.AppendLine("<OfxParamTypeRGBA name=\"ShadowColor\"><OfxParamValue>0.000000 0.000000 0.000000 1.000000</OfxParamValue></OfxParamTypeRGBA>");
            builder.AppendLine("<OfxParamTypeDouble name=\"ShadowOffsetX\"><OfxParamValue>0.200000</OfxParamValue></OfxParamTypeDouble>");
            builder.AppendLine("<OfxParamTypeDouble name=\"ShadowOffsetY\"><OfxParamValue>0.200000</OfxParamValue></OfxParamTypeDouble>");
            builder.AppendLine("<OfxParamTypeDouble name=\"ShadowBlur\"><OfxParamValue>0.400000</OfxParamValue></OfxParamTypeDouble>");
            builder.AppendLine("</OfxPreset>");

            new FileInfo(path).Directory.Create();
            File.WriteAllText(path, builder.ToString());
        }
        public static string BreakLine(string text, int maxCharsInLine, out int lines)
        {
            string builder = "";
            string[] ls = Regex.Split(text, @"\\par" + Environment.NewLine);
            lines = ls.Length;

            bool first = true;
            foreach (string line in ls)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder += "\\par" + Environment.NewLine;
                }

                int charsInLine = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    if (char.IsWhiteSpace(c) && charsInLine >= maxCharsInLine)
                    {
                        builder += "\\par" + Environment.NewLine;
                        charsInLine = 0;
                        lines++;
                    }
                    else
                    {
                        builder += c;
                        charsInLine++;
                    }
                }
            }

            return builder;
        }
    }
}
