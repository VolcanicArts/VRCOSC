// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public partial class QueryableParameterSettingView
{
    private WindowManager windowManager = null!;

    public object ModuleSetting { get; }

    public QueryableParameterSettingView(Module _, QueryableParameterListModuleSetting moduleSetting)
    {
        InitializeComponent();
        ModuleSetting = moduleSetting;
    }

    public QueryableParameterSettingView(Module _, ActionableQueryableParameterListModuleSetting moduleSetting)
    {
        InitializeComponent();
        ModuleSetting = moduleSetting;
    }

    private void EditButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ModuleSetting is QueryableParameterListModuleSetting queryableParameterListModuleSetting)
        {
            windowManager.TrySpawnChild(new QueryableParameterEditWindow(queryableParameterListModuleSetting));
        }

        if (ModuleSetting is ActionableQueryableParameterListModuleSetting actionableQueryableParameterListModuleSetting)
        {
            windowManager.TrySpawnChild(new QueryableParameterEditWindow(actionableQueryableParameterListModuleSetting));
        }
    }

    private void QueryableParameterSettingView_OnLoaded(object sender, RoutedEventArgs e)
    {
        windowManager = new WindowManager(this);
    }
}