using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;
internal sealed class ShiftBarConfiguration : OverlayConfiguration
{
    public ShiftBarConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("Dimension", "Adjust the size of the shift bar")]
    public SizeGrouping Size { get; init; } = new();
    public sealed class SizeGrouping
    {
        [IntRange(100, 800, 10)]
        public int Width { get; init; } = 400;

        [IntRange(18, 50, 2)]
        public int Height { get; init; } = 20;
    }

    [ConfigGrouping("Rendering", "Adjust the size of the shift bar")]
    public RenderGrouping Render { get; init; } = new();
    public sealed class RenderGrouping
    {
        [IntRange(50, 120, 10, GameMaxs = [80], MaxGames = [Game.iRacing])]
        public int RefreshRate { get; init; } = 80;
    }

    [ConfigGrouping("Data", "Adjust data displayed in the shift bar")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        [ToolTip("Hide Rpms in the bar, starting from 0.\nIt will always leave 2000 RPM.")]
        [IntRange(0, 9000, 100)]
        public int HideRpm { get; init; } = 3000;
    }


    [ConfigGrouping("Upshift Percentages", "Adjust the Early and Upshift percentages.\n" +
        "The displayed early and upshift RPM texts only show in the GUI.\n" +
        "These RPMs will update if you are currently in a lobby, once you adjust any of the settings.")]
    [HideForGame(Game.RaceRoom)]
    public UpshiftGrouping Upshift { get; init; } = new UpshiftGrouping();
    public sealed class UpshiftGrouping
    {
        [ToolTip("Sets the percentage of max rpm required to activate the early upshift color")]
        [FloatRange(80.0f, 96.8f, 0.02f, 2)]
        public float Early { get; init; } = 94.0f;

        [ToolTip("Sets the percentage of max rpm required to activate the upshift color")]
        [FloatRange(97f, 99.98f, 0.02f, 2)]
        public float Upshift { get; init; } = 97.3f;
    }

    [ConfigGrouping("Colors", "Adjust the colors used in the shift bar")]
    public ColorsGrouping Colors { get; init; } = new ColorsGrouping();
    public sealed class ColorsGrouping
    {
        public Color NormalColor { get; init; } = Color.FromArgb(255, 5, 255, 5);
        [IntRange(75, 255, 1)]
        public int NormalOpacity { get; init; } = 255;

        public Color EarlyColor { get; init; } = Color.FromArgb(255, 255, 255, 0);
        [IntRange(75, 255, 1)]
        public int EarlyOpacity { get; init; } = 255;

        public Color UpshiftColor { get; init; } = Color.FromArgb(255, 255, 4, 4);
        [IntRange(75, 255, 1)]
        public int UpshiftOpacity { get; init; } = 255;

        public Color FlashColor { get; init; } = Color.FromArgb(255, 0, 131, 255);
        [IntRange(75, 255, 1)]
        public int FlashOpacity { get; init; } = 255;
    }
}

