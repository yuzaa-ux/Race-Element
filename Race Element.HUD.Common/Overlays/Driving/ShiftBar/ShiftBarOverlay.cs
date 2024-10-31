using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;

//#if DEBUG
[Overlay(
    Name = "Shift Bar",
    Description = "A Fancy Bar"
)]
//#endif
internal sealed class ShiftBarOverlay : CommonAbstractOverlay
{
    private readonly ShiftBarConfiguration _config = new();

    private CachedBitmap _cachedBackground;
    private CachedBitmap _cachedRpmLines;

    private readonly List<CachedBitmap> _cachedColorBars = [];
    private readonly List<(float percentage, Color color)> _colors = [];

    private readonly Rectangle WorkingSpace;
    private RectangleF BarSpace;

    private record struct ShiftBarDataModel(int Rpm, int MaxRpm);
    private ShiftBarDataModel _model = new(0, 0);

    private MaxRpmDetectionJob _maxRpmDetectionJob;


    public ShiftBarOverlay(Rectangle rectangle) : base(rectangle, "Shift Bar")
    {
        WorkingSpace = new(0, 0, _config.Size.Width, _config.Size.Height);
        Width = (int)WorkingSpace.Width + 1;
        Height = (int)WorkingSpace.Height + 1;
        RefreshRateHz = _config.Render.RefreshRate;
    }

    public override void SetupPreviewData() => _model = new(8500, 9250);

    private void UpdateColorDictionary()
    {
        // configured percentages (0-100%)
        float earlyPercentage = _config.Upshift.Early;
        float upshiftPercentage = _config.Upshift.Upshift;

        if (GameWhenStarted.HasFlag(Game.RaceRoom))
        {
            float maxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;
            float upshiftRpm = SimDataProvider.LocalCar.Engine.ShiftUpRpm;
            if (maxRpm > 0 && upshiftRpm > 0)
            {
                upshiftPercentage = upshiftRpm * 100 / maxRpm;
                earlyPercentage = upshiftPercentage * 0.96f;
            }
        }

        _colors.Clear();
        _colors.Add((0.7f, Color.FromArgb(_config.Colors.NormalOpacity, _config.Colors.NormalColor)));
        _colors.Add((earlyPercentage / 100f, Color.FromArgb(_config.Colors.EarlyOpacity, _config.Colors.EarlyColor)));
        _colors.Add((upshiftPercentage / 100f, Color.FromArgb(_config.Colors.UpshiftOpacity, _config.Colors.UpshiftColor)));
        _colors.Add((upshiftPercentage / 100f, Color.FromArgb(_config.Colors.UpshiftOpacity, _config.Colors.UpshiftColor)));
    }
    public override void BeforeStart()
    {
        UpdateColorDictionary();

        int horizontalBarPadding = 2;
        int verticalBarPadding = 2;
        BarSpace = new(horizontalBarPadding, verticalBarPadding, WorkingSpace.Width - horizontalBarPadding * 2, WorkingSpace.Height - verticalBarPadding * 2);

        _cachedBackground = new(WorkingSpace.Width, WorkingSpace.Height, g =>
        {
            int horizontalPadding = 1;
            int verticalPadding = 1;
            RectangleF barArea = new(horizontalPadding, verticalPadding, WorkingSpace.Width - horizontalPadding * 2, WorkingSpace.Height - verticalPadding * 2);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using SolidBrush darkBrush = new(Color.FromArgb(90, Color.Black));
            g.FillRoundedRectangle(darkBrush, Rectangle.Round(barArea), 4);
        });

        _cachedColorBars.Clear();
        for (int i = 0; i < _colors.Count; i++)
            _cachedColorBars.Add(new(WorkingSpace.Width, WorkingSpace.Height, g =>
            {
                Rectangle area = Rectangle.Round(WorkingSpace);

                using LinearGradientBrush whiteToDarkGradientVertical = new(area, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(60, 20, 20, 20), -90f);
                g.FillRoundedRectangle(whiteToDarkGradientVertical, Rectangle.Round(BarSpace), 4);


                Color primaryColor = _colors.ElementAt(i).Item2;
                Color secondaryColor = Color.FromArgb(170, primaryColor);

                using LinearGradientBrush blackToGreenGradient = new(area, secondaryColor, primaryColor, 0f);
                g.FillRoundedRectangle(blackToGreenGradient, Rectangle.Round(BarSpace), 4);

                Color hatchColor = Color.FromArgb(95, primaryColor);
                using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, hatchColor/* Color.FromArgb(95, 0, 20, 0)*/, Color.Transparent);
                g.FillRoundedRectangle(hatchBrush, Rectangle.Round(BarSpace), 4);
            }));

