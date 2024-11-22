using System;
using System.Speech.Synthesis;
using RaceElement.Core.Jobs;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

public class LowFuelMotorsportSpeechSynthesizer : IJob
{
    private readonly SpeechSynthesizer _synth;
    private readonly String _message;

    public LowFuelMotorsportSpeechSynthesizer(String message)
    {
        _synth = new SpeechSynthesizer();
        _message = message;
    }

    public bool IsRunning { get; private set; } = false;
    public void Run() { _synth.SpeakAsync(_message); }
    public void Cancel() { }
}
