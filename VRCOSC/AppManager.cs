// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace VRCOSC;

public class AppManager
{
    private static AppManager? instance;
    public static AppManager GetInstance() => instance ??= new AppManager();

    private readonly Dictionary<PageLookup, Page> pageInstances = new();

    public AppManager()
    {
    }

    public void RegisterPage(PageLookup pageLookup, Page instance)
    {
        pageInstances[pageLookup] = instance;
    }

    public void Refresh(PageLookup flags)
    {
        if ((flags & PageLookup.Home) == PageLookup.Home && pageInstances.TryGetValue(PageLookup.Home, out var homePage))
            homePage.NavigationService?.Refresh();

        if ((flags & PageLookup.Packages) == PageLookup.Packages && pageInstances.TryGetValue(PageLookup.Packages, out var packagePage))
            packagePage.NavigationService?.Refresh();

        if ((flags & PageLookup.Modules) == PageLookup.Modules && pageInstances.TryGetValue(PageLookup.Modules, out var modulesPage))
            modulesPage.NavigationService?.Refresh();
    }
}

[Flags]
public enum PageLookup
{
    Home = 1 << 0,
    Packages = 1 << 1,
    Modules = 1 << 2
}
