using RaceElement.HUD.Overlay.OverlayUtil;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;
internal static class ShiftBarShapes
{
    public static CachedBitmap CreateBackground(Rectangle workingSpace) => new(workingSpace.Width, workingSpace.Height, g =>
    {
        int horizontalPadding = 2;
        int verticalPadding = 2;
        RectangleF barArea = new(horizontalPadding, verticalPadding, workingSpace.Width - horizontalPadding * 2, workingSpace.Height - verticalPadding * 2);

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using SolidBrush darkBrush = new(Color.FromArgb(90, Color.Black));
        g.FillRoundedRectangle(darkBrush, Rectangle.Round(barArea), 3);
        using Pen darkPen = new(darkBrush, 1);
        g.DrawRoundedRectangle(darkPen, Rectangle.Round(barArea), 3);
    });

    public static CachedBitmap CreateColoredBar(Rectangle workingSpace, RectangleF barSpace, Color primaryColor) => new(workingSpace.Width, workingSpace.Height, g =>
    {
        Rectangle area = Rectangle.Round(workingSpace);

        using LinearGradientBrush whiteToDarkGradientVertical = new(area, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(60, 20, 20, 20), -90f);
        g.FillRoundedRectangle(whiteToDarkGradientVertical, Rectangle.Round(barSpace), 3);

        Color secondaryColor = Color.FromArgb(170, primaryColor);

        using LinearGradientBrush blackToGreenGradient = new(area, secondaryColor, primaryColor, 0f);
        g.FillRoundedRectangle(blackToGreenGradient, Rectangle.Round(barSpace), 3);

        using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, Color.FromArgb(95, 75, 75, 75), secondaryColor);
        Rectangle hatchRect = Rectangle.Round(barSpace);
        int hatchPadding = 2;
        hatchRect.X = hatchRect.X + hatchPadding;
        hatchRect.Y = hatchRect.Y + hatchPadding;
        hatchRect.Width = hatchRect.Width - hatchPadding * 2;
        hatchRect.Height = hatchRect.Height - hatchPadding * 2;
        g.FillRoundedRectangle(hatchBrush, hatchRect, 3);
    });

    public static CachedBitmap CreateFlashBar(Rectangle workingSpace, RectangleF barSpace, ShiftBarConfiguration config) => new(workingSpace.Width, workingSpace.Height, g =>
    {
        Rectangle area = Rectangle.Round(workingSpace);

        using LinearGradientBrush whiteToDarkGradientVertical = new(area, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(60, 20, 20, 20), -90f);
        g.FillRoundedRectangle(whiteToDarkGradientVertical, Rectangle.Round(barSpace), 3);

        Color primaryColor = config.Colors.FlashColor;
        Color secondaryColor = Color.FromArgb(170, primaryColor);

        using LinearGradientBrush blackToGreenGradient = new(area, secondaryColor, primaryColor, 0f);
        g.FillRoundedRectangle(blackToGreenGradient, Rectangle.Round(barSpace), 3);

        using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, Color.FromArgb(95, 75, 75, 75), secondaryColor);
        Rectangle hatchRect = Rectangle.Round(barSpace);
        int hatchPadding = 1;
        hatchRect.X = hatchRect.X + hatchPadding;
        hatchRect.Y = hatchRect.Y + hatchPadding;
        hatchRect.Width = hatchRect.Width - hatchPadding * 2;
        hatchRect.Height = hatchRect.Height - hatchPadding * 2;
        g.FillRoundedRectangle(hatchBrush, hatchRect, 3);
    });

    public static CachedBitmap[] CreatePitLimiter(Rectangle workingSpace, RectangleF barSpace) => [
        new(workingSpace.Width, workingSpace.Height, g =>
        {
            using HatchBrush hatchBrush = new(HatchStyle.WideUpwardDiagonal, Color.FromArgb(170, 255, 255, 0), Color.FromArgb(90, Color.Black));
            Rectangle hatchRect = Rectangle.Round(barSpace);
            g.SetClip(hatchRect);
            int hatchPadding = 2;
            hatchRect.X = hatchRect.X + hatchPadding;
            hatchRect.Y = hatchRect.Y + hatchPadding;
            hatchRect.Width = hatchRect.Width - hatchPadding * 2;
            hatchRect.Height = hatchRect.Height - hatchPadding * 2;
            g.FillRoundedRectangle(hatchBrush, Rectangle.Round(barSpace), 3);
        }),
        new(workingSpace.Width, workingSpace.Height, g =>
         {
            using HatchBrush hatchBrush = new (HatchStyle.WideUpwardDiagonal, Color.FromArgb(90, 255, 255, 0), Color.FromArgb(90, Color.Black));
            Rectangle hatchRect = Rectangle.Round(barSpace);
            g.SetClip(hatchRect);
            int hatchPadding = 2;
            hatchRect.X = hatchRect.X + hatchPadding;
            hatchRect.Y = hatchRect.Y + hatchPadding;
            hatchRect.Width = hatchRect.Width - hatchPadding* 2;
            hatchRect.Height = hatchRect.Height - hatchPadding* 2;
            g.FillRoundedRectangle(hatchBrush, Rectangle.Round(barSpace), 3);
        })
    ];
}
