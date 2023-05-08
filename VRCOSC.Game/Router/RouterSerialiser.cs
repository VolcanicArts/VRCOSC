// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Router;

public class RouterSerialiser : Serialiser<RouterManager, List<RouterData>>
{
    protected override string FileName => "router.json";

    public RouterSerialiser(Storage storage, NotificationContainer notification, RouterManager routerManager)
        : base(storage, notification, routerManager)
    {
    }

    protected override object GetSerialisableData(RouterManager routerManager) => routerManager.Store;

    protected override void ExecuteAfterDeserialisation(RouterManager routerManager, List<RouterData> data)
    {
        routerManager.Store = data;
    }
}
