// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;

namespace VRCOSC.App.UI.Views.Modules.Settings;

public partial class DropdownSettingView
{
    public DropdownSettingView(Module _, EnumModuleSetting enumModuleSetting)
    {
        InitializeComponent();

        DataContext = enumModuleSetting;
        AttributeComboBox.ItemsSource = enumModuleSetting.EnumType.GetEnumValues();
    }
}
