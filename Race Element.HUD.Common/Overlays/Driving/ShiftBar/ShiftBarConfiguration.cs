using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;
internal sealed class ShiftBarConfiguration : OverlayConfiguration
{
    public ShiftBarConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("Bar", "Adjust the size of the shift bar")]
    public SizeGrouping Bar { get; init; } = new();
    public sealed class SizeGrouping
    {
        [IntRange(100, 800, 10)]
        public int Width { get; init; } = 500;

        [IntRange(12, 50, 2)]
        public int Height { get; init; } = 16;

        [IntRange(50, 200, 10, GameMaxs = [80], MaxGames = [Game.iRacing])]
        public int RefreshRate { get; init; } = 80;
    }

    [ConfigGrouping("Data", "Adjust data displayed in the shift bar")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        /// <summary>
        /// Used for showing a certain amount of RPM, <see cref="VisibleRpmAmount"/>
        /// </summary>
        public int HideRpm = 3_000;
        public readonly int MinVisibleRpm = 500;

        [ToolTip("The amount of RPM that the bar displays, 500 at minimum.\n")]
        [IntRange(500, 30_000, 100)]
        public int VisibleRpmAmount { get; init; } = 3_000;

        [ToolTip("Shows a vertical line that indicates the optimal upshift point")]
        public bool RedlineMarker { get; init; } = true;
    }

    [ConfigGrouping("Upshift Percentages", "Adjust the Early and Upshift percentages.\n" + "The Early is always checked first, so if the Redline is lower than the early.. it won't be hit.")]
    [HideForGame(Game.RaceRoom)]
    public UpshiftGrouping Upshift { get; init; } = new UpshiftGrouping();
    public sealed class UpshiftGrouping
    {
        [ToolTip("Sets the percentage of max rpm required to activate the early upshift color")]
        [FloatRange(69.0f, 99.8f, 0.001f, 3)]
        public float EarlyPercentage { get; init; } = 94.0f;

        [ToolTip("Sets the percentage of max rpm required to activate the upshift color")]
        [FloatRange(70f, 99.98f, 0.001f, 3)]
        public float RedlinePercentage { get; init; } = 97.3f;

        [ToolTip("Only enable this when configuring the Upshift Percentages below." +
                      "\nDraws the outcome of these percentages when activating this HUD. Including the Max amount of rpm." +
                      "\n")]
        public bool DrawUpshiftData { get; init; } = false;

        /// <summary>
        /// If you know the max rpm you can use this to adjust the Max Rpm in the above Preview Image.
        /// </summary>
        [IntRange(10, 17_000, 1)]
        [ToolTip("Sets the current Rpm in the above preview image.")]
        public int PreviewRpm { get; init; } = 8500;
        /// <summary>
        /// If you know the max rpm you can use this to adjust the Max Rpm in the above Preview Image.
        /// </summary>
        [IntRange(10, 17_000, 1)]
        [ToolTip("Sets the current Max Rpm in the above preview image.")]
        public int MaxPreviewRpm { get; init; } = 9250;
    }

    [ConfigGrouping("Colors", "Adjust the colors used in the shift bar")]
    public ColorsGrouping Colors { get; init; } = new ColorsGrouping();
    public sealed class ColorsGrouping
    {
        public Color NormalColor { get; init; } = Color.FromArgb(255, 5, 255, 5);

        public Color EarlyColor { get; init; } = Color.FromArgb(255, 255, 255, 0);

        public Color RedlineColor { get; init; } = Color.FromArgb(255, 255, 4, 4);

        public Color FlashColor { get; init; } = Color.FromArgb(255, 0, 131, 255);
    }
}
