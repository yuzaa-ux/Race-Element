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
        Width = (int)WorkingSpace.Width;
        Height = (int)WorkingSpace.Height;
        RefreshRateHz = _config.Render.RefreshRate;
    }

    public override void SetupPreviewData()
    {
        _model = new(8234, 9250);
    }

    public override void BeforeStart()
    {
        int scaledWorkingWidth = (int)(WorkingSpace.Width * Scale);
        int scaledWorkingHeight = (int)(WorkingSpace.Height * Scale);

        RectangleF scaledWorkingSize = new(0, 0, scaledWorkingWidth, scaledWorkingHeight);

        _cachedbackground = new(scaledWorkingWidth, scaledWorkingHeight, g =>
        {
            int horizontalPadding = 2;
            int verticalPadding = 1;
            RectangleF barArea = new(horizontalPadding, verticalPadding, scaledWorkingWidth - horizontalPadding * 2, scaledWorkingHeight - verticalPadding * 2);

            g.FillRectangle(Brushes.Black, barArea);
        });

        int horizontalPadding = 6;
        int verticalPadding = 2;
        BarSpace = new(horizontalPadding, verticalPadding, scaledWorkingWidth - horizontalPadding * 2, scaledWorkingHeight - verticalPadding * 2);
        _cachedBar = new(scaledWorkingWidth, scaledWorkingHeight, g =>
        {
            Rectangle area = Rectangle.Round(WorkingSpace);
            using LinearGradientBrush gradientBrush = new(area, Color.FromArgb(160, 0, 20, 0), Color.FromArgb(130, 0, 255, 0), 0f);
            g.FillRectangle(gradientBrush, BarSpace);
            using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, Color.FromArgb(95, 0, 20, 0), Color.Transparent);
            g.FillRectangle(hatchBrush, BarSpace);
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
        }

        RectangleF percented = new RectangleF(BarSpace.ToVector4());
        double percent = 0;
        if (_model.Rpm > 0 && _model.MaxRpm > 0)
            percent = (double)_model.Rpm / _model.MaxRpm;
        percent.Clip(0, 1);
        percented.Width = (float)(BarSpace.Width * percent);
        g.SetClip(percented);
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        _cachedBar.Draw(g, 0, 0, workingSpaceWidth, workingSpaceHeight);
    }

}
