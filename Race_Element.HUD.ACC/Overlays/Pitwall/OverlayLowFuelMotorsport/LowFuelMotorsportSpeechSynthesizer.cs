using System;
using System.Speech.Synthesis;
using RaceElement.Core.Jobs;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

internal class LowFuelMotorsportSpeechSynthesizer : IJob
{
    private readonly SpeechSynthesizer _synth;
    private readonly String _message;

    private LowFuelMotorsportOverlay _overlay;
    private int _time;

    public LowFuelMotorsportSpeechSynthesizer(String message, LowFuelMotorsportOverlay overlay, int time)
    {
        _synth = new SpeechSynthesizer();
        _message = message;

        _overlay = overlay;
        _time = time;
    }

    public bool IsRunning { get; private set; } = false;
    public void Cancel() { }

    public void Run()
    {
        _synth.SpeakAsync(_message);
        _overlay.SynthesizerCallback(_time);
    }
}
