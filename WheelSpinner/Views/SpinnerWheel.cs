using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace WheelSpinner.Views
{
    public class SpinnerWheel : ContentView
    {
        private const double DegreesToRadianFactor = Math.PI / 180;
        private const double RadianToDegreesFactor = 180 / Math.PI;

        public static readonly BindableProperty RotationAngleProperty = BindableProperty.Create("RotationAngleProperty",
            typeof(double),
            typeof(SpinnerWheel),
            0.0d,
            BindingMode.TwoWay,
            propertyChanged: RotationAngleChanged
        );

        private readonly SKCanvasView _canvasView;

        private readonly SKPaint _thickStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color.Red.ToSKColor(),
            StrokeWidth = 25,
            IsAntialias = true
        };

        private readonly SKPaint _thinStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color.Red.ToSKColor(),
            StrokeWidth = 8,
            IsAntialias = true
        };

        private readonly SKPaint _thinStrokeTextPaint = new SKPaint
        {
            IsStroke = false,
            Color = Color.Orchid.ToSKColor(),
            TextSize = 40.0f,
            TextAlign = SKTextAlign.Center,
            TextEncoding = SKTextEncoding.Utf16,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        private SKPoint _centerPoint;
        private SKPoint currentPoint;

        private SKPoint prevPoint;

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

        public double RotationAngle
        {
            get => (double) GetValue(RotationAngleProperty);
            set => SetValue(RotationAngleProperty, value);
        }

        private static void RotationAngleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as SpinnerWheel)?._canvasView.InvalidateSurface();
        }

        private void CanvasViewOnTouch(object sender, SKTouchEventArgs e)
        {
            Console.WriteLine($" {e.ActionType.ToString()} - ({e.Location.X}, {e.Location.Y})");

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    // Undo center-point translation
                    prevPoint = e.Location - _centerPoint;
                    break;
                case SKTouchAction.Moved:

                    // Undo center-point translation
                    currentPoint = e.Location - _centerPoint;

                    var dotProduct = currentPoint.X * prevPoint.X
                                     + currentPoint.Y * prevPoint.Y;
                    var vectorMagnitude = Math.Sqrt(Math.Pow(currentPoint.X, 2) + Math.Pow(currentPoint.Y, 2))
                                          * Math.Sqrt(Math.Pow(prevPoint.X, 2) + Math.Pow(prevPoint.Y, 2));
                    var angleRadians = Math.Acos(dotProduct / vectorMagnitude);

                    var angleDegrees = angleRadians * RadianToDegreesFactor;
                    RotationAngle += angleDegrees;


                    break;
                case SKTouchAction.Released:
                    break;
                case SKTouchAction.Cancelled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            e.Handled = true;
            _canvasView.InvalidateSurface();
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();

            _centerPoint = new SKPoint(info.Width / 2f, info.Height / 2f);

            // Account for desired rotation angle
            canvas.RotateDegrees((float) RotationAngle, _centerPoint.X, _centerPoint.Y);


            var radius = Math.Min(info.Width / 2f, info.Height / 2f) * 0.8f;

            // Draw large circle
            canvas.DrawCircle(_centerPoint, radius, _thickStrokePaint);

            // Draw little circles
            var angle = 0.0f;
            var littleCircleRadius = radius * 0.1f;
            for (var i = 1; i < 13; i++)
            {
                var littleCircleCenter = new SKPoint(
                                             (float) (radius * Math.Cos(angle * DegreesToRadianFactor)),
                                             (float) (radius * Math.Sin(angle * DegreesToRadianFactor))
                                         ) + _centerPoint;

                canvas.DrawCircle(littleCircleCenter, littleCircleRadius, _thinStrokePaint);

                var textLocation = littleCircleCenter + new SKPoint(0, littleCircleRadius / 2);
                canvas.DrawText($"{i.ToString()}", textLocation, _thinStrokeTextPaint);
                angle += 30;
            }
        }
    }
}