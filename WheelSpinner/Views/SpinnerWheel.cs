using System;
using System.Collections.Generic;
using System.Linq;
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
            Color = Color.Aqua.ToSKColor(),
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

        private long touchId = -1;

        private void CanvasViewOnTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:

                    if (littleCircleBoundingRects.Any(rect => rect.Contains(e.Location - _centerPoint)))
                    {
                        touchId = e.Id;

                        // Save the touchdown location and undo center-point translation
                        prevPoint = e.Location - _centerPoint;
                    }

                    break;
                case SKTouchAction.Moved:

                    if (e.Id == touchId)
                    {
                        // Save the move location and undo center-point translation
                        currentPoint = e.Location - _centerPoint;

                        /// Compute the vector angle
                        var dotProduct = currentPoint.X * prevPoint.X
                                         + currentPoint.Y * prevPoint.Y;
                        var vectorMagnitude = Math.Sqrt(Math.Pow(currentPoint.X, 2) + Math.Pow(currentPoint.Y, 2))
                                              * Math.Sqrt(Math.Pow(prevPoint.X, 2) + Math.Pow(prevPoint.Y, 2));
                        // sanitize Acos arg
                        var ratio = dotProduct / vectorMagnitude;
                        ratio = Math.Min(ratio, 1.0d);
                        ratio = Math.Max(ratio, -1.0d);
                        var angleRadians = Math.Acos(ratio);

                        if (double.IsNaN(angleRadians)
                            || double.IsNegativeInfinity(angleRadians)
                            || double.IsPositiveInfinity(angleRadians))
                        {
                            Console.WriteLine("huh?");
                        }

                        var angleDegrees = angleRadians * RadianToDegreesFactor;
                        var clockwise = (prevPoint.X * currentPoint.Y - prevPoint.Y * currentPoint.X) > 0;

                        RotationAngle += clockwise ? angleDegrees : -angleDegrees;
                        RotationAngle = RotationAngle > 360.0d ? RotationAngle - 360d : RotationAngle;
                        RotationAngle = RotationAngle < -360.0d ? RotationAngle + 360d : RotationAngle;
                        if (double.IsNaN(RotationAngle))
                        {
                            Console.WriteLine("huh=");
                        }


                        prevPoint = currentPoint;
                    }


                    break;
                case SKTouchAction.Released:
                    currentPoint = SKPoint.Empty;
                    prevPoint = SKPoint.Empty;
                    touchId = -1;
                    break;
                case SKTouchAction.Cancelled:
                    touchId = -1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            e.Handled = true;

            Console.WriteLine($"Rotation angle: {RotationAngle}°");
            _canvasView.InvalidateSurface();
        }

        List<SKRect> littleCircleBoundingRects = new List<SKRect>();

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();

            _centerPoint = new SKPoint(info.Width / 2f, info.Height / 2f);

            // Move canvas origin to the center of the screen
            canvas.Translate(_centerPoint);

            // Set the desired rotation angle
//            canvas.RotateDegrees((float) RotationAngle);

            var radius = Math.Min(info.Width / 2f, info.Height / 2f) * 0.8f;

            // Draw large circle
            canvas.DrawCircle(new SKPoint(), radius, _thickStrokePaint);

            // Draw little circles
            var angleIncrement = 0.0f;
            var littleCircleRadius = radius * 0.1f;
            littleCircleBoundingRects.Clear();
            for (var i = 1; i < 13; i++)
            {
                var littleCircleCenter = new SKPoint(
                    (float) (radius * Math.Cos((RotationAngle + angleIncrement) * DegreesToRadianFactor)),
                    (float) (radius * Math.Sin((RotationAngle + angleIncrement) * DegreesToRadianFactor))
                );

                canvas.DrawCircle(littleCircleCenter, littleCircleRadius, _thinStrokePaint);
                littleCircleBoundingRects.Add(new SKRect(littleCircleCenter.X - (littleCircleRadius / 2f),
                    littleCircleCenter.Y - (littleCircleRadius / 2f),
                    littleCircleCenter.X + (littleCircleRadius / 2f),
                    littleCircleCenter.Y + (littleCircleRadius / 2f)
                ));

                var textLocation = littleCircleCenter + new SKPoint(0, littleCircleRadius / 2);
                canvas.DrawText($"{i.ToString()}", textLocation, _thinStrokeTextPaint);
                angleIncrement += 30;
            }
        }
    }
}