// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using VRCOSC.App.Router;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class RouterView
{
    private static readonly Uri router_docs_uri = new("https://vrcosc.com/docs/v2/router");

    public RouterManager RouterManager { get; }

    public RouterView()
    {
        InitializeComponent();
        RouterManager = RouterManager.GetInstance();
        DataContext = this;
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
        router_docs_uri.OpenExternally();
    }
}