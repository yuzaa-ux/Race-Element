﻿using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACCManager.HUD.Overlay.Util;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.ACC.Overlays.OverlayPressureTrace;
using ACCManager.HUD.Overlay.OverlayUtil;
using static ACCManager.Data.SetupConverter;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayTyreInfo
{
    internal sealed class TyreInfoOverlay : AbstractOverlay
    {


        private TyreInfoConfig config = new TyreInfoConfig();
        private class TyreInfoConfig : OverlayConfiguration
        {
            [ToolTip("Shows progress bars and text displaying the percentage of brake pad life.")]
            public bool DrawPadLife { get; set; } = true;

            [ToolTip("Shows the average of front and rear brake temperatures.")]
            public bool DrawBrakeTemps { get; set; } = true;


            public TyreInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        private const double MaxPadLife = 29;

        public TyreInfoOverlay(Rectangle rectangle) : base(rectangle, "Tyre Info Overlay")
        {
            this.Width = 135;
            this.Height = 200;
            this.RefreshRateHz = 10;
        }

        public sealed override void BeforeStart()
        {
        }

        public sealed override void BeforeStop()
        {
        }

        public sealed override void Render(Graphics g)
        {
            DrawPressureBackgrounds(g);

        }

        private void DrawPressureBackgrounds(Graphics g)
        {
            TyrePressureRange range = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);

            if (range != null)
            {
                DrawPressureBackground(g, 0, 10, Wheel.FrontLeft, range);
                DrawPressureBackground(g, 76, 10, Wheel.FrontRight, range);
                DrawPressureBackground(g, 0, 179, Wheel.RearLeft, range);
                DrawPressureBackground(g, 76, 179, Wheel.RearRight, range);
            }

            if (this.config.DrawPadLife)
            {
                DrawPadWearProgressBar(g, 50, 45, Wheel.FrontLeft);
                DrawPadWearProgressBar(g, 81, 45, Wheel.FrontRight);
                DrawPadWearProgressBar(g, 50, 129, Wheel.RearLeft);
                DrawPadWearProgressBar(g, 81, 129, Wheel.RearRight);

                DrawPadWearText(g, 68, 29, Position.Front);
                DrawPadWearText(g, 68, 113, Position.Rear);
            }


            if (this.config.DrawBrakeTemps)
            {
                DrawBrakeTemps(g, 68, 81, Position.Front);
                DrawBrakeTemps(g, 68, 166, Position.Rear);
            }

        }

        private void DrawBrakeTemps(Graphics g, int x, int y, Position position)
        {
            Font fontFamily = FontUtil.FontOrbitron(9);

            double percentage = 0;
            switch (position)
            {
                case Position.Front:
                    {
                        float brakeTempLeft = pagePhysics.BrakeTemperature[(int)Wheel.FrontLeft];
                        float brakeTempRight = pagePhysics.BrakeTemperature[(int)Wheel.FrontRight];

                        float averageBrakeTemp = (brakeTempLeft + brakeTempRight) / 2;

                        percentage = averageBrakeTemp / MaxPadLife;
                        break;
                    }
                case Position.Rear:
                    {
                        float brakeTempLeft = pagePhysics.BrakeTemperature[(int)Wheel.RearRight];
                        float brakeTempRight = pagePhysics.BrakeTemperature[(int)Wheel.RearRight];

                        float averageBrakeTemp = (brakeTempLeft + brakeTempRight) / 2;

                        percentage = averageBrakeTemp / MaxPadLife;
                        break;
                    }
            }

            string text = $"{percentage * 100:F0} C";
            int textWidth = (int)g.MeasureString(text, fontFamily).Width;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, (int)textWidth, fontFamily.Height), 2);
            g.DrawString(text, fontFamily, Brushes.White, x - textWidth / 2, y);

            g.SmoothingMode = previous;
        }

        private void DrawPadWearText(Graphics g, int x, int y, Position position)
        {
            Font fontFamily = FontUtil.FontOrbitron(9);

            double percentage = 0;
            switch (position)
            {
                case Position.Front:
                    {
                        float padLifeLeft = pagePhysics.PadLife[(int)Wheel.FrontLeft];
                        float padLifeRight = pagePhysics.PadLife[(int)Wheel.FrontRight];

                        float averagePadLife = (padLifeLeft + padLifeRight) / 2;

                        percentage = averagePadLife / MaxPadLife;
                        break;
                    }
                case Position.Rear:
                    {
                        float padLifeLeft = pagePhysics.PadLife[(int)Wheel.RearRight];
                        float padLifeRight = pagePhysics.PadLife[(int)Wheel.RearRight];

                        float averagePadLife = (padLifeLeft + padLifeRight) / 2;

                        percentage = averagePadLife / MaxPadLife;
                        break;
                    }
            }

            string text = $"{percentage * 100:F0} %";
            int textWidth = (int)g.MeasureString(text, fontFamily).Width;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, (int)textWidth, fontFamily.Height), 2);
            g.DrawString(text, fontFamily, Brushes.White, x - textWidth / 2, y);

            g.SmoothingMode = previous;
        }

        private void DrawPadWearProgressBar(Graphics g, int x, int y, Wheel wheel)
        {

            float padLife = pagePhysics.PadLife[(int)wheel];

            double percentage = padLife / MaxPadLife;

            Brush padBrush = Brushes.Green;
            if (percentage < 0.66)
            {
                padBrush = Brushes.Orange;
            }
            if (percentage < 0.33)
            {
                padBrush = Brushes.Red;
            }

            VerticalProgressBar padBar = new VerticalProgressBar(0, MaxPadLife, padLife);
            padBar.Draw(g, x, y, 5, 35, padBrush, padBrush);

        }

        private void DrawPressureBackground(Graphics g, int x, int y, Wheel wheel, TyrePressureRange range)
        {
            Color brushColor = Color.FromArgb(80, 0, 255, 0);

            if (pagePhysics.WheelPressure[(int)wheel] >= range.OptimalMaximum)
                brushColor = Color.FromArgb(80, 255, 0, 0);

            if (pagePhysics.WheelPressure[(int)wheel] <= range.OptimalMinimum)
                brushColor = Color.FromArgb(80, 0, 0, 255);

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRoundedRectangle(new SolidBrush(brushColor), new Rectangle(x, y, 58, 20), 2);
            g.SmoothingMode = previous;
        }

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}
