using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace PlantAlarm.Services
{
    public static class SkiaConverters
    {
        public static float Pix2DipLength(this float f, float canvasLength, float xamLength)
        {
            return f * xamLength / canvasLength;
        }

        public static float Dip2PixLength(this float f, float canvasLength, float xamLength)
        {
            return f / xamLength * canvasLength;
        }

        public static float Dip2PixRatioX(SKSizeI canvasSize, SKCanvasView xamControl)
        {
            return canvasSize.Width / (float)xamControl.Width;
        }

        public static float Dip2PixRatioY(SKSizeI canvasSize, SKCanvasView xamControl)
        {
            return canvasSize.Height / (float)xamControl.Height;
        }

        public static float Pix2DipRatioX(SKSizeI canvasSize, SKCanvasView xamControl)
        {
            return 1 / Dip2PixRatioX(canvasSize, xamControl);
        }

        public static float Pix2DipRatioY(SKSizeI canvasSize, SKCanvasView xamControl)
        {
            return 1 / Dip2PixRatioY(canvasSize, xamControl);
        }
    }
}
