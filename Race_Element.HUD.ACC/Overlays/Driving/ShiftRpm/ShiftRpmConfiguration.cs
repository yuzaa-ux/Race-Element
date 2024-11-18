using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.ShiftRpm;
internal sealed class ShiftRpmConfiguration : OverlayConfiguration
{
    public ShiftRpmConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("General", "General options")]
    public GeneralGrouping General { get; init; } = new();
    public sealed class GeneralGrouping
    {
        [ToolTip("Size of the font")]
        [FloatRange(12.0f, 90.0f, 0.5f, 1)]
        public float FontSize { get; init; } = 20;

        [ToolTip("Change the Font")]
        public RpmTextFont Font { get; init; } = RpmTextFont.Obitron;

        [IntRange(0, 30, 1)]
        public int ExtraDigitSpacing { get; init; } = 2;

        [IntRange(1, 100, 1)]
        public int RefreshRate { get; init; } = 50;
    }


    [ConfigGrouping("Colors", "Adjust colors")]
    public ColorsGrouping Colors { get; init; } = new ColorsGrouping();
    public class ColorsGrouping
    {
        public Color TextColor { get; init; } = Color.FromArgb(255, 255, 255, 255);
        [IntRange(75, 255, 1)]
        public int TextOpacity { get; init; } = 255;

        public Color BackgroundColor { get; init; } = Color.FromArgb(255, 0, 0, 0);
        [IntRange(75, 255, 1)]
        public int BackgroundOpacity { get; init; } = 175;
    }

    public enum RpmTextFont { Roboto, Conthrax, Obitron, Segoe }
}