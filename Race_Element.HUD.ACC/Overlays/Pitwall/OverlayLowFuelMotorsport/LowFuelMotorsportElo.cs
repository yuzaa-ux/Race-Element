using System;
using System.Collections.Generic;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport.API;

internal sealed class LowFuelMotorsportElo
{
    private const float LN2 = 0.69314718056f;
    private const float E = 2.71828182845f;

    private readonly RaceInfo _raceInfo;
    private readonly float _magic;

    public LowFuelMotorsportElo(RaceInfo raceInfo)
    {
        _raceInfo = raceInfo;
        _magic = ComputeMagic(_raceInfo.Entries);
    }

    public int GetPositionThreshold() => (int)Math.Floor(_raceInfo.Entries.Count - _magic);

    public int GetElo(int position)
    {
        float elo = (_raceInfo.Entries.Count - position - _magic - ((_raceInfo.Entries.Count / 2.0f) - position) / 100.0f) * 200.0f / _raceInfo.Entries.Count * _raceInfo.KFactor;
        return (int)Math.Round(elo);
    }

    public int GetCarNumber()
    {
        SplitEntry player = _raceInfo.Entries.Find(x => x.IsPlayer);
        return player.RaceNumber;
    }

    private static float ComputeMagic(int selfElo, int otherElo)
    {
        float e = (1600.0f / LN2);
        float youExp = (-selfElo / e);
        float otherExp = (-otherElo / e);

        double you = Math.Pow(E, youExp);
        double other = Math.Pow(E, otherExp);

        double result = ((1 - you) * other) / ((1 - other) * you + (1 - you) * other);
        return (float)result;
    }

    private static float ComputeMagic(List<SplitEntry> entries)
    {
        float magic = 0;
        SplitEntry player = entries.Find(x => x.IsPlayer);

        foreach (SplitEntry e in entries)
        {
            if (e.IsPlayer) continue;
            magic += ComputeMagic(player.Elo, e.Elo);
        }

        return magic;
    }
}
