using System.Collections.Generic;
using System.Windows.Input;
using SkiaSharp;
using Xamarin.Forms;

namespace WheelSpinner.Views
{
    public partial class SpinnerWheel : ContentView
    {
        #region Fields

        private readonly SKPaint _hitAreaPaint = new SKPaint()
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
            get => (ICommand) GetValue(GoLeftCommandProperty);
            set => SetValue(GoLeftCommandProperty, value);
        }

        public static readonly BindableProperty GoRightCommandProperty = BindableProperty.Create("GoRightCommand",
            typeof(ICommand),
            typeof(SpinnerWheel));

        public ICommand GoRightCommand
        {
            get => (ICommand) GetValue(GoRightCommandProperty);
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
            get => (double) GetValue(RotationAngleProperty);
            set => SetValue(RotationAngleProperty, value);
        }

        private static void RotationAngleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as Views.SpinnerWheel)?._canvasView.InvalidateSurface();
        }

        public static readonly BindableProperty ItemsProperty = BindableProperty.Create("Items",
            typeof(List<object>),
            typeof(SpinnerWheel),
            new List<object>() {"a", "b", "c"}, propertyChanged: ItemsChanged);

        public List<object> Items
        {
            get => (List<object>) GetValue(ItemsProperty);
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

        public static readonly BindableProperty ShouldHighlightHitAreaProperty = BindableProperty.Create(
            "ShouldHighlightHitArea",
            typeof(bool),
            typeof(SpinnerWheel),
            false,
            BindingMode.TwoWay,
            propertyChanged: (bindable, value, newValue) => { ((SpinnerWheel) bindable).ShouldHighlightHitArea = (bool) newValue; }
        );

        public bool ShouldHighlightHitArea
        {
            get => (bool) GetValue(ShouldHighlightHitAreaProperty);
            set => SetValue(ShouldHighlightHitAreaProperty, value);
        }

        #endregion BindableProperties
    }
}