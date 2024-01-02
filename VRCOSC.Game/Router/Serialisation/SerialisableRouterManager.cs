// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.Serialisation;

namespace VRCOSC.Router.Serialisation;

public class SerialisableRouterManager : SerialisableVersion
{
    [JsonProperty("data")]
    public List<SerialisableRouterData> Data = new();

    [JsonConstructor]
    public SerialisableRouterManager()
    {
    }

    public SerialisableRouterManager(RouterManager routerManager)
    {
        Version = 1;
        Data.AddRange(routerManager.Routes.Select(route => new SerialisableRouterData(route)));
    }
}

public class SerialisableRouterData
{
    [JsonProperty("name")]
    public string Name = string.Empty;

    [JsonProperty("address")]
    public string Address = string.Empty;

    [JsonProperty("port")]
    public int Port;

    [JsonConstructor]
    public SerialisableRouterData()
    {
    }

    public SerialisableRouterData(Route route)
    {
        Name = route.Name.Value;
        Address = route.Endpoint.Value.Address.ToString();
        Port = route.Endpoint.Value.Port;
    }
}
