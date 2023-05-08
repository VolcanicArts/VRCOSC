// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.OSC;

namespace VRCOSC.Game.Router;

public class RouterManager
{
    public List<RouterData> Store = new();

    private readonly RouterSerialiser serialiser;

    public RouterManager(Storage storage, NotificationContainer notification)
    {
        serialiser = new RouterSerialiser(storage, notification, this);
    }

    public RouterData Create()
    {
        var routerData = new RouterData
        {
            Label = string.Empty,
            Endpoints = new OSCRouterEndpoints()
        };

        Store.Add(routerData);

        return routerData;
    }

    public void LoadData()
    {
        var data = serialiser.Load();

        if (data is null) return;

        Store = data;
        SaveData();
    }

    public void SaveData()
    {
        serialiser.Save();
    }
}

public class RouterData
{
    public string Label = null!;
    public OSCRouterEndpoints Endpoints = null!;
}
