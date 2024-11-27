using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.Data.ACC.Tracks.Data;

internal sealed class Nordschleife : AbstractTrackData
{
    public override Guid Guid => new("fae08ea8-03fb-429b-be59-c56c7cd18ef1");
    public override string GameName => "nurburgring_24h";
    public override string FullName => "24H Nürburgring";

    public override float FactorScale => 0.061f;
    public override float PitLaneTime => 25f;

    public override int TrackLength => 25378;


    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
        { new FloatRangeStruct(0.0169f, 0.025f), (1, "Yokohama-S")},
        { new FloatRangeStruct(0.025f, 0.03f), (2, "Yokohama-S")},
        { new FloatRangeStruct(0.041f, 0.0487f), (3, "Valvoline Kurve")},
        { new FloatRangeStruct(0.0488f, 0.0575f), (4, "Ford Kurve")},
        { new FloatRangeStruct(0.0697f, 0.081f), (5, "Goodyear Hairpin")},
        { new FloatRangeStruct(0.0867f, 0.0935f), (6, "Michael Schumacher-S")},
        { new FloatRangeStruct(0.0936f, 0.1006f), (7, "Michael Schumacher-S")},
        { new FloatRangeStruct(0.1091f, 0.116f), (8, "Ravenol bend")},
        { new FloatRangeStruct(0.1186f, 0.1274f), (9, "Bilstein bend")},
        { new FloatRangeStruct(0.1345f, 0.1445f), (10, "Advan arch")},
        { new FloatRangeStruct(0.1525f, 0.1585f), (11, "NGK chicane")},
        { new FloatRangeStruct(0.1586f, 0.1639f), (12, "NGK chicane")},
        { new FloatRangeStruct(0.166f, 0.172f), (13, "T13")},
        { new FloatRangeStruct(0.1721f, 0.1796f), (14, "Sabine Schmitz-Kurve")},
        { new FloatRangeStruct(0.1867f, 0.1955f), (15, "Hatzenbach Bogen")},
        { new FloatRangeStruct(0.1998f, 0.2219f), (16, "Hatzenbach")},
        { new FloatRangeStruct(0.224f, 0.2346f), (17, "Hoheichen")},
        { new FloatRangeStruct(0.2405f, 0.2487f), (18, "Quiddelbacher Höhe")},
        { new FloatRangeStruct(0.251f, 0.273f), (19, "Flugplatz")},
        { new FloatRangeStruct(0.2735f, 0.2884f), (20, "Kottenborn")},
        { new FloatRangeStruct(0.3007f, 0.3149f), (21, "Schwedenkreuz")},
        { new FloatRangeStruct(0.3158f, 0.3268f), (22, "Aremberg")},
        { new FloatRangeStruct(0.3421460f, 0.35568f), (23, "Fuchsröhre")},
        { new FloatRangeStruct(0.35854f, 0.3807f), (25, "Adenauer Forst")},
        { new FloatRangeStruct(0.3921f, 0.3981f), (-1, "Rebel Tree")},
        { new FloatRangeStruct(0.40f, 0.4086f), (26, "Metzgesfeld 1")},
        { new FloatRangeStruct(0.4096f, 0.4136433f), (27, "Metzgesfeld 2")},
        { new FloatRangeStruct(0.4196f, 0.4286f), (28, "Kallenhard")},
        { new FloatRangeStruct(0.4323f, 0.4405f), (29, "Spiegelkurve")},
        { new FloatRangeStruct(0.4405f, 0.4522f), (30, "Miss-hit-miss")},
        { new FloatRangeStruct(0.455f, 0.4652f), (31, "Wehrseifen")},
        { new FloatRangeStruct(0.476f, 0.483f), (32, "Breidscheid")},
        { new FloatRangeStruct(0.4865f, 0.493f), (33, "Ex-Mühle")},
        { new FloatRangeStruct(0.5046f, 0.5136f), (34, "Lauda Links")},
        { new FloatRangeStruct(0.5148f, 0.5252f), (35, "Bergwerk")},
        { new FloatRangeStruct(0.5529f, 0.5633f), (36, "Kesselchen")},
        { new FloatRangeStruct(0.5725f, 0.5893f), (37, "Klostertal")},
        { new FloatRangeStruct(0.593f, 0.6038f), (38, "Mutkurve")},
        { new FloatRangeStruct(0.6197f, 0.6316f), (39, "Steilstrecke")},
        { new FloatRangeStruct(0.6394f, 0.6479f), (40, "Caracciola Karussell")},
        { new FloatRangeStruct(0.6695f, 0.6858f), (41, "Hohe Acht")},
        { new FloatRangeStruct(0.6859f, 0.6928f), (42, "Hedwigshöhe")},
        { new FloatRangeStruct(0.6937f, 0.7082f), (43, "Wippermann")},
        { new FloatRangeStruct(0.7083f, 0.7255f), (44, "Eschbach")},
        { new FloatRangeStruct(0.7262f, 0.733f), (45, "Brünnchen")},
        { new FloatRangeStruct(0.7457f, 0.7542f), (46, "Eiskurve")},
        { new FloatRangeStruct(0.7586f, 0.7827f), (47, "Pflanzgarten 1")},
        { new FloatRangeStruct(0.7832f, 0.793f), (48, "Pflanzgarten 2")},
        { new FloatRangeStruct(0.7932f, 0.8106f), (49, "Stefan Bellof S")},
        { new FloatRangeStruct(0.8154f, 0.8327f), (50, "Schwalbenschwanz")},
        { new FloatRangeStruct(0.8344f, 0.8409f), (51, "Kleines Karussell")},
        { new FloatRangeStruct(0.849f, 0.8675f), (52, "Galgenkopf")},
        { new FloatRangeStruct(0.8690f, 0.9456f), (-1, "Döttinger Höhe")},
        { new FloatRangeStruct(0.9457247f, 0.957091f), (53, "Antoniusbuche")},
        { new FloatRangeStruct(0.95985f, 0.9741f), (54, "Tiergarten")},
        { new FloatRangeStruct(0.9691328f, 0.975f), (55, "Hohenrain")},
        { new FloatRangeStruct(0.9753f, 0.985f), (56, "Hohenrain Schikane")}
        
    };
    public override List<float> Sectors => new() { 0.32676908f, 0.641197f };
}
