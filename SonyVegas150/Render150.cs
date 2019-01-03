using System.IO;

namespace ScriptPortal.Vegas.Render
{
    public class EntryPoint
    {
        public void FromVegas(Vegas vegas)
        {
            string rendererName = Script.Args.ValueOf("renderer");
            string templateName = Script.Args.ValueOf("template");
            Renderer renderer = vegas.Renderers.FindByName(rendererName);
            RenderTemplate template = null;
            if (renderer != null)
            {
                template = renderer.Templates.FindByName(templateName);
            }
            if (template == null)
            {
                vegas.ShowError("Render template not found.");
                return;
            }
            string path = vegas.Project.FilePath;
            string saveas = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + ".rendered.mp4";
            RenderStatus status = vegas.Render(saveas, template);
            if (status == RenderStatus.Complete || status == RenderStatus.Canceled || status == RenderStatus.Quit)
            {
                vegas.Exit();
            }
            else
            {
                vegas.ShowError("Render incomplete. Please try again.");
                return;
            }
        }
    }
}
