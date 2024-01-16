﻿namespace RaceElement.Util.DataTypes;

public readonly struct FloatRangeStruct
{
    public readonly float From;
    public readonly float To;

    public FloatRangeStruct(float from, float to)
    {
        From = from;
        To = to;
    }

    public bool IsInRange(float value) => value >= From && value <= To;
}
