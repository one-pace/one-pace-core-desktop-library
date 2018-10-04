namespace ScriptPortal.Vegas.MakeVeg
{
    public class EntryPoint
    {
        public void FromVegas(Vegas vegas)
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
            vegas.Exit();
        }
    }
}
