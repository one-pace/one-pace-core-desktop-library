using System.IO;
using System.Windows.Forms;

namespace ScriptPortal.Vegas.Render
{
    public class EntryPoint
    {
        public void FromVegas(Vegas vegas)
        {
            string rendererName = Script.Args.ValueOf("renderer");
            string templateName = Script.Args.ValueOf("template");
            string saveas = Script.Args.ValueOf("saveas", true);
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
            if (string.IsNullOrWhiteSpace(saveas))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = ".mp4 file (*.mp4)|*.mp4",
                    Title = "Select render location",
                    InitialDirectory = vegas.Project.FilePath,
                    CheckPathExists = true,
                    AddExtension = true
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    saveas = Path.GetFullPath(saveFileDialog.FileName);
                }
            }
            if (string.IsNullOrWhiteSpace(saveas))
            {
                vegas.ShowError("No valid save path exists.");
                return;
            }

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
