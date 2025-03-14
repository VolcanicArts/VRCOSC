// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;

namespace VRCOSC.App.Router.Serialisation.V1;

internal class SerialisableRouterManagerV1 : SerialisableVersion
{
    [JsonProperty("routes")]
    public List<SerialisableRouterInstanceV1> Routes = [];

    [JsonConstructor]
    public SerialisableRouterManagerV1()
    {
    }

    public SerialisableRouterManagerV1(RouterManager routerManager)
    {
        Version = 1;

        Routes = routerManager.Routes.Select(instance => new SerialisableRouterInstanceV1(instance)).ToList();
    }
}

internal class SerialisableRouterInstanceV1
{
    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("mode")]
    public RouterMode Mode;

    [JsonProperty("endpoint")]
    public string Endpoint = null!;

    [JsonConstructor]
    public SerialisableRouterInstanceV1()
    {
    }

    public SerialisableRouterInstanceV1(RouterInstance instance)
    {
        Name = instance.Name.Value;
        Mode = instance.Mode.Value;
        Endpoint = instance.Endpoint.Value;
    }
}