using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;
internal sealed class ShiftBarConfiguration : OverlayConfiguration
{
    public ShiftBarConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("Dimension", "Adjust the size of the shift bar")]
    public SizeGrouping Size { get; init; } = new();
    public sealed class SizeGrouping
    {
        [IntRange(100, 600, 10)]
        public int Width { get; init; } = 400;

        [IntRange(10, 50, 2)]
        public int Height { get; init; } = 20;
    }

    [ConfigGrouping("Rendering", "Adjust the size of the shift bar")]
    public RenderGrouping Render { get; init; } = new();
    public sealed class RenderGrouping
    {
        [IntRange(60, 120, 10)]
        public int RefreshRate { get; init; } = 120;
    }

    [ConfigGrouping("Data", "Adjust data displayed in the shift bar")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        [ToolTip("Hides an amount of rpm from the ship bar.")]
        [IntRange(0, 10000, 1000)]
        public int HideRpm { get; init; } = 3000;
    }
}

