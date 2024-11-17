using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftRpm;
internal sealed class ShiftRpmConfiguration : OverlayConfiguration
{
    public ShiftRpmConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("General", "General options")]
    public GeneralGrouping General { get; init; } = new();
    public sealed class GeneralGrouping
    {
        [ToolTip("Size of the font")]
        [FloatRange(12.0f, 90.0f, 0.5f, 1)]
        public float FontSize { get; init; } = 12.0f;

        [ToolTip("Change the Font")]
        public RpmTextFont Font { get; init; } = RpmTextFont.Conthrax;

        [IntRange(0, 30, 1)]
        public int ExtraDigitSpacing { get; init; } = 0;
    }

    public enum RpmTextFont { Roboto, Conthrax, Obitron }
}