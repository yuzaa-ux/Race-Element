using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Diagnostics;
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
    private readonly InfoPanel _upshiftDataPanel;

    private CachedBitmap _cachedBackground;
    private CachedBitmap _cachedRpmLines;
    private CachedBitmap _cachedFlashBar;

    private readonly List<CachedBitmap> _cachedColorBars = [];
    private readonly List<(float percentage, Color color)> _colors = [];

    private readonly Rectangle WorkingSpace;
    private RectangleF BarSpace;

    private record struct ShiftBarDataModel(int Rpm, int MaxRpm);
    private ShiftBarDataModel _model = new(0, 0);

    private MaxRpmDetectionJob _maxRpmDetectionJob;

    private readonly TimeSpan _redlineTime = TimeSpan.FromMilliseconds(64);
    private readonly TimeSpan _flashTime = TimeSpan.FromMilliseconds(32);
    public ShiftBarOverlay(Rectangle rectangle) : base(rectangle, "Shift Bar")
    {
        RefreshRateHz = _config.Bar.RefreshRate;
        WorkingSpace = new(0, 0, _config.Bar.Width, _config.Bar.Height);
        Width = (int)WorkingSpace.Width + 1;
        Height = (int)WorkingSpace.Height + 1;

        // increase base height and clipmin width to support upshift data panel
        if (_config.Upshift.DrawUpshiftData)
        {
            int panelMinWidth = 200;
            if (Width < panelMinWidth)
                Width = panelMinWidth;
            _upshiftDataPanel = new(11, panelMinWidth) { Y = Height + 1, FirstRowLine = 0 };
            Height += 3 * _upshiftDataPanel.FontHeight + 1;
        }
    }

    public sealed override void SetupPreviewData() => _model = new(_config.Upshift.PreviewRpm, _config.Upshift.MaxPreviewRpm);

    private (float earlyPercentage, float redlinePercentage) GetUpShiftPercentages()
    {
        // configured percentages (0-100%)
        float earlyPercentage = _config.Upshift.EarlyPercentage;
        float upshiftPercentage = _config.Upshift.RedlinePercentage;

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
        return (earlyPercentage, upshiftPercentage);
    }

    private void UpdateColorDictionary()
    {
        var percentages = GetUpShiftPercentages();

        _colors.Clear();
        _colors.Add((0.6f, Color.FromArgb(255, _config.Colors.NormalColor)));
        _colors.Add((percentages.earlyPercentage / 100f, Color.FromArgb(255, _config.Colors.EarlyColor)));
        _colors.Add((percentages.redlinePercentage / 100f, Color.FromArgb(255, _config.Colors.RedlineColor)));
    }
    public sealed override void BeforeStart()
    {
        UpdateColorDictionary();

        int horizontalBarPadding = 2;
        int verticalBarPadding = 2;
        BarSpace = new(horizontalBarPadding, verticalBarPadding, WorkingSpace.Width - horizontalBarPadding * 2, WorkingSpace.Height - verticalBarPadding * 2);

        _cachedBackground = new(WorkingSpace.Width, WorkingSpace.Height, g =>
        {
            int horizontalPadding = 2;
            int verticalPadding = 2;
            RectangleF barArea = new(horizontalPadding, verticalPadding, WorkingSpace.Width - horizontalPadding * 2, WorkingSpace.Height - verticalPadding * 2);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using SolidBrush darkBrush = new(Color.FromArgb(90, Color.Black));
            g.FillRoundedRectangle(darkBrush, Rectangle.Round(barArea), 3);
            using Pen darkPen = new(darkBrush, 1);
            g.DrawRoundedRectangle(darkPen, Rectangle.Round(barArea), 3);
        });

        _cachedColorBars.Clear();
        for (int i = 0; i < _colors.Count; i++)
            _cachedColorBars.Add(new(WorkingSpace.Width, WorkingSpace.Height, g =>
            {
                Rectangle area = Rectangle.Round(WorkingSpace);

                using LinearGradientBrush whiteToDarkGradientVertical = new(area, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(60, 20, 20, 20), -90f);
                g.FillRoundedRectangle(whiteToDarkGradientVertical, Rectangle.Round(BarSpace), 3);

                Color primaryColor = _colors.ElementAt(i).color;
                Color secondaryColor = Color.FromArgb(170, primaryColor);

                using LinearGradientBrush blackToGreenGradient = new(area, secondaryColor, primaryColor, 0f);
                g.FillRoundedRectangle(blackToGreenGradient, Rectangle.Round(BarSpace), 3);

                using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, Color.FromArgb(95, 75, 75, 75), secondaryColor);
                Rectangle hatchRect = Rectangle.Round(BarSpace);
                int hatchPadding = 2;
                hatchRect.X = hatchRect.X + hatchPadding;
                hatchRect.Y = hatchRect.Y + hatchPadding;
                hatchRect.Width = hatchRect.Width - hatchPadding * 2;
                hatchRect.Height = hatchRect.Height - hatchPadding * 2;
                g.FillRoundedRectangle(hatchBrush, hatchRect, 3);
            }));

        _cachedFlashBar = new(WorkingSpace.Width, WorkingSpace.Height, g =>
        {
            Rectangle area = Rectangle.Round(WorkingSpace);

            using LinearGradientBrush whiteToDarkGradientVertical = new(area, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(60, 20, 20, 20), -90f);
            g.FillRoundedRectangle(whiteToDarkGradientVertical, Rectangle.Round(BarSpace), 3);

            Color primaryColor = _config.Colors.FlashColor;
            Color secondaryColor = Color.FromArgb(170, primaryColor);

            using LinearGradientBrush blackToGreenGradient = new(area, secondaryColor, primaryColor, 0f);
            g.FillRoundedRectangle(blackToGreenGradient, Rectangle.Round(BarSpace), 3);

            using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, Color.FromArgb(95, 75, 75, 75), secondaryColor);
            Rectangle hatchRect = Rectangle.Round(BarSpace);
            int hatchPadding = 1;
            hatchRect.X = hatchRect.X + hatchPadding;
            hatchRect.Y = hatchRect.Y + hatchPadding;
            hatchRect.Width = hatchRect.Width - hatchPadding * 2;
            hatchRect.Height = hatchRect.Height - hatchPadding * 2;
            g.FillRoundedRectangle(hatchBrush, hatchRect, 3);
        });
        _cachedRpmLines = new(WorkingSpace.Width, WorkingSpace.Height, g =>
        {
            var model = _model;
            if (model.MaxRpm <= 0) return;

            int totalRpm = model.MaxRpm - _config.Data.HideRpm;
            totalRpm.ClipMin(_config.Data.MinVisibleRpm);

            int lineCount = (int)Math.Floor(totalRpm / 1000d);

            int leftOver = totalRpm % 1000;
            if (leftOver < 70)
                lineCount--;

            lineCount.ClipMin(0);
            if (lineCount == 0) return;
            using SolidBrush brush = new(Color.FromArgb(220, Color.Black));
            using Pen linePen = new(brush, 1.6f);

            double thousandPercent = 1000d / totalRpm * lineCount;
            if (thousandPercent == 0) return;
            double baseX = BarSpace.Width / lineCount * thousandPercent;
            for (int i = 1; i <= lineCount; i++)
            {
                float x = (float)(BarSpace.X + (i * baseX));
                g.DrawLine(linePen, x, 2, x, _config.Bar.Height - 2);
            }

            if (_config.Data.RedlineMarker)
            {
                var upshiftPercentages = GetUpShiftPercentages();
                double adjustedPercent = GetAdjustedPercentToHideRpm((int)(model.MaxRpm * upshiftPercentages.redlinePercentage / 100), model.MaxRpm, _config.Data.HideRpm, _config.Data.MinVisibleRpm);
                float x = (float)(BarSpace.X + (BarSpace.Width * adjustedPercent));
                g.DrawLine(Pens.Red, x, 4, x, _config.Bar.Height - 4);
            }
        });

        if (!IsPreviewing)
        {
            _maxRpmDetectionJob = new(this) { IntervalMillis = 1000 };
            _maxRpmDetectionJob.Run();
        }
    }

    public sealed override void BeforeStop()
    {
        _cachedBackground?.Dispose();
        _cachedRpmLines?.Dispose();
        _cachedFlashBar?.Dispose();

        foreach (var item in _cachedColorBars) item.Dispose();
        _cachedColorBars.Clear();

        _maxRpmDetectionJob?.CancelJoin();
        _upshiftDataPanel?.Dispose();
    }
    public sealed override bool ShouldRender() => true;

    // demo stuff
    private int shiftsDone = 0;
    private bool up = true;
    // demo stuff

    public sealed override void Render(Graphics g)
    {

        if (!IsPreviewing)
        {   // SET MODEL: Before release, uncomment line below and remove everything in-between the test data. it emulates the rpm going up 
            //_model.Rpm = SimDataProvider.LocalCar.Engine.Rpm;
            //_model.MaxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;

            // test data    ------------
            _model.MaxRpm = 11000;
            if (_model.Rpm < _model.MaxRpm / 3) _model.Rpm = _model.MaxRpm / 3;
            int increment = Random.Shared.Next(0, 2) == 1 ? Random.Shared.Next(0, 43) : -7;
            if (!up) increment *= -4;
            _model.Rpm = _model.Rpm + increment;
            if (up && _model.Rpm > _model.MaxRpm)
            {
                _model.Rpm = _model.MaxRpm - _model.MaxRpm / 4;
                shiftsDone++;
            }
            if (!up && _model.Rpm < _model.MaxRpm * _config.Upshift.EarlyPercentage / 100f - _model.MaxRpm / 5f)
            {
                _model.Rpm = _model.MaxRpm;
                shiftsDone++;
            }
            if (shiftsDone > 3)
            {
                up = !up;
                shiftsDone = 0;
            }

            // test data    ------------

            if (_model.Rpm < 0) _model.Rpm = 0;
            if (_model.Rpm > _model.MaxRpm) _model.Rpm = _model.MaxRpm;
        }

        _cachedBackground?.Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);
        DrawBar(g);
        _cachedRpmLines.Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);

        if (_config.Upshift.DrawUpshiftData)
        {
            _upshiftDataPanel.AddLine("Early", $"{_model.MaxRpm * _config.Upshift.EarlyPercentage / 100d:F1}");
            _upshiftDataPanel.AddLine("Redline", $"{_model.MaxRpm * _config.Upshift.RedlinePercentage / 100d:F1}");
            _upshiftDataPanel.AddLine("Max", $"{_model.MaxRpm:F1}");
            _upshiftDataPanel.Draw(g);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentRpm"></param>
    /// <param name="maxRpm"></param>
    /// <param name="hideRpm"></param>
    /// <permission cref="<see cref="Reinier Klarenberg"/>"
    /// <returns></returns>
    private static double GetAdjustedPercentToHideRpm(int currentRpm, int maxRpm, int hideRpm, int minVisibleRpm)
    {
        if (hideRpm > maxRpm - minVisibleRpm)
            hideRpm = maxRpm - minVisibleRpm;

        return (double)(currentRpm - hideRpm) / (maxRpm - hideRpm);
    }

    private bool _flashFlip;
    private long _lastFlash;
    private void DrawBar(Graphics g)
    {
        RectangleF percented = new(BarSpace.ToVector4());
        double rpmPercentage = 0;
        if (_model.Rpm > 0 && _model.MaxRpm > 0) rpmPercentage = (double)_model.Rpm / _model.MaxRpm;

        double adjustedPercent = GetAdjustedPercentToHideRpm(_model.Rpm, _model.MaxRpm, _config.Data.HideRpm, _config.Data.MinVisibleRpm);
        adjustedPercent.Clip(0.05f, 1);
        percented.Width = (float)(BarSpace.Width * adjustedPercent);

        int barIndex = GetCurrentColorBarIndex(rpmPercentage);
        g.SetClip(percented);
        if (barIndex < _colors.Count - 1)
            _cachedColorBars[barIndex].Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);
        else
        {
            (_flashFlip ? _cachedFlashBar : _cachedColorBars[barIndex]).Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);

            if (TimeProvider.System.GetElapsedTime(_lastFlash) > (_flashFlip ? _redlineTime : _flashTime))
            {
                _flashFlip = !_flashFlip;
                _lastFlash = TimeProvider.System.GetTimestamp();
            }
        }
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

    private sealed class MaxRpmDetectionJob(ShiftBarOverlay shiftBarOverlay) : AbstractLoopJob
    {
        private int _lastMaxRpm = -1;
        public sealed override void RunAction()
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
