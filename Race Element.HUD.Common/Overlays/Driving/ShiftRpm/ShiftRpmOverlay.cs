using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftRpm;
[Overlay(
Name = "Shift RPM",
Description = "The current engine RPM as text")]
internal sealed class ShiftRpmOverlay : CommonAbstractOverlay
{

    public ShiftRpmOverlay(Rectangle rectangle) : base(rectangle, "Shift RPM")
    {
    }

    public override void Render(Graphics g)
    {

    }
}
internal class RpmBitmaps
{
    public CachedBitmap? GetForNumber(byte number)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 9);

        return null;
    }
}
