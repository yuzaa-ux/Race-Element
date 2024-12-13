namespace RaceElement.HUD.Overlay.Configuration;
public sealed class LinkOption(string link)
{
    public string Link { get; init; } = link;

    public override string ToString()
    {
        return Link;
    }

}
