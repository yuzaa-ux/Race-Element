using System;
using System.Speech.Synthesis;
using RaceElement.Core.Jobs;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

internal class LowFuelMotorsportSpeechSynthesizer : IJob
{
    private readonly LowFuelMotorsportOverlay _overlay;
    private readonly String _message;
    private readonly int _time;

    public LowFuelMotorsportSpeechSynthesizer(String message, LowFuelMotorsportOverlay overlay, int time)
    {
        _message = message;
        _overlay = overlay;
        _time = time;
    }

    public bool IsRunning { get; private set; } = false;
    public void Cancel() { }

    public void Run()
    {
        var speech = new SpeechSynthesizer();
            speech.Speak(_message);
            speech.Dispose();
        _overlay.SynthesizerCallback(_time);
    }
}
