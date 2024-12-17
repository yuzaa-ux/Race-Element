using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using System;
using static RaceElement.HUD.Common.Overlays.Pitwall.DSX.DsxResources;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DSX;

internal sealed class DsxJob(DsxOverlay overlay) : AbstractLoopJob
{
    public sealed override void RunAction()
    {
        //if (!overlay.ShouldRender())
        //    return;

        if (overlay._client == null)
        {
            try
            {
                overlay.CreateEndPoint();
                overlay.SetLighting();
            }
            catch (Exception)
            {
                // let's not cause an app crash, shall we?
            }
        }

        Packet tcPacket = TriggerHaptics.HandleAcceleration(overlay._config, overlay.GameWhenStarted);
        if (tcPacket != null)
        {
            overlay.Send(tcPacket);
            //ServerResponse response = Receive();
            //HandleResponse(response);
        }

        Packet absPacket = TriggerHaptics.HandleBraking(overlay._config, overlay.GameWhenStarted);
        if (absPacket != null)
        {
            overlay.Send(absPacket);
            //ServerResponse response = Receive();
            //HandleResponse(response);
        }
    }
    public override void AfterCancel()
    {
        overlay?._client?.Close();
        overlay?._client?.Dispose();
    }
}
