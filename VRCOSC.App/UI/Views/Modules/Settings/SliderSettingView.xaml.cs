// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Modules.Settings;

public partial class SliderSettingView
{
    public SliderModuleSetting SliderModuleSetting { get; }
    public Observable<float> SliderValue { get; } = new();

    public SliderSettingView(Module _, SliderModuleSetting sliderModuleSetting)
    {
        InitializeComponent();

        SliderValue.Value = sliderModuleSetting.Attribute.Value;
        SliderModuleSetting = sliderModuleSetting;
        DataContext = this;

        var binding = new Binding("SliderValue.Value")
        {
            Mode = BindingMode.OneWay,
            StringFormat = sliderModuleSetting.ValueType == typeof(int) ? "F0" : $"F{countDecimalPlaces(sliderModuleSetting.TickFrequency)}"
        };

        SliderValueText.SetBinding(TextBlock.TextProperty, binding);
    }

    private void Slider_LostMouseCapture(object sender, MouseEventArgs e)
    {
        SliderModuleSetting.Attribute.Value = SliderValue.Value;
    }

    private static int countDecimalPlaces(float number)
    {
        var numberString = number.ToString(CultureInfo.InvariantCulture);
        var decimalPointIndex = numberString.IndexOf('.');

        if (decimalPointIndex == -1)
        {
            return 0;
        }

        return numberString.Length - decimalPointIndex - 1;
    }
}
