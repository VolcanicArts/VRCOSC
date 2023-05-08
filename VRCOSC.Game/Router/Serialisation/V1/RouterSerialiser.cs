// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.OSC;
using VRCOSC.Game.Router.Serialisation.V1.Models;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Router.Serialisation.V1;

public class RouterSerialiser : Serialiser<RouterManager, SerialisableRouterManager>
{
    protected override string FileName => "router.json";

    public RouterSerialiser(Storage storage, NotificationContainer notification, RouterManager routerManager)
        : base(storage, notification, routerManager)
    {
    }

    protected override SerialisableRouterManager GetSerialisableData(RouterManager routerManager) => new(routerManager);

    protected override void ExecuteAfterDeserialisation(RouterManager routerManager, SerialisableRouterManager data)
    {
        routerManager.Store.Clear();

        data.Data.ForEach(routerData =>
        {
            routerManager.Store.Add(new RouterData
            {
                Label = routerData.Label,
                Endpoints = new OSCRouterEndpoints
                {
                    ReceiveAddress = routerData.ReceiveAddress,
                    ReceivePort = routerData.ReceivePort,
                    SendAddress = routerData.SendAddress,
                    SendPort = routerData.SendPort
                }
            });
        });
    }
}
