using System;
using System.Collections.Generic;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport.API;

public class LowFuelMotorsportElo
{
    static readonly float LN2 = 0.69314718056f;
    static readonly float E   = 2.71828182845f;

    private List<SplitEntry> _entries;
    private SplitEntry _player;
    private float _magic;

    public LowFuelMotorsportElo(List<SplitEntry> entries)
    {
        _entries = entries;
        _magic = ComputeMagic(_entries);
    }

    public int GetPositionThreshold()
    {
        return (int)Math.Floor(_entries.Count - _magic);
    }

    public int GetElo(int position)
    {
        float elo = (_entries.Count - position - _magic - ((_entries.Count / 2.0f) - position) / 100.0f) * 200.0f / _entries.Count * 0.8f;
        return (int)Math.Round(elo);
    }

    public int GetCarNumber()
    {
        SplitEntry player = _entries.Find(x => x.IsPlayer);
        return player.RaceNumber;
    }

    private float ComputeMagic(int selfElo, int otherElo)
    {
        float e = (1600.0f / LN2);
        float youExp   = (-selfElo / e);
        float otherExp = (-otherElo / e);

        double you   = Math.Pow(E, youExp);
        double other = Math.Pow(E, otherExp);

        double result = ((1 - you) * other)/((1 - other) * you + (1 - you) * other);
        return (float)result;
    }

    private float ComputeMagic(List<SplitEntry> entries)
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
