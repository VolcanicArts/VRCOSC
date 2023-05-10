// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Router.Serialisation.V1.Models;

public class SerialisableRouterManager
{
    [JsonProperty("version")]
    public int Version = 1;

    [JsonProperty("data")]
    public List<SerialisableRouterData> Data = new();

    [JsonConstructor]
    public SerialisableRouterManager()
    {
    }

    public SerialisableRouterManager(RouterManager routerManager)
    {
        routerManager.Store.ForEach(routerData => Data.Add(new SerialisableRouterData(routerData)));
    }
}
