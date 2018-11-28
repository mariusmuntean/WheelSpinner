using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace WheelSpinner.Views
{
    public class SpinnerWheel : ContentView
    {
        private const double DegreesToRadianFactor = Math.PI / 180;
        private const double RadianToDegreesFactor = 180 / Math.PI;

        #region PROPERTIES

        public static readonly BindableProperty GoLeftCommandProperty = BindableProperty.Create("GoLeftCommand",
            typeof(ICommand),
            typeof(SpinnerWheel));

        public ICommand GoLeftCommand
        {
            get => (ICommand)GetValue(GoLeftCommandProperty);
            set => SetValue(GoLeftCommandProperty, value);
        }

        public static readonly BindableProperty GoRightCommandProperty = BindableProperty.Create("GoRightCommand",
            typeof(ICommand),
            typeof(SpinnerWheel));

        public ICommand GoRightCommand
        {
            get => (ICommand)GetValue(GoRightCommandProperty);
            set => SetValue(GoRightCommandProperty, value);
        }

        public static readonly BindableProperty RotationAngleProperty = BindableProperty.Create("RotationAngle",
            typeof(double),
            typeof(SpinnerWheel),
            0.0d,
            BindingMode.TwoWay,
            propertyChanged: RotationAngleChanged
        );

        public double RotationAngle
        {
            get => (double)GetValue(RotationAngleProperty);
            set => SetValue(RotationAngleProperty, value);
        }

        private static void RotationAngleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as SpinnerWheel)?._canvasView.InvalidateSurface();
        }

        public static readonly BindableProperty ItemsProperty = BindableProperty.Create("Items",
            typeof(List<object>),
            typeof(SpinnerWheel),
            new List<object>() { "a", "b", "c" }, propertyChanged: ItemsChanged);

        public List<object> Items
        {
            get => (List<object>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        private static void ItemsChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as SpinnerWheel)?._canvasView.InvalidateSurface();
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create("SelectedItem",
            typeof(object),
            typeof(SpinnerWheel),
            null,
            BindingMode.TwoWay);

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        #endregion PROPERTIES

        private readonly SKPaint _thickStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color.FromHex("#AAFF6600").ToSKColor(),
            StrokeWidth = 8,
            IsAntialias = true
        };

        private readonly SKPaint _thinStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color.FromHex("#AAFF6600").ToSKColor(),
            StrokeWidth = 4,
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

        private readonly SKCanvasView _canvasView;

        private SKPoint _centerPoint;

        private SKPoint _currentPoint;

        private SKPoint _prevPoint;

        public SpinnerWheel()
        {
            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = _canvasView;

            _canvasView.EnableTouchEvents = true;
            _canvasView.Touch += CanvasViewOnTouch;

            GoLeftCommand = new Command(() =>
            {
                var rotationAnimation = new Animation(rotationAngle =>
                    {
                        RotationAngle = rotationAngle;
                        _canvasView.InvalidateSurface();
                    },
                    RotationAngle, RotationAngle + 60.0d,
                    Easing.CubicInOut);
                rotationAnimation.Commit(this, "LeftRotationAnimation", 1000 / 60, 300);
            });

            GoRightCommand = new Command(() =>
            {
                var rotationAnimation = new Animation(rotationAngle =>
                    {
                        RotationAngle = rotationAngle;
                        _canvasView.InvalidateSurface();
                    },
                    RotationAngle, RotationAngle - 60.0d,
                    Easing.CubicInOut);
                rotationAnimation.Commit(this, "RightRotationAnimation", 1000 / 60, 300);
            });

            _selectedIndex = 0;
        }

        private long _touchId = -1;
        private int _pressedIdx = -1;
        private int _selectedIndex = -1;

        private void CanvasViewOnTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:

                    var pressedContentBoundingRect =
                        _littleCircleBoundingRects.FirstOrDefault(rect => rect.Contains(e.Location - _centerPoint));
                    if (pressedContentBoundingRect != SKRect.Empty)
                    {
                        _touchId = e.Id;
                        _pressedIdx = _littleCircleBoundingRects.IndexOf(pressedContentBoundingRect);

                        // Save the touchdown location and undo center-point translation
                        _prevPoint = e.Location - _centerPoint;
                    }

                    break;

                case SKTouchAction.Moved:

                    if (e.Id == _touchId)
                    {
                        // Save the move location and undo center-point translation
                        _currentPoint = e.Location - _centerPoint;

                        // Compute the vector angle
                        var dotProduct = _currentPoint.X * _prevPoint.X
                                         + _currentPoint.Y * _prevPoint.Y;
                        var vectorMagnitude = Math.Sqrt(Math.Pow(_currentPoint.X, 2) + Math.Pow(_currentPoint.Y, 2))
                                              * Math.Sqrt(Math.Pow(_prevPoint.X, 2) + Math.Pow(_prevPoint.Y, 2));
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
                        var clockwise = (_prevPoint.X * _currentPoint.Y - _prevPoint.Y * _currentPoint.X) > 0;

                        RotationAngle += clockwise ? angleDegrees : -angleDegrees;
                        RotationAngle = RotationAngle > 360.0d ? RotationAngle - 360d : RotationAngle;
                        RotationAngle = RotationAngle < -360.0d ? RotationAngle + 360d : RotationAngle;
                        if (double.IsNaN(RotationAngle))
                        {
                            Console.WriteLine("huh=");
                        }

                        _prevPoint = _currentPoint;
                    }

                    break;

                case SKTouchAction.Released:
                    _currentPoint = SKPoint.Empty;
                    _prevPoint = SKPoint.Empty;

                    _touchId = -1;
                    _pressedIdx = -1;

                    SnapToClosestSlot();

                    break;

                case SKTouchAction.Cancelled:
                    _touchId = -1;
                    _pressedIdx = -1;
                    break;

                default:
                    {
                        Console.WriteLine($"SKTouchAction: {e.ActionType}");
                        //throw new ArgumentOutOfRangeException();
                        break;
                    }
            }

            e.Handled = true;

            //            Console.WriteLine($"Rotation angle: {RotationAngle}°");
            _canvasView.InvalidateSurface();
        }

        private void SnapToClosestSlot()
        {
            var slotCount = 6;
            var slotAngle = 360.0d / slotCount;

            var prevSlotIndex = Math.Floor(RotationAngle / slotAngle);
            var nextSlotIndex = Math.Ceiling(RotationAngle / slotAngle);

            var prevSlotAngle = prevSlotIndex * slotAngle;
            var nextSlotAngle = nextSlotIndex * slotAngle;

            var prevSlotAngleDelta = Math.Abs(RotationAngle - prevSlotAngle);
            var nextSlotAngleDelta = Math.Abs(RotationAngle - nextSlotAngle);

            var destinationAngle = prevSlotAngleDelta < nextSlotAngleDelta ? prevSlotAngle : nextSlotAngle;

            var animateToAngle = new Animation(angle =>
                {
                    RotationAngle = angle;
                    _canvasView.InvalidateSurface();
                }, RotationAngle, destinationAngle,
                Easing.CubicInOut);

            animateToAngle.Commit(this, "SnapAnimation", 1000 / 60, 300, finished: (d, b) =>
            {
                var closestPairTopDelta = _hitAreaRectToItemMap
                    .Where(pair => pair.Key.Left < 0.0f)
                    .Min(pair => Math.Abs(pair.Key.Top));
                var closestPair = _hitAreaRectToItemMap
                    .FirstOrDefault(pair => pair.Key.Left < 0.0f
                                            && Math.Abs(Math.Abs(pair.Key.Top) - closestPairTopDelta) < 0.1f);

                _selectedIndex = _littleCircleBoundingRects.IndexOf(closestPair.Key) % 3;
                _canvasView.InvalidateSurface();
                SelectedItem = closestPair.Value;
            });
        }

        private List<SKRect> _littleCircleBoundingRects = new List<SKRect>();
        private Dictionary<SKRect, object> _hitAreaRectToItemMap = new Dictionary<SKRect, object>();

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();

            _centerPoint = new SKPoint(info.Width, info.Height / 2f);

            // Move canvas origin to middle of the screen's right edge
            canvas.Translate(info.Width, info.Height / 2f);

            // Draw large circle
            var radius = Math.Min(info.Width, info.Height / 2f) * 0.8f;
            canvas.DrawCircle(new SKPoint(), radius, _thinStrokePaint);

            // Draw little circles
            var rotationAngleOffset = 240.0f;
            var rotationAngleOffsetIncrement = 60;
            var littleCircleRadius = radius * 0.2f;
            
            _littleCircleBoundingRects.Clear();
            _hitAreaRectToItemMap.Clear();
            
            for (var currentItemIndex = 0; currentItemIndex < 6; currentItemIndex++)
            {
                canvas.Save();

                // Compute the current item center
                var currentItemCenter = new SKPoint(
                    (float)(radius * Math.Cos((RotationAngle + rotationAngleOffset) * DegreesToRadianFactor)),
                    (float)(radius * Math.Sin((RotationAngle + rotationAngleOffset) * DegreesToRadianFactor))
                );
                canvas.Translate(currentItemCenter);

                // Draw the current item
                if (currentItemIndex == _pressedIdx) // Highlight if pressed
                {
                    canvas.Scale(1.1f);
                    _thickStrokePaint.Color = _thickStrokePaint.Color.WithAlpha(255);
                    _thinStrokeTextPaint.Color = _thinStrokeTextPaint.Color.WithAlpha(255);
                }
                else if (currentItemIndex % 3 == _selectedIndex) // Highlight if item is currently selected
                {
                    canvas.Scale(1.2f);
                    _thickStrokePaint.Color = _thickStrokePaint.Color.WithAlpha(255);
                    _thinStrokeTextPaint.Color = _thinStrokeTextPaint.Color.WithAlpha(255);
                }
                else // draw normally
                {
                    _thickStrokePaint.Color = _thickStrokePaint.Color.WithAlpha(128);
                    _thinStrokeTextPaint.Color = _thinStrokeTextPaint.Color.WithAlpha(128);
                }

                canvas.DrawCircle(0, 0, littleCircleRadius, _thickStrokePaint);
                
                // Compute hit area of current item
                var hitAreaRect = new SKRect(currentItemCenter.X - (littleCircleRadius),
                    currentItemCenter.Y - (littleCircleRadius),
                    currentItemCenter.X + (littleCircleRadius),
                    currentItemCenter.Y + (littleCircleRadius)
                );
                _littleCircleBoundingRects.Add(hitAreaRect);
                _hitAreaRectToItemMap.Add(hitAreaRect, Items[currentItemIndex % 3]);

                // Draw something to distinguish the items
                var text = $"{((currentItemIndex % 3) + 1).ToString()}";
                var textRect = new SKRect();
                _thinStrokePaint.MeasureText(text, ref textRect);
                var textLocation = new SKPoint(0, textRect.Height);
                canvas.DrawText(text, textLocation, _thinStrokeTextPaint);

                // Progress to next item angle
                rotationAngleOffset -= rotationAngleOffsetIncrement;
                
                // Restore canvas transformations
                canvas.Restore();
            }
        }
    }
}