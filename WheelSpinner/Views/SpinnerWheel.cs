using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using WheelSpinner.Extensions;
using WheelSpinner.Utils;
using Xamarin.Forms;

namespace WheelSpinner.Views
{
    public partial class SpinnerWheel : ContentView
    {
        private const double DegreesToRadianFactor = Math.PI / 180;
        private const double RadianToDegreesFactor = 180 / Math.PI;

        /// <summary>
        /// The maximum value for item image alpha channel
        /// </summary>
        private const byte MaxAlpha = 255;

        /// <summary>
        /// The minimum value for item image alpha channel
        /// </summary>
        private const byte MinAlpha = 50;

        internal readonly SKCanvasView _canvasView;

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

            // commands
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

            // Default selected index
            _selectedItemIndex = 1;

            // Load mandatory cat pictures
            _bitmaps = new List<SKBitmap>();
            foreach (var resourceName in new List<string> {"img_a.png", "img_b.png", "img_d.png"})
            {
                var relativePath = $"Assets.{resourceName}";
                _bitmaps.Add(BitmapUtil.LoadBitmapFromResource(relativePath));
            }
        }

        private long _touchId = -1;
        private int _pressedItemIdx = -1;
        private int _selectedItemIndex = -1;

        private void CanvasViewOnTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:

                    var pressedContentBoundingRect =
                        _hitAreaToIdxItemTuple.FirstOrDefault(pair => pair.Key.Contains(e.Location - _centerPoint));
                    if (pressedContentBoundingRect.Key != SKRect.Empty)
                    {
                        _touchId = e.Id;
                        _pressedItemIdx = pressedContentBoundingRect.Value.itemIndex;

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
                    _pressedItemIdx = -1;

                    SnapToClosestSlot();

                    break;

                case SKTouchAction.Cancelled:
                    _touchId = -1;
                    _pressedItemIdx = -1;
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
                var closestPairTopDelta = _hitAreaToIdxItemTuple
                    .Where(pair => pair.Key.Left < 0.0f)
                    .Min(pair => Math.Abs(pair.Key.Top));
                var closestPair = _hitAreaToIdxItemTuple
                    .FirstOrDefault(pair => pair.Key.Left < 0.0f
                                            && Math.Abs(Math.Abs(pair.Key.Top) - closestPairTopDelta) < 0.1f);

                // Update selected index
                _selectedItemIndex = closestPair.Value.itemIndex;
                _canvasView.InvalidateSurface();

                // Update selected item property
                SelectedItem = closestPair.Value.item;
            });
        }

        private readonly Dictionary<SKRect, (int itemIndex, object item)> _hitAreaToIdxItemTuple =
            new Dictionary<SKRect, (int, object)>();

        private List<SKBitmap> _bitmaps;

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;

            // Start fresh - isn't that nice?
            canvas.Clear();

            _centerPoint = new SKPoint(info.Width, info.Height / 2f);

            // Move canvas origin to middle of the screen's right edge
            canvas.Translate(info.Width, info.Height / 2f);

            // Draw large circle
            var radius = Math.Min(info.Width, info.Height / 2f) * 0.8f;
            if (ShowWheel)
            {
                canvas.DrawCircle(new SKPoint(), radius, _mainCirclePaint);
            }

            // Draw little circles
            var rotationAngleOffset = 240.0f;
            var rotationAngleOffsetIncrement = 60;
            var littleCircleRadius = radius * 0.2f;

            _hitAreaToIdxItemTuple.Clear();

            for (var i = 0; i < 6; i++)
            {
                var currentItemIndex = i % 3;
                canvas.Save();

                // Compute the current item center
                var currentItemAngle = RotationAngle + rotationAngleOffset;
                var currentItemCenter = new SKPoint(
                    (float) (radius * Math.Cos(currentItemAngle * DegreesToRadianFactor)),
                    (float) (radius * Math.Sin(currentItemAngle * DegreesToRadianFactor))
                );
                canvas.Translate(currentItemCenter);

                // Draw the current item
                var newAlpha = ShouldFadeToEdge ? ComputeItemAlpha(currentItemCenter, radius) : MaxAlpha;
                DrawCurrentItem(currentItemIndex, canvas, littleCircleRadius, newAlpha);

                // Compute hit area of current item
                // ToDo: account fo scaling - add an extension method to simplify the code
                var hitAreaRect = new SKRect(currentItemCenter.X - (littleCircleRadius),
                    currentItemCenter.Y - (littleCircleRadius),
                    currentItemCenter.X + (littleCircleRadius),
                    currentItemCenter.Y + (littleCircleRadius)
                );
                _hitAreaToIdxItemTuple.Add(hitAreaRect,
                    (itemIndex: currentItemIndex, item: ItemSource[currentItemIndex]));

                // Display the hit area if needed
                if (ShouldHighlightHitArea)
                {
                    canvas.DrawRect(hitAreaRect.OffsetClone(currentItemCenter, OffsetMode.Subtract), _hitAreaPaint);
                }

                // Progress to next item angle
                rotationAngleOffset -= rotationAngleOffsetIncrement;

                // Restore canvas transformations
                canvas.Restore();
            }
        }

        private static byte ComputeItemAlpha(SKPoint currentItemCenter, float radius)
        {
            var distanceToOy = Math.Min(Math.Abs(currentItemCenter.X), radius);
            var alphaFactor = distanceToOy / radius;
            var newAlpha = MinAlpha + (MaxAlpha - MinAlpha) * alphaFactor;
            return (byte) newAlpha;
        }

        private void DrawCurrentItem(int currentItemIndex, SKCanvas canvas, float littleCircleRadius, byte itemAlpha)
        {
            if (currentItemIndex == _pressedItemIdx) // Highlight if pressed
            {
                canvas.Scale(1.1f);
                _thickStrokePaint.Color = _thickStrokePaint.Color.WithAlpha(MaxAlpha);
                _thinStrokeTextPaint.Color = _thinStrokeTextPaint.Color.WithAlpha(MaxAlpha);
            }
            else if (currentItemIndex == _selectedItemIndex) // Highlight if item is currently selected
            {
                canvas.Scale(1.2f);
                _thickStrokePaint.Color = _thickStrokePaint.Color.WithAlpha(MaxAlpha);
                _thinStrokeTextPaint.Color = _thinStrokeTextPaint.Color.WithAlpha(MaxAlpha);
            }
            else // draw normally
            {
                _thickStrokePaint.Color = _thickStrokePaint.Color.WithAlpha(128);
                _thinStrokeTextPaint.Color = _thinStrokeTextPaint.Color.WithAlpha(128);
            }

            var currentItemBitmap = _bitmaps[currentItemIndex];
            var scale = Math.Min(
                (2 * littleCircleRadius) / currentItemBitmap.Width,
                (2 * littleCircleRadius) / currentItemBitmap.Height
            );
            var destinationRect = new SKRect(
                -(currentItemBitmap.Width * scale) / 2,
                -(currentItemBitmap.Height * scale) / 2,
                (currentItemBitmap.Width * scale) / 2,
                (currentItemBitmap.Height * scale) / 2
            );

            // Finally, draw the item
            _itemPaint.Color = SKColor.Empty.WithAlpha(itemAlpha);
            canvas.DrawBitmap(
                currentItemBitmap,
                //new SKRect(-littleCircleRadius, -littleCircleRadius, littleCircleRadius, littleCircleRadius),
                destinationRect,
                _itemPaint);
        }
    }
}