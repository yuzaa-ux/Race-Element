using System.Runtime.InteropServices;

namespace RaceElement.HUD.Overlay.Internal;
internal static unsafe partial class Timers
{
    [LibraryImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
    internal static partial uint TimeBeginPeriod(uint uMilliseconds);

    [LibraryImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
    internal static partial uint TimeEndPeriod(uint uMilliseconds);
}