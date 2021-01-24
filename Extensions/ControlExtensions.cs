using System.Windows.Controls;
using System.Windows.Media;

namespace OnePaceCore.Extensions
{
    public static class ControlExtensions
    {
        public static void Enable(this Control control)
        {
            control.Foreground = Brushes.Black;
            control.IsEnabled = true;
        }
        public static void Disable(this Control control)
        {
            control.Foreground = Brushes.Gray;
            control.IsEnabled = false;
        }
    }
}
