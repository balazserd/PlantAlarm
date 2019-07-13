using System;
using System.Collections.Generic;
using System.Windows.Input;
using PlantAlarm.Interfaces;
using PlantAlarm.Services;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using TouchTracking;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public partial class AddButton : ContentView, IDrawnControlOnScrollableView
    {
        SKPaint BackgroundPaint = null;
        SKPaint LightBlueBackgroundPaint = new SKPaint
        {
            Color = SKColor.FromHsv(256.1f, 81f, 88.6f, 192),
            Style = SKPaintStyle.StrokeAndFill,
            IsAntialias = true,
            StrokeWidth = 0
        };
        SKPaint DarkBlueBackgroundPaint = new SKPaint
        {
            Color = SKColor.FromHsv(256.1f, 81f, 88.6f, 224),
            Style = SKPaintStyle.StrokeAndFill,
            IsAntialias = true,
            StrokeWidth = 0
        };
        SKPaint WhiteAddSignPaint = new SKPaint
        {
            Color = SKColor.FromHsv(0f, 0f, 100f),
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeWidth = 6,
            StrokeJoin = SKStrokeJoin.Round,
            StrokeCap = SKStrokeCap.Round
        };

        SKPoint[] Points;
        SKPath PathOfCross;

        private const float sizeX = 50;
        private const float sizeY = 50;

        public ICommand TouchedCommand { get; set; }
        public static readonly BindableProperty TouchedCommandProperty = BindableProperty.Create(
            propertyName: nameof(TouchedCommand),
            returnType: typeof(ICommand),
            declaringType: typeof(AddButton),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (b, o, n) =>
            {
                (b as AddButton).TouchedCommand = (ICommand)n;
            });

        public AddButton()
        {
            InitializeComponent();

            RemoveSelectedState();
        }

        public void ButtonTapped(object sender, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    {
                        AddSelectedState();
                        break;
                    }
                case TouchActionType.Exited:
                    {
                        RemoveSelectedState();
                        break;
                    }
                case TouchActionType.Released:
                    {
                        RemoveSelectedState();
                        TouchedCommand?.Execute(null);
                        break;
                    }
            }
        }

        public void ContainerCanvasRendered(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear();

            //resizing from dpi to pixels
            float xRatio = SkiaConverters.Dip2PixRatioX(info.Size, ContainerCanvas);
            float yRatio = SkiaConverters.Dip2PixRatioY(info.Size, ContainerCanvas);

            if (this.Points == null)
            {
                this.Points = new SKPoint[]
                {
                    new SKPoint(sizeX * 0.3f * xRatio, sizeY * 0.5f * yRatio), //left of cross
                    new SKPoint(sizeX * 0.5f * xRatio, sizeY * 0.7f * yRatio), //bottom ..
                    new SKPoint(sizeX * 0.7f * xRatio, sizeY * 0.5f * yRatio), //right .. 
                    new SKPoint(sizeX * 0.5f * xRatio, sizeY * 0.3f * yRatio), //top ..

                    new SKPoint(sizeX * 0.5f * xRatio, sizeY * 0.5f * yRatio)  //center of circle
                };
            }

            canvas.DrawOval(Points[4].X, Points[4].Y, sizeX * 0.5f * xRatio, sizeY * 0.5f * yRatio, this.BackgroundPaint);

            if (this.PathOfCross == null)
            {
                PathOfCross = new SKPath();
                PathOfCross.MoveTo(Points[0]);
                PathOfCross.LineTo(Points[2]);
                PathOfCross.MoveTo(Points[1]);
                PathOfCross.LineTo(Points[3]);
            }

            canvas.DrawPath(PathOfCross, this.WhiteAddSignPaint);
        }

        public void RemoveSelectedState()
        {
            this.BackgroundPaint = LightBlueBackgroundPaint;
            ContainerCanvas.InvalidateSurface();
        }

        public void AddSelectedState()
        {
            this.BackgroundPaint = DarkBlueBackgroundPaint;
            ContainerCanvas.InvalidateSurface();
        }
    }
}
