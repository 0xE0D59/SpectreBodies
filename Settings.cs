using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace SpectreBodies
{
    public class Settings : ISettings
    {
        public Settings()
        {
            ReloadListKey = new HotkeyNode(Keys.F6);
            TextColor = new ColorBGRA(255, 255, 255, 255);
            BackgroundColor = new ColorBGRA(0, 0, 0, 255);
            TextOffset = new RangeNode<int>(120, -360, 360);
            TextSize = new RangeNode<int>(16, 1, 200);
            DrawDistance = new RangeNode<int>(600, 0, 2000);
            UseRenderNames = new ToggleNode(true);
        }

        [Menu("Reload body list ")] public HotkeyNode ReloadListKey { get; set; }

        [Menu("Text color")] public ColorNode TextColor { get; set; }

        [Menu("Background color")] public ColorNode BackgroundColor { get; set; }

        [Menu("Text size")] public RangeNode<int> TextSize { get; set; }

        [Menu("Text offset")] public RangeNode<int> TextOffset { get; set; }

        [Menu("Draw distance")] public RangeNode<int> DrawDistance { get; set; }

        [Menu("Use render names")] public ToggleNode UseRenderNames { get; set; }

        public ToggleNode Enable { get; set; } = new ToggleNode(true);
    }
}