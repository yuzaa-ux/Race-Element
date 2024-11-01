using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Common.Overlays.Pitwall.TimerDebug;
[Overlay(Name = "Timer Debug", Description = "Debug timers", OverlayType = OverlayType.Pitwall)]
internal sealed class TimerDebugOverlay : CommonAbstractOverlay
{
    InfoPanel _panel;
    public TimerDebugOverlay(Rectangle rectangle) : base(rectangle, "Timer Debug")
    {
        _panel = new(11, 300);
    }

    public override bool ShouldRender() => true;

    public override void Render(Graphics g)
    {
        _panel.AddLine("TP Hz", $"{TimeProvider.System.TimestampFrequency}");

        _panel.AddLine("HR?", $"{Stopwatch.IsHighResolution}");
        _panel.AddLine("SW Hz", $"{Stopwatch.Frequency}");
        _panel.Draw(g);
    }
}
