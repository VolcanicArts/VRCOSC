// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public partial class QueryableParameterListModuleSettingView
{
    public QueryableParameterListModuleSettingView(Module _, QueryableParameterListModuleSetting moduleSetting)
    {
        InitializeComponent();
        DataContext = moduleSetting;
    }
}