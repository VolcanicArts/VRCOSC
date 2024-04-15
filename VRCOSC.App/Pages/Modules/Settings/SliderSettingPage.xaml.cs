// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows.Input;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Modules.Settings;

public partial class SliderSettingPage
{
    public SliderModuleSetting SliderModuleSetting { get; }
    public Observable<float> SliderValue { get; } = new();

    public SliderSettingPage(SliderModuleSetting sliderModuleSetting)
    {
        InitializeComponent();

        SliderValue.Value = sliderModuleSetting.Attribute.Value;
        SliderModuleSetting = sliderModuleSetting;
        DataContext = this;
    }

    private void Slider_LostMouseCapture(object sender, MouseEventArgs e)
    {
        SliderModuleSetting.Attribute.Value = SliderValue.Value;
    }
}
