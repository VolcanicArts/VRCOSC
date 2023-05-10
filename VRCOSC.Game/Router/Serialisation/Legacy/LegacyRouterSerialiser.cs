// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Router.Serialisation.Legacy;

public class LegacyRouterSerialiser : Serialiser<RouterManager, List<RouterData>>
{
    protected override string FileName => "router.json";

    public LegacyRouterSerialiser(Storage storage, NotificationContainer notification, RouterManager routerManager)
        : base(storage, notification, routerManager)
    {
    }

    protected override List<RouterData> GetSerialisableData(RouterManager routerManager) => routerManager.Store.ToList();

    protected override void ExecuteAfterDeserialisation(RouterManager routerManager, List<RouterData> data)
    {
        routerManager.Store.Clear();
        routerManager.Store.AddRange(data);
    }
}
