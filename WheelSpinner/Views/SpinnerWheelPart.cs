using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace WheelSpinner.Views
{
    public partial class SpinnerWheel : ContentView
    {
        #region Fields

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

        private readonly SKPaint _itemPaint = new SKPaint
        {
            IsAntialias = true,
            //Color = SKColors.White.WithAlpha(128)
        };

        private readonly SKPaint _hitAreaPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = SKColors.GreenYellow.WithAlpha(189)
        };

        #endregion Fields

        #region BindableProperties

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

        /// <summary>
        /// The current rotation angle of the spinnerWheel
        /// </summary>
        public double RotationAngle
        {
            get => (double)GetValue(RotationAngleProperty);
            set => SetValue(RotationAngleProperty, value);
        }

        private static void RotationAngleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as Views.SpinnerWheel)?._canvasView.InvalidateSurface();
        }

        public static readonly BindableProperty ItemSourceProperty = BindableProperty.Create(
            "ItemSource",
            typeof(List<object>),
            typeof(SpinnerWheel),
            new List<object> { "a", "b", "c" }, propertyChanged: ItemSourceChanged);

        /// <summary>
        /// The items to be displayed
        /// </summary>
        public List<object> ItemSource
        {
            get => (List<object>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        private static void ItemSourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as SpinnerWheel)?._canvasView.InvalidateSurface();
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create("SelectedItem",
            typeof(object),
            typeof(SpinnerWheel),
            null,
            BindingMode.TwoWay);

        /// <summary>
        /// The item from the ItemSource, that is currently selected.
        /// </summary>
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly BindableProperty ShouldHighlightHitAreaProperty = BindableProperty.Create(
            "ShouldHighlightHitArea",
            typeof(bool),
            typeof(SpinnerWheel),
            false,
            BindingMode.TwoWay,
            propertyChanged: (bindable, value, newValue) => { ((SpinnerWheel)bindable).ShouldHighlightHitArea = (bool)newValue; }
        );

        /// <summary>
        /// Decides whether or not the hit area for touch input should be highlighted.
        /// This is meant as development support and should be set to false for production builds
        /// </summary>
        public bool ShouldHighlightHitArea
        {
            get => (bool)GetValue(ShouldHighlightHitAreaProperty);
            set => SetValue(ShouldHighlightHitAreaProperty, value);
        }

        public static readonly BindableProperty ShouldFadeToEdgeProperty = BindableProperty.Create(
            "ShouldFadeToEdge",
            typeof(bool),
            typeof(SpinnerWheel),
            false,
            BindingMode.TwoWay,
            propertyChanged: (bindable, value, newValue) => { ((SpinnerWheel)bindable).ShouldFadeToEdge = (bool)newValue; }
        );

        /// <summary>
        /// Decides whether or not items get progressively more transparent as they approach the right edge
        /// </summary>
        public bool ShouldFadeToEdge
        {
            get => (bool)GetValue(ShouldFadeToEdgeProperty);
            set => SetValue(ShouldFadeToEdgeProperty, value);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            _canvasView?.InvalidateSurface();
        }

        #endregion BindableProperties
    }
}