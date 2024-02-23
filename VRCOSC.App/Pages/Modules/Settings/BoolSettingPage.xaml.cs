// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Modules.Attributes.Settings;

namespace VRCOSC.App.Pages.Modules.Settings;

public partial class BoolSettingPage
{
    private BoolModuleSetting boolModuleSetting;

    public BoolSettingPage(BoolModuleSetting boolModuleSetting)
    {
        InitializeComponent();

        this.boolModuleSetting = boolModuleSetting;
        Title.DataContext = boolModuleSetting;
    }
}
