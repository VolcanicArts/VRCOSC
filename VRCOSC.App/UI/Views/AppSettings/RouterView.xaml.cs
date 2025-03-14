// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Windows;
using VRCOSC.App.Router;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class RouterView
{
    public RouterManager RouterManager { get; }

    public RouterView()
    {
        InitializeComponent();
        DataContext = this;
        RouterManager = RouterManager.GetInstance();
    }

    private static readonly Uri router_docs_uri = new("https://vrcosc.com/docs/v2/router");

    public IEnumerable<RouterMode> RouterModeSource => Enum.GetValues<RouterMode>();

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

    private void RouterHelpButton_OnClick(object sender, RoutedEventArgs e)
    {
        router_docs_uri.OpenExternally();
    }
}