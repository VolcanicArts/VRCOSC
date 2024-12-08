// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;

namespace VRCOSC.App.Router.Serialisation;

public class SerialisableRouterManager : SerialisableVersion
{
    [JsonProperty("routes")]
    public List<RouterInstance> Routes = new();

    [JsonConstructor]
    public SerialisableRouterManager()
    {
    }

    public SerialisableRouterManager(RouterManager routerManager)
    {
        Version = 1;

        Routes = routerManager.Routes.ToList();
    }
}
