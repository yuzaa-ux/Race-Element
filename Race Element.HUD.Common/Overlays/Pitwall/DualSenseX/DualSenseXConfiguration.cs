using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DualSenseX;

internal sealed class DualSenseXConfiguration : OverlayConfiguration
{
    public DualSenseXConfiguration()
    {
        GenericConfiguration.AlwaysOnTop = false;
        GenericConfiguration.Window = false;
        GenericConfiguration.Opacity = 1.0f;
        GenericConfiguration.AllowRescale = false;
    }

    [ConfigGrouping("DSX UDP", "Adjust the port DSX uses, 6969 is default.")]
    public UdpConfig UDP { get; init; } = new UdpConfig();
    public sealed class UdpConfig
    {
        [ToolTip("Adjust the port used by DSX, 6969 is default.")]
        [IntRange(0, 65535, 1)]
        public int Port { get; init; } = 6969;
    }

    [ConfigGrouping("TC Haptics", "Adjust the haptics right trigger when TC is activated.")]
    [HideForGame(Game.AssettoCorsa1)]
    public ThrottleHapticsConfig TcHaptics { get; init; } = new ThrottleHapticsConfig();
    public sealed class ThrottleHapticsConfig
    {
        [ToolTip("Adds haptics when traction control is activated.")]
        public bool TcEffect { get; init; } = true;

        [ToolTip("Sets the frequency of the traction control haptics.")]
        [IntRange(10, 150, 1)]
        public int TcFrequency { get; init; } = 90;
    }

    [ConfigGrouping("ABS Haptics", "Adjust the haptics for the left and right trigger.")]
    [HideForGame(Game.AssettoCorsa1)]
    public AbsHapticsConfig AbsHaptics { get; init; } = new AbsHapticsConfig();
    public sealed class AbsHapticsConfig
    {
        [ToolTip("Adds haptics when abs is activated.")]
        public bool AbsEffect { get; init; } = true;

        [ToolTip("Sets the frequency of the abs haptics.")]
        [IntRange(10, 150, 1)]
        public int AbsFrequency { get; init; } = 85;
    }

    [ConfigGrouping("Brake Slip", "Adjust the slip effect whilst applying the brakes.")]
    [HideForGame(Game.RaceRoom)]
    public BrakeSlipHaptics BrakeSlip { get; init; } = new();
    public sealed class BrakeSlipHaptics
    {
        [ToolTip("The minimum brake percentage before any effects are applied. See this like a deadzone.")]
        [FloatRange(0.1f, 99f, 0.1f, 1)]
        public float BrakeTreshold { get; init; } = 3f;

        [ToolTip("Sets the frequency of the slip effect whilst applying the brakes.")]
        [IntRange(10, 150, 1)]
        public int Frequency { get; init; } = 85;

        [FloatRange(0.05f, 6f, 0.01f, 2)]
        public float FrontSlipTreshold { get; init; } = 0.7f;

        [FloatRange(0.05f, 6f, 0.01f, 2)]
        public float RearSlipTreshold { get; init; } = 0.6f;
    }

    [ConfigGrouping("Throttle Slip", "Adjust the slip effect whilst applying the throttle.\nModify the threshold to increase or decrease sensitivity in different situations.")]
    [HideForGame(Game.RaceRoom)]
    public ThrottleSlipHaptics ThrottleSlip { get; init; } = new();
    public sealed class ThrottleSlipHaptics
    {

        /// <summary>
        /// The throttle in percentage (divide by 100f if you want 0-1 value)
        /// </summary>
        [ToolTip("The minimum throttle percentage before any effects are applied. See this like a deadzone.")]
        [FloatRange(0.1f, 99f, 0.1f, 1)]
        public float ThrottleTreshold { get; init; } = 3f;

        [ToolTip("Sets the frequency of the slip effect whilst applying the throttle.")]
        [IntRange(10, 150, 1)]
        public int Frequency { get; init; } = 85;

        [ToolTip("Decrease this treshold to increase the sensitivity when the front wheels slip (understeer).\n")]
        [FloatRange(0.05f, 6f, 0.01f, 2)]
        public float FrontSlipTreshold { get; init; } = 0.6f;

        [ToolTip("Decrease this treshold to increase the sensitivity when the rear wheels slip (oversteer).")]
        [FloatRange(0.05f, 6f, 0.01f, 2)]
        public float RearSlipTreshold { get; init; } = 0.5f;
    }
}
