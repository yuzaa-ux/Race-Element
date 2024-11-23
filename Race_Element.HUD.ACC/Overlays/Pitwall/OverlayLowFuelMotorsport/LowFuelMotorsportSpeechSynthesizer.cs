using System;
using System.Speech.Synthesis;
using RaceElement.Core.Jobs;
using RaceElement.Core.Jobs.Timer;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

internal class LowFuelMotorsportSpeechSynthesizer : IJob
{
    private readonly ReferenceProperty<long> _synthIdentifier;
    private readonly DateTime _raceStartTime;

    public LowFuelMotorsportSpeechSynthesizer(DateTime raceStartTime, ReferenceProperty<long> synthIdentifier)
    {
        _synthIdentifier = synthIdentifier;
        _raceStartTime = raceStartTime;
    }

    public bool IsRunning { get; private set; } = false;
    public void Cancel() { }

    public void Run()
    {
        var diffSec = (int)((_raceStartTime - DateTime.Now).TotalSeconds);
        string message = $"{diffSec / 60} minutes until race starts";
        int minTimeRaceStartSec = 5;

        if (diffSec <= minTimeRaceStartSec)
        {
            message = "Race has started";
        }
        else if (diffSec <= 60)
        {
            message = "Race starts in less than 1 minute";
        }

        var speech = new SpeechSynthesizer();
        speech.Speak(message);
        speech.Dispose();

        if (diffSec > minTimeRaceStartSec)
        {
            NextMessage(diffSec);
        }
        else
        {
            _synthIdentifier.PropertyAsValue = 0;
        }
    }

    private void NextMessage(int remainingTimeSeconds)
    {
        DateTime time = _raceStartTime;

        if (remainingTimeSeconds > (5 * 60))
        {
            time = _raceStartTime.Subtract(new TimeSpan(0, 0, 5, 0));
        }
        else if (remainingTimeSeconds > (3 * 60))
        {
            time = _raceStartTime.Subtract(new TimeSpan(0, 0, 3, 0));
        }
        else if (remainingTimeSeconds > 60)
        {
            time = _raceStartTime.Subtract(new TimeSpan(0, 0, 1, 0));
        }

        LowFuelMotorsportSpeechSynthesizer speech = new(_raceStartTime, _synthIdentifier);
        TaskTimerExecutor.Instance().Add(speech, time, out _synthIdentifier.PropertyAsReference[0]);
    }
}
