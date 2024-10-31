using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;

#if DEBUG
[Overlay(
    Name = "Shift Bar",
    Description = "A Fancy Bar"
)]
#endif
internal sealed class ShiftBarOverlay : CommonAbstractOverlay
{
    private readonly ShiftBarConfiguration _config = new();

    private CachedBitmap _cachedbackground;
    private CachedBitmap _cachedBar;

    private RectangleF WorkingSpace;
    private RectangleF BarSpace;

    private record struct ShiftBarDataModel(int Rpm, int MaxRpm);
    private ShiftBarDataModel _model;

    public ShiftBarOverlay(Rectangle rectangle) : base(rectangle, "Shift Bar")
    {
        WorkingSpace = new(0, 0, _config.Size.Width, _config.Size.Height);
        Width = (int)WorkingSpace.Width + 1;
        Height = (int)WorkingSpace.Height + 1;
        RefreshRateHz = _config.Render.RefreshRate;
    }

    public override void SetupPreviewData() => _model = new(8234, 9250);

    public override void BeforeStart()
    {
        int scaledWorkingWidth = (int)(Math.Ceiling(WorkingSpace.Width * Scale));
        int scaledWorkingHeight = (int)(Math.Ceiling(WorkingSpace.Height * Scale));

        RectangleF scaledWorkingSize = new(0, 0, scaledWorkingWidth, scaledWorkingHeight);

        int horizontalPadding = 2;
        int verticalPadding = 2;
        BarSpace = new(horizontalPadding, verticalPadding, scaledWorkingWidth - horizontalPadding * 2, scaledWorkingHeight - verticalPadding * 2);

        _cachedbackground = new(scaledWorkingWidth, scaledWorkingHeight, g =>
        {
            int horizontalPadding = 1;
            int verticalPadding = 1;
            RectangleF barArea = new(horizontalPadding, verticalPadding, scaledWorkingWidth - horizontalPadding * 2, scaledWorkingHeight - verticalPadding * 2);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using SolidBrush darkBrush = new(Color.FromArgb(90, Color.Black));
            g.FillRoundedRectangle(darkBrush, Rectangle.Round(barArea), 4);

            float lineDistance = barArea.Width / 10;
            for (int i = 1; i < 10; i++)
                g.DrawLine(Pens.Black, new PointF(i * lineDistance, verticalPadding * 2), new PointF(i * lineDistance, barArea.Height - verticalPadding));
        });

        _cachedBar = new(scaledWorkingWidth, scaledWorkingHeight, g =>
        {
            Rectangle area = Rectangle.Round(WorkingSpace);
            using LinearGradientBrush blackToGreenGradient = new(area, Color.FromArgb(170, 0, 170, 0), Color.FromArgb(170, 0, 255, 0), 0f);
            g.FillRoundedRectangle(blackToGreenGradient, Rectangle.Round(BarSpace), 4);

            using LinearGradientBrush whiteToDarkGradientVertical = new(area, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(60, 20, 20, 20), -90f);
            g.FillRoundedRectangle(whiteToDarkGradientVertical, Rectangle.Round(BarSpace), 4);

            using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, Color.FromArgb(95, 0, 20, 0), Color.Transparent);
            g.FillRoundedRectangle(hatchBrush, Rectangle.Round(BarSpace), 4);
        });
    }
    public override bool ShouldRender() => true;

    public override void Render(Graphics g)
    {
        int workingSpaceWidth = (int)WorkingSpace.Width;
        int workingSpaceHeight = (int)WorkingSpace.Height;

        _cachedbackground.Draw(g, 0, 0, workingSpaceWidth, workingSpaceHeight);

        if (!IsPreviewing)
        {
            _model.Rpm = SimDataProvider.LocalCar.Engine.Rpm;
            _model.MaxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;

            _model.Rpm = Random.Shared.Next(100, 9500);
            _model.MaxRpm = 9500;
        }

        RectangleF percented = new(BarSpace.ToVector4());
        double percent = 0;
        if (_model.Rpm > 0 && _model.MaxRpm > 0)
            percent = (double)_model.Rpm / _model.MaxRpm;
        percent.Clip(0, 1);
        percented.Width = (float)(BarSpace.Width * percent);
        g.SetClip(percented);
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        _cachedBar.Draw(g, 0, 0, workingSpaceWidth, workingSpaceHeight);
    }

}
