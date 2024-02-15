// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.App;

public class AppManager
{
    private static AppManager? instance;
    public static AppManager GetInstance() => instance ??= new AppManager();

    private readonly Dictionary<PageLookup, IVRCOSCPage> pageInstances = new();

    public AppManager()
    {
    }

    public void RegisterPage(PageLookup pageLookup, IVRCOSCPage instance)
    {
        pageInstances[pageLookup] = instance;
    }

    public void Refresh(PageLookup flags)
    {
        if ((flags & PageLookup.Home) == PageLookup.Home && pageInstances.TryGetValue(PageLookup.Home, out var homePage))
            homePage.Refresh();

        if ((flags & PageLookup.Packages) == PageLookup.Packages && pageInstances.TryGetValue(PageLookup.Packages, out var packagePage))
            packagePage.Refresh();

        if ((flags & PageLookup.Modules) == PageLookup.Modules && pageInstances.TryGetValue(PageLookup.Modules, out var modulesPage))
            modulesPage.Refresh();
    }
}

[Flags]
public enum PageLookup
{
    Home = 1 << 0,
    Packages = 1 << 1,
    Modules = 1 << 2
}
