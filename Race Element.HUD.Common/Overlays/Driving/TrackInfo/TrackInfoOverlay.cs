using System.Drawing;
using System.Drawing.Drawing2D;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;

namespace RaceElement.HUD.Common.Overlays.Driving.TrackInfo;

[Overlay(
    Name = "Track Bar",
    Description = "A bar displaying a flat and zoomed in version of the Track Circle HUD.",
    Game = Game.Automobilista2,
    Authors = ["Reinier Klarenberg", "Connor Molz"]
)]
public class TrackInfoOverlay: CommonAbstractOverlay
{
    private readonly TrackInfoConfig _config = new();
    private sealed class TrackInfoConfig : OverlayConfiguration
    {
        [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new InfoPanelGrouping();
        public sealed class InfoPanelGrouping
        {

            [ToolTip("Shows the global track flag.")]
            public bool GlobalFlag { get; init; } = true;

            [ToolTip("Shows the type of the session.")]
            public bool SessionType { get; set; } = true;

            [ToolTip("Displays the track temperature")]
            public bool TrackTemperature { get; init; } = true;
        }

        public TrackInfoConfig()
        {
            this.GenericConfiguration.AllowRescale = true;
        }
    }

    private Font _font;
    
    private PanelText _globalFlagHeader;
    private PanelText _globalFlagValue;
    private PanelText _sessionTypeLabel;
    private PanelText _sessionTypeValue;
    private PanelText _airTempLabel;
    private PanelText _airTempValue;
    private PanelText _trackTempLabel;
    private PanelText _trackTempValue;
    private PanelText _windLabel;
    private PanelText _windValue;
    
    
    public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info")
    {
        RefreshRateHz = 1;
    }

    public sealed override void BeforeStart()
    {
        _font = FontUtil.FontSegoeMono(10f * this.Scale);

        int lineHeight = _font.Height;

        int unscaledHeaderWidth = 66;
        int unscaledValueWidth = 94;

        int headerWidth = (int)(unscaledHeaderWidth * this.Scale);
        int valueWidth = (int)(unscaledValueWidth * this.Scale);
        int roundingRadius = (int)(6 * this.Scale);

        RectangleF headerRect = new(0, 0, headerWidth, lineHeight);
        RectangleF valueRect = new(headerWidth, 0, valueWidth, lineHeight);
        StringFormat headerFormat = new() { Alignment = StringAlignment.Near };
        StringFormat valueFormat = new() { Alignment = StringAlignment.Far };

        Color accentColor = Color.FromArgb(25, 255, 0, 0);
        CachedBitmap headerBackground = new(headerWidth, lineHeight, g =>
        {
            Rectangle panelRect = new(0, 0, headerWidth, lineHeight);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, 0, 0, roundingRadius);
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(185, 0, 0, 0), Color.FromArgb(255, 10, 10, 10), LinearGradientMode.BackwardDiagonal);
            g.FillPath(brush, path);
            using Pen underlinePen = new(accentColor);
            g.DrawLine(underlinePen, 0 + roundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
        });
        CachedBitmap valueBackground = new(valueWidth, lineHeight, g =>
        {
            Rectangle panelRect = new(0, 0, valueWidth, lineHeight);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, roundingRadius, 0, 0);
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(185, 0, 0, 0), LinearGradientMode.ForwardDiagonal);
            g.FillPath(brush, path);
            using Pen underlinePen = new(accentColor);
            g.DrawLine(underlinePen, 0, lineHeight - 1, valueWidth, lineHeight - 1);
        });

        if (this._config.InfoPanel.GlobalFlag)
        {
            _globalFlagHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _globalFlagValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        if (this._config.InfoPanel.SessionType)
        {
            _sessionTypeLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _sessionTypeValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }
        
        _airTempLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        _airTempValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
        headerRect.Offset(0, lineHeight);
        valueRect.Offset(0, lineHeight);

        if (this._config.InfoPanel.TrackTemperature)
        {
            _trackTempLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _trackTempValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        _windLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        _windValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
        headerRect.Offset(0, lineHeight);
        valueRect.Offset(0, lineHeight);

        this.Width = unscaledHeaderWidth + unscaledValueWidth;
        this.Height = (int)(headerRect.Top / this.Scale);
    }

    public sealed override void BeforeStop()
    {
        _font?.Dispose();

        _globalFlagHeader?.Dispose();
        _globalFlagValue?.Dispose();
        _sessionTypeLabel?.Dispose();
        _sessionTypeValue?.Dispose();
        _airTempLabel?.Dispose();
        _airTempValue?.Dispose();
        _trackTempLabel?.Dispose();
        _trackTempValue?.Dispose();
        _windLabel?.Dispose();
        _windValue?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        SessionData session = SessionData.Instance;

        if (this._config.InfoPanel.GlobalFlag)
        {
            string flag = session.CurrentFlag.ToString();
            _globalFlagHeader.Draw(g, "Flag", this.Scale);
            _globalFlagValue.Draw(g, $"{flag}", this.Scale);
        }

        if (this._config.InfoPanel.SessionType)
        {
            string sessionName = session.SessionType.ToString();
            _sessionTypeLabel.Draw(g, "Session", this.Scale);
            _sessionTypeValue.Draw(g, $"{sessionName}", this.Scale);
        }

        string airTemp = session.Weather.AirTemperature.ToString("F3");
        _airTempLabel.Draw(g, "Air", this.Scale);
        _airTempValue.Draw(g, $"{airTemp} °C", this.Scale);

        if (this._config.InfoPanel.TrackTemperature)
        {
            string roadTemp = session.Track.Temperature.ToString("F3");
            _trackTempLabel.Draw(g, "Track", this.Scale);
            _trackTempValue.Draw(g, $"{roadTemp} °C", this.Scale);
        }
        
        string windSpeed = session.Weather.AirDirection.ToString();
        _windLabel.Draw(g, "Wind", this.Scale);
        _windValue.Draw(g, $"{windSpeed} km/h", this.Scale);
    }
}