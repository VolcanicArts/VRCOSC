// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Modules.Attributes.Types;

namespace VRCOSC.App.Pages.Modules.Settings;

public partial class MutableKeyValuePairSettingPage
{
    public MutableKeyValuePairSettingPage(MutableKeyValuePairListModuleSetting moduleSetting)
    {
        InitializeComponent();

        DataContext = moduleSetting;
    }
}
