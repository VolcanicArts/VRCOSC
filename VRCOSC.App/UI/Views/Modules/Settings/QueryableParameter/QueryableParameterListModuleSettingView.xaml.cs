// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public partial class QueryableParameterListModuleSettingView
{
    private WindowManager windowManager = null!;
    private readonly ListModuleSetting moduleSetting;

    public QueryableParameterListModuleSettingView(Module _, ListModuleSetting moduleSetting)
    {
        if (moduleSetting.GetType() != typeof(QueryableParameterListModuleSetting<>)) throw new ArgumentException($"{nameof(QueryableParameterListModuleSettingView)} cannot take in a module setting not of type QueryableParameterListModuleSetting");

        InitializeComponent();
        this.moduleSetting = moduleSetting;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        windowManager = new WindowManager(this);
    }

    private void EditButton_OnClick(object sender, RoutedEventArgs e)
    {
        windowManager.TrySpawnChild(new QueryableParameterListEditWindow(moduleSetting));
    }
}