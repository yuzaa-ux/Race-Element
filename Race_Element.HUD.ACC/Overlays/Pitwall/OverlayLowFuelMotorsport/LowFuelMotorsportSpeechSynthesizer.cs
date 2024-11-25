using System;
using System.Speech.Synthesis;
using RaceElement.Core.Jobs;
using RaceElement.Core.Jobs.Timer;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

internal class LowFuelMotorsportSpeechSynthesizer : IJob
{
    private readonly DateTime _raceStartTimeUtc;
    private readonly LowFuelMotorsportOverlay _lfmOverlay;

    public LowFuelMotorsportSpeechSynthesizer(DateTime raceStartTimeUtc, LowFuelMotorsportOverlay lfmOverlay)
    {
        _raceStartTimeUtc = raceStartTimeUtc;
        _lfmOverlay = lfmOverlay;
    }

    public bool IsRunning { get; private set; } = false;

    public Guid JobId => Guid.NewGuid();

    public void Cancel() { }

    public void Run()
    {
        var timeDiff = (_raceStartTimeUtc - DateTime.UtcNow);
        string message = $"{timeDiff.Minutes} minutes until race starts";
        int minTimeRaceStartSec = 5;

        if (timeDiff.TotalSeconds <= minTimeRaceStartSec)
        {
            message = "Race has started";
        }
        else if (timeDiff.TotalSeconds < 60)
        {
            message = "Race starts in less than 1 minute";
        }

        var speech = new SpeechSynthesizer();
        speech.Speak(message);
        speech.Dispose();

        if (timeDiff.TotalSeconds > minTimeRaceStartSec)
        {
            NextMessage((int)timeDiff.TotalSeconds);
        }
    }

    private void NextMessage(int remainingTimeSeconds)
    {
        DateTime time = _raceStartTimeUtc;

        if (remainingTimeSeconds > (5 * 60))
        {
            time = _raceStartTimeUtc.Subtract(new TimeSpan(0, 0, 5, 0));
        }
        else if (remainingTimeSeconds > (3 * 60))
        {
            time = _raceStartTimeUtc.Subtract(new TimeSpan(0, 0, 3, 0));
        }
        else if (remainingTimeSeconds > 60)
        {
            time = _raceStartTimeUtc.Subtract(new TimeSpan(0, 0, 1, 0));
        }

        LowFuelMotorsportSpeechSynthesizer speech = new(_raceStartTimeUtc, _lfmOverlay);
        if (JobTimerExecutor.Instance().Add(speech, time, out Guid jobId))
            _lfmOverlay._speechJobIds.Add(jobId);
    }
}
