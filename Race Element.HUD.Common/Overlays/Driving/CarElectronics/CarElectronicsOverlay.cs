using System.Drawing;
using System.Drawing.Drawing2D;
using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;

namespace RaceElement.HUD.Common.Overlays.Driving.CarElectronics;


[Overlay(
    Name = "Car Electronics",
    Description = "Shows current Brake Bias, ABS and TC settings.",
    Game = Game.Automobilista2,
    Authors = ["Connor Molz"]
)]
public sealed class CarElectronicsOverlay : CommonAbstractOverlay
{
    private readonly CarElectronicsConfig _config = new();

    private sealed class CarElectronicsConfig : OverlayConfiguration
    {
        [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new InfoPanelGrouping();

        public sealed class InfoPanelGrouping
        {
            [ToolTip("Toggle ABS in overlay")]
            public bool ShowAbs { get; init; } = true;

            [ToolTip("Toggle TC in overlay")]
            public bool ShowTc { get; init; } = true;

            [ToolTip("Refresh rate in Hz of the HUD.")]
            [IntRange(1, 10, 2)]
            public int RefreshRate { get; init; } = 10;
        }

        public CarElectronicsConfig()
        {
            this.GenericConfiguration.AllowRescale = true;
        }
    }

    public CarElectronicsOverlay(Rectangle rectangle) : base(rectangle, "Car Electronics")
    {
        RefreshRateHz = _config.InfoPanel.RefreshRate;
    }

    // Window Components
    private Font _font;

    private PanelText _absHeader;
    private PanelText _absValue;
    private PanelText _tcHeader;
    private PanelText _tcValue;
    private PanelText _bbHeader;
    private PanelText _bbValue;

    // Before render function to setup the window and size
    public sealed override void BeforeStart()
    {
        _font = FontUtil.FontSegoeMono(10f * this.Scale);

        int lineHeight = _font.Height;

        int unscaledHeaderWidth = 40;
        int unscaledValueWidth = 58;

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
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(185, 0, 0, 0),
                Color.FromArgb(255, 10, 10, 10), LinearGradientMode.BackwardDiagonal);
            g.FillPath(brush, path);
            using Pen underlinePen = new(accentColor);
            g.DrawLine(underlinePen, 0 + roundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
        });

        CachedBitmap valueBackground = new(valueWidth, lineHeight, g =>
        {
            Rectangle panelRect = new(0, 0, valueWidth, lineHeight);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, roundingRadius, 0, 0);
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(185, 0, 0, 0),
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(brush, path);
            using Pen underlinePen = new(accentColor);
            g.DrawLine(underlinePen, 0, lineHeight - 1, valueWidth, lineHeight - 1);
        });

        // Init ABS, TC and BB headers and values
        _bbHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        _bbValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
        headerRect.Offset(0, lineHeight);
        valueRect.Offset(0, lineHeight);

        if (_config.InfoPanel.ShowTc)
        {
            _absHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _absValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        if (_config.InfoPanel.ShowTc)
        {
            _tcHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _tcValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }
    }

    public sealed override void Render(Graphics g)
    {
        int abs = SimDataProvider.LocalCar.Electronics.AbsLevel;
        int tc = SimDataProvider.LocalCar.Electronics.TractionControlLevel;
        string bb = SimDataProvider.LocalCar.Electronics.BrakeBias.ToString("F2");

        // Drawing the UI
        _bbHeader.Draw(g, "BB", this.Scale);
        _bbValue.Draw(g, bb, this.Scale);

        if (_config.InfoPanel.ShowAbs)
        {
            _absHeader.Draw(g, "ABS", this.Scale);
            _absValue.Draw(g, abs.ToString(), this.Scale);
        }

        if (_config.InfoPanel.ShowTc)
        {
            _tcHeader.Draw(g, "TC", this.Scale);
            _tcValue.Draw(g, tc.ToString(), this.Scale);
        }
    }

    public sealed override void BeforeStop()
    {
        _font?.Dispose();

        _bbHeader?.Dispose();
        _bbValue?.Dispose();
        _absHeader?.Dispose();
        _absValue?.Dispose();
        _tcHeader?.Dispose();
        _tcValue?.Dispose();
    }
}