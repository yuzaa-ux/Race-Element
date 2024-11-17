using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftRpm;
[Overlay(
Name = "Shift RPM",
Description = "The current engine RPM as text")]
internal sealed class ShiftRpmOverlay : CommonAbstractOverlay
{
    private readonly ShiftRpmConfiguration _config = new();

    private CachedBitmap _cachedBackground;

    private RpmBitmaps _bitmaps;
    public ShiftRpmOverlay(Rectangle rectangle) : base(rectangle, "Shift RPM")
    {
        RefreshRateHz = 20;
    }

    public override void BeforeStart()
    {
        _bitmaps = new RpmBitmaps(_config.General.Font, _config.General.FontSize);
        Width = 5 * _bitmaps.Dimension.Width + _config.General.ExtraDigitSpacing * 4;
        Height = _bitmaps.Dimension.Height;

        _cachedBackground = new(Width, Height, g =>
        {
            RectangleF barArea = new(0, 0, Width - 1, Height - 1);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using SolidBrush darkBrush = new(Color.FromArgb(90, Color.Black));
            g.FillRoundedRectangle(darkBrush, Rectangle.Round(barArea), 3);
            using Pen darkPen = new(darkBrush, 1);
            g.DrawRoundedRectangle(darkPen, Rectangle.Round(barArea), 3);
        });
    }

    public override void BeforeStop()
    {
        _cachedBackground?.Dispose();
        _bitmaps?.Dispose();
    }

    public override void Render(Graphics g)
    {
        _cachedBackground?.Draw(g);

        int x = 0;
        for (int i = 0; i < 5; i++)
        {
            _bitmaps.GetForNumber((byte)Random.Shared.Next(0, 9)).Draw(g, new(x, 0));
            x += _bitmaps.Dimension.Width + _config.General.ExtraDigitSpacing;
        }
    }
}

internal sealed class RpmBitmaps : IDisposable
{
    private readonly CachedBitmap[] _rpmBitmaps = new CachedBitmap[10];
    public readonly (int Width, int Height) Dimension;
    public RpmBitmaps(ShiftRpmConfiguration.RpmTextFont font, float fontSize)
    {
        GenerateBitMaps(font, fontSize);
        Dimension = (_rpmBitmaps[0].Width, _rpmBitmaps[0].Height);
    }

    private void GenerateBitMaps(ShiftRpmConfiguration.RpmTextFont fontType, float fontSize)
    {
        Font font = fontType switch
        {
            ShiftRpmConfiguration.RpmTextFont.Conthrax => FontUtil.FontConthrax(fontSize),
            ShiftRpmConfiguration.RpmTextFont.Obitron => FontUtil.FontOrbitron(fontSize),
            ShiftRpmConfiguration.RpmTextFont.Roboto => FontUtil.FontRoboto(fontSize),
            _ => FontUtil.FontConthrax(fontSize),
        };

        StringFormat format = StringFormat.GenericDefault;
        format.Alignment = StringAlignment.Center;
        format.LineAlignment = StringAlignment.Center;
        format.FormatFlags = StringFormatFlags.NoClip;

        int bitmapWidth = (int)(fontSize + 4);
        int bitmapHeight = bitmapWidth + 4;
        for (int i = 0; i <= 9; i++)
        {
            _rpmBitmaps[i] = new(bitmapWidth, bitmapHeight, g =>
            {
                g.DrawStringWithShadow($"{i}", font, Color.White, new RectangleF(0, 2, bitmapWidth, bitmapHeight - 2), format);
            });
        }
    }
    public CachedBitmap GetForNumber(byte number)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 9);
        return _rpmBitmaps.AsSpan()[number];
    }

    public void Dispose()
    {
        foreach (CachedBitmap cachedBitmap in _rpmBitmaps)
            cachedBitmap?.Dispose();
    }
}

