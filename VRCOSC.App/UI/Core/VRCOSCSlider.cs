// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

public class VRCOSCSlider : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(VRCOSCSlider), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(VRCOSCSlider), new PropertyMetadata(0d));

    public double MinValue
    {
        get => (double)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(VRCOSCSlider), new PropertyMetadata(1d));

    public double MaxValue
    {
        get => (double)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly DependencyProperty TickFrequencyProperty =
        DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(VRCOSCSlider), new PropertyMetadata(1d));

    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public static readonly DependencyProperty SliderTypeProperty =
        DependencyProperty.Register(nameof(SliderType), typeof(SliderType), typeof(VRCOSCSlider), new PropertyMetadata(SliderType.Float));

    public SliderType SliderType
    {
        get => (SliderType)GetValue(SliderTypeProperty);
        set => SetValue(SliderTypeProperty, value);
    }

    public static readonly DependencyProperty ToStringFormatProperty =
        DependencyProperty.Register(nameof(ToStringFormat), typeof(string), typeof(VRCOSCSlider), new PropertyMetadata("#"));

    public string ToStringFormat
    {
        get => (string)GetValue(ToStringFormatProperty);
        set => SetValue(ToStringFormatProperty, value);
    }

    public bool IsPercentage { get; set; }

    static VRCOSCSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(VRCOSCSlider), new FrameworkPropertyMetadata(typeof(VRCOSCSlider)));
    }
}

public enum SliderType
{
    Float,
    Int
}

public class VRCOSCSliderToTextConverter : IMultiValueConverter
{
    public SliderToTextMode Mode { get; set; }

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [VRCOSCSlider slider, double]) return string.Empty;

        if (Mode == SliderToTextMode.Value)
        {
            switch (slider.SliderType)
            {
                case SliderType.Float:
                    return slider.Value.ToString(slider.ToStringFormat);

                case SliderType.Int:
                    return $"{(int)Math.Round(slider.Value)}" + (slider.IsPercentage ? "%" : string.Empty);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (Mode == SliderToTextMode.Min)
        {
            switch (slider.SliderType)
            {
                case SliderType.Float:
                    return slider.MinValue.ToString(slider.ToStringFormat);

                case SliderType.Int:
                    return $"{(int)Math.Round(slider.MinValue)}" + (slider.IsPercentage ? "%" : string.Empty);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (Mode == SliderToTextMode.Max)
        {
            switch (slider.SliderType)
            {
                case SliderType.Float:
                    return slider.MaxValue.ToString(slider.ToStringFormat);

                case SliderType.Int:
                    return $"{(int)Math.Round(slider.MaxValue)}" + (slider.IsPercentage ? "%" : string.Empty);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return string.Empty;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;

    public enum SliderToTextMode
    {
        Value,
        Min,
        Max
    }
}