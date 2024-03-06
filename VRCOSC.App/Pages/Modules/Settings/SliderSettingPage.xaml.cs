// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.App.Modules.Attributes.Settings;

namespace VRCOSC.App.Pages.Modules.Settings;

public partial class SliderSettingPage
{
    public SliderSettingPage(SliderModuleSetting sliderModuleSetting)
    {
        InitializeComponent();

        DataContext = sliderModuleSetting;
    }

    private static int getDecimalPlaces(float value) => value.ToString(CultureInfo.InvariantCulture).Split('.')[1].Length;
}
