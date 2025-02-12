// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.Startup;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class StartupView
{
    public StartupView()
    {
        InitializeComponent();

        DataContext = StartupManager.GetInstance();
    }

    private void AddInstance_OnClick(object sender, RoutedEventArgs e)
    {
        StartupManager.GetInstance().Instances.Add(new StartupInstance());
    }

    private void RemoveInstance_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (IconButton)sender;
        var instance = (StartupInstance)element.Tag;

        StartupManager.GetInstance().Instances.Remove(instance);
    }
}