using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace WheelSpinner.Views
{
    public class SpinnerWheel : ContentView
    {
        public SpinnerWheel()
        {
            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();


            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Red.ToSKColor(),
                StrokeWidth = 25
            };

            var centerPoint = new SKPoint(info.Width / 2f, info.Height / 2f);
            var radius = Math.Min(info.Width / 2f, info.Height / 2f) * 0.8f;

            // Draw large circle
            canvas.DrawCircle(centerPoint, radius, paint);

            // Draw little circles
            var angle = 0.0f;
            var degreeToRadianFactor = Math.PI / 180;
            var littleCircleRadius = radius * 0.1f;
            for (int i = 0; i < 13; i++)
            {
                var littleCircleCenter = new SKPoint(
                                             (float) (radius * Math.Cos(angle * degreeToRadianFactor)),
                                             (float) (radius * Math.Sin(angle * degreeToRadianFactor))
                                         ) + centerPoint;

                canvas.DrawCircle(littleCircleCenter, littleCircleRadius, paint);
                angle += 30;
            }
        }
    }
}