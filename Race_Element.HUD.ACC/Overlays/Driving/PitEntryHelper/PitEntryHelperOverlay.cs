using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.PitEntryHelper;
#if DEBUG
[Overlay(
    Name = "Pit Entry Helper",
    Description = "Provides an accurate entry point of the pits and the required speed to match using a bar.\n Helps you to brake as late as possible for pit entry.",
    Authors = ["Reinier Klarenberg"]
)]
#endif
internal class PitEntryHelperOverlay : AbstractOverlay
{
    public PitEntryHelperOverlay(Rectangle rectangle) : base(rectangle, "Pit Entry Helper")
    {
    }

    public override void Render(Graphics g)
    {
        if (pageGraphics.IsInPitLane)
        {

        }
    }
}
