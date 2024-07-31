// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.Router;

namespace VRCOSC.App.UI.Views.Router;

public partial class RouterView
{
    public RouterManager RouterManager { get; }

    public RouterView()
    {
        RouterManager = RouterManager.GetInstance();
        DataContext = this;

        InitializeComponent();
    }

    private void AddInstance_OnClick(object sender, RoutedEventArgs e)
    {
        RouterManager.Routes.Add(new RouterInstance());
    }

    private void RemoveInstance_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var route = (RouterInstance)element.Tag;

        RouterManager.Routes.Remove(route);
    }

    private void InfoButton_OnClick(object sender, RoutedEventArgs e)
    {
        // open router docs
    }
}