        _cachedRpmLines = new(WorkingSpace.Width, WorkingSpace.Height, g =>
        {
            var tempModel = _model;
            int lineCount = (int)Math.Floor((tempModel.MaxRpm - _config.Data.HideRpm) / 1000d);

            int leftOver = (tempModel.MaxRpm - _config.Data.HideRpm) % 1000;
            if (leftOver < 70)
                lineCount--;

            lineCount.ClipMin(0);
            using SolidBrush brush = new(Color.FromArgb(220, Color.Black));
            using Pen linePen = new(brush, 1.6f * Scale);

            double thousandPercent = 1000d / (tempModel.MaxRpm - _config.Data.HideRpm) * lineCount;
            double baseX = _config.Size.Width * Scale / lineCount * thousandPercent;
            for (int i = 1; i <= lineCount; i++)
            {
                int x = (int)(i * baseX);
                g.DrawLine(linePen, x, 1, x, _config.Size.Height * Scale - 1);
            }
        });

        if (!IsPreviewing)
        {
            _maxRpmDetectionJob = new(this) { IntervalMillis = 1000 };
            _maxRpmDetectionJob.Run();
        }
    }

    public override void BeforeStop()
    {
        _maxRpmDetectionJob?.CancelJoin();
        _cachedBackground?.Dispose();
        _cachedRpmLines?.Dispose();
        foreach (var item in _cachedColorBars)
            item.Dispose();
        _cachedColorBars.Clear();
    }
    public override bool ShouldRender() => true;

    public override void Render(Graphics g)
    {
        _cachedBackground?.Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);

        if (!IsPreviewing)
        {
            _model.Rpm = SimDataProvider.LocalCar.Engine.Rpm;
            _model.MaxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;

            if (_model.Rpm < 0) _model.Rpm = 0;
            if (_model.Rpm > _model.MaxRpm) _model.Rpm = _model.MaxRpm;
        }

        DrawBar(g);

        _cachedRpmLines.Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);
    }

    private void DrawBar(Graphics g)
    {
        RectangleF percented = new(BarSpace.ToVector4());
        double rpmPercentage = 0;
        if (_model.Rpm > 0 && _model.MaxRpm > 0)
            rpmPercentage = (double)_model.Rpm / _model.MaxRpm;

        rpmPercentage.Clip(0, 1);
        percented.Width = (float)(BarSpace.Width * rpmPercentage);

        int barIndex = GetCurrentColorBarIndex(rpmPercentage);
        g.SetClip(percented);
        _cachedColorBars[barIndex].Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);
        g.ResetClip();
    }

    private int GetCurrentColorBarIndex(double percentage)
    {
        int barIndex = 0;
        var colorSpan = CollectionsMarshal.AsSpan(_colors);
        for (int i = 0; i < colorSpan.Length; i++)
        {
            if (percentage < colorSpan[i].percentage) break;
            barIndex = i;
        }
        return barIndex;
    }

    private class MaxRpmDetectionJob(ShiftBarOverlay shiftBarOverlay) : AbstractLoopJob
    {
        private int _lastMaxRpm = -1;
        public override void RunAction()
        {
            var model = shiftBarOverlay._model;
            if (_lastMaxRpm != model.MaxRpm)
            {
                shiftBarOverlay?._cachedRpmLines.Render();
                shiftBarOverlay?.UpdateColorDictionary();
                _lastMaxRpm = model.MaxRpm;
            }
        }
    }
}
