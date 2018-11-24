using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace WheelSpinner.Views
{
    public class SpinnerWheel : ContentView
    {
        private SKCanvasView _canvasView;

        public static readonly BindableProperty RotationAngleProperty = BindableProperty.Create("RotationAngleProperty",
            typeof(int),
            typeof(SpinnerWheel),
            0,
            BindingMode.TwoWay,
            propertyChanged: RotationAngleChanged
        );

        private static void RotationAngleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as SpinnerWheel)?._canvasView.InvalidateSurface();
        }

        public int RotationAngle
        {
            get => (int) GetValue(RotationAngleProperty);
            set => SetValue(RotationAngleProperty, value);
        }

        /**
         * 
         */
        public SpinnerWheel()
        {
            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = _canvasView;

            _canvasView.EnableTouchEvents = true;
            _canvasView.Touch += CanvasViewOnTouch;
        }

        private void CanvasViewOnTouch(object sender, SKTouchEventArgs e)
        {
            Console.WriteLine($" {e.ActionType.ToString()} - ({e.Location.X}, {e.Location.Y})");

            e.Handled = true;
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            var centerPoint = new SKPoint(info.Width / 2f, info.Height / 2f);

            // Account for desired rotation angle
            canvas.RotateDegrees(RotationAngle, centerPoint.X, centerPoint.Y);

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Red.ToSKColor(),
                StrokeWidth = 25,
                IsAntialias = true
            };


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