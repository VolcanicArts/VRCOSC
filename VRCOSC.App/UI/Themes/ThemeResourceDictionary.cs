// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;

namespace VRCOSC.App.UI.Themes;

public class ThemeResourceDictionary : ResourceDictionary
{
    private readonly Uri darkSource = new("pack://application:,,,/VRCOSC.App;component/UI/Themes/Dark.xaml");
    private readonly Uri lightSource = new("pack://application:,,,/VRCOSC.App;component/UI/Themes/Light.xaml");

    public ThemeResourceDictionary()
    {
        AppManager.GetInstance().ProxyTheme.Subscribe(theme => Source = theme == Theme.Dark ? darkSource : lightSource, true);
    }
}