using System;
using System.Collections.Generic;
using PlantAlarm.Interfaces;
using PlantAlarm.Services;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public partial class WaterDropControl : ContentView
    {
        //Coordinates as if the total height were 1 and everything was compared to that.
        //Y coords
        private static readonly float totalBaseHeight = 5 + 1.44f + (float)Math.Sqrt(1.8 * 1.8 + 1.44 * 1.44); //Total height
        private static readonly float y0 = 0 / totalBaseHeight; //Bezier start (right and left)
        private static readonly float y1 = 2 / totalBaseHeight; //Bezier p1 (right and left)
        private static readonly float y2 = 4 / totalBaseHeight; //Bezier p2 (right and left)
        private static readonly float y3 = 4.94f / totalBaseHeight; //Bezier end (right and left)
        private static readonly float y4 = (totalBaseHeight - 2 * ((float)Math.Sqrt(1.8 * 1.8 + 1.44 * 1.44) + 0.11f)) / totalBaseHeight; //Circle rect upper left point
        private static readonly float y5 = 1; //Circle rect lower right point

        //X coords
        private static readonly float x0 = 0.5f; //Bezier start (right and left)
        private static readonly float x1 = 0.5f; //Bezier p1 (right and left)
        private static readonly float x2_l = x0 - 1 / totalBaseHeight; //Bezier p2 (left)
        private static readonly float x2_r = x0 + 1 / totalBaseHeight; //Bezier p2 (right)
        private static readonly float x3_l = x0 - 1.97f / totalBaseHeight; //Bezier end (left)
        private static readonly float x3_r = x0 + 1.97f / totalBaseHeight; //Bezier end (right)
        private static readonly float x4 = x0 - ((float)Math.Sqrt(1.8 * 1.8 + 1.44 * 1.44) + 0.11f) / totalBaseHeight; //Circle rect upper left point
        private static readonly float x5 = x0 + ((float)Math.Sqrt(1.8 * 1.8 + 1.44 * 1.44) + 0.11f) / totalBaseHeight; //Circle rect lower right point

        //Radius & angle
        private static readonly float rad = (float)Math.Sqrt(1.8 * 1.8 + 1.44 * 1.44);
        private static readonly float startAngle = (float)(Math.Atan(1.8 / 1.44) * 180 / Math.PI);

        private readonly SKPaint paint;

        private SKPoint[] waterDropPoints;
        private SKPath pathToDraw;

        private static readonly float sizeX = 50;
        private static readonly float sizeY = 50;

        public WaterDropControl() : this(1) { }

        public WaterDropControl(double percentage)
        {
            InitializeComponent();

            paint = new SKPaint
            {
                Color = SKColor.FromHsv((float)(239 * percentage), (float)(79 + 2 * percentage), 100f),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeWidth = 3,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeCap = SKStrokeCap.Round
            };
        }

        public void WaterDropCanvasRendered(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear();

            //resizing from dpi to pixels
            float xRatio = SkiaConverters.Dip2PixRatioX(info.Size, ContainerCanvas);
            float yRatio = SkiaConverters.Dip2PixRatioY(info.Size, ContainerCanvas);

            if (waterDropPoints == null)
            {
                waterDropPoints = new SKPoint[]
                {
                    new SKPoint(sizeX * x0 * xRatio, sizeY * y0 * yRatio),
                    new SKPoint(sizeX * x1 * xRatio, sizeY * y1 * yRatio),
                    new SKPoint(sizeX * x2_l * xRatio, sizeY * y2 * yRatio),
                    new SKPoint(sizeX * x2_r * xRatio, sizeY * y2 * yRatio),
                    new SKPoint(sizeX * x3_l * xRatio, sizeY * y3 * yRatio),
                    new SKPoint(sizeX * x3_r * xRatio, sizeY * y3 * yRatio),
                    new SKPoint(sizeX * x4 * xRatio, sizeY * y4 * yRatio)
                };
            }

            if (pathToDraw == null)
            {
                pathToDraw = new SKPath();
                pathToDraw.MoveTo(waterDropPoints[0]);
                pathToDraw.CubicTo(waterDropPoints[1], waterDropPoints[2], waterDropPoints[4]);
                pathToDraw.MoveTo(waterDropPoints[0]);
                pathToDraw.CubicTo(waterDropPoints[1], waterDropPoints[3], waterDropPoints[5]);
                pathToDraw.ArcTo(new SKRect(x4 * sizeX * xRatio, y4 * sizeY * yRatio, x5 * sizeX * xRatio, y5 * sizeY * yRatio), startAngle - 90, 360 - 2 * startAngle, false);
            }

            canvas.DrawPath(pathToDraw, paint);
        }

        void Handle_TouchAction(object sender, TouchTracking.TouchActionEventArgs args)
        {
            //
        }
    }
}
