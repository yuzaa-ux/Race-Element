using RaceElement.Core.Jobs.Loop;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport.API;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

internal sealed class LowFuelMotorsportJob(string userId) : AbstractLoopJob
{
    public static readonly string DRIVER_RACE_API_URL = "https://api2.lowfuelmotorsport.com/api/licenseWidgetUserData/{0}";
    public static readonly string RACE_API_URL = "https://api3.lowfuelmotorsport.com/api/race/{0}";

    public EventHandler<ApiObject> OnNewApiObject;
    public EventHandler<RaceInfo> OnNewSplitObject;

    public override void RunAction()
    {
        if (userId.Trim().Length == 0)
        {
            return;
        }

        try
        {
            RaceInformation();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private static string GetContents(string url)
    {
        using HttpClient client = new();

        using HttpResponseMessage response = client.GetAsync(url).Result;
        using HttpContent content = response.Content;

        return content.ReadAsStringAsync().Result;
    }

    private void RaceInformation()
    {
        string json = GetContents(string.Format(DRIVER_RACE_API_URL, userId));
        ApiObject root = JObject.Parse(json).ToObject<ApiObject>();
        OnNewApiObject?.Invoke(null, root);

        if (root.Races.Count > 0)
        {
            SplitInformation(root.Races[0].Split, root.Races[0].RaceId, root.User.SteamId);
        }
    }

    private void SplitInformation(int split, int race, string id)
    {
        var json = JObject.Parse(GetContents(string.Format(RACE_API_URL, race)));
        var entries = json["splits"]["participants"][(split - 1)]["entries"];

        var list = new List<SplitEntry>();
        float kFactor = json["event"]["k_factor"].ToObject<float>();

        foreach (var e in entries)
        {
            var entry = new SplitEntry
            {
                IsPlayer = id == (string)e["steam_id"],
                RaceNumber = (int)e["raceNumber"],
                Elo = (int)e["elo"]
            };

            list.Add(entry);
        }

        OnNewSplitObject?.Invoke(null, new RaceInfo(kFactor, list));
    }
}
