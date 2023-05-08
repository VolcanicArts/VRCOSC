﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.OSC;
using VRCOSC.Game.Router.Serialisation.Legacy;
using VRCOSC.Game.Router.Serialisation.V1;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Managers;

public class RouterManager : ICanSerialise
{
    public List<RouterData> Store = new();

    private readonly SerialisationManager serialisationManager;

    public RouterManager(Storage storage, NotificationContainer notification)
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(0, new LegacyRouterSerialiser(storage, notification, this));
        serialisationManager.RegisterSerialiser(1, new RouterSerialiser(storage, notification, this));
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

    public void Load()
    {
        Deserialise();
    }

    public void Deserialise()
    {
        if (!serialisationManager.Deserialise()) return;

        Serialise();
    }

    public void Serialise()
    {
        serialisationManager.Serialise();
    }
}

public class RouterData
{
    public string Label = null!;
    public OSCRouterEndpoints Endpoints = null!;
}
