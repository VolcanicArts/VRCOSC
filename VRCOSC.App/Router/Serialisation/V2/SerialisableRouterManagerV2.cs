// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;

namespace VRCOSC.App.Router.Serialisation.V2;

internal class SerialisableRouterManagerV2 : SerialisableVersion
{
    [JsonProperty("routes")]
    public List<SerialisableRouterInstanceV2> Routes = [];

    [JsonConstructor]
    public SerialisableRouterManagerV2()
    {
    }

    public SerialisableRouterManagerV2(RouterManager routerManager)
    {
        Version = 2;

        Routes = routerManager.Routes.Select(instance => new SerialisableRouterInstanceV2(instance)).ToList();
    }
}

internal class SerialisableRouterInstanceV2
{
    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("receive_enabled")]
    public bool ReceiveEnabled;

    [JsonProperty("receive_endpoint")]
    public string ReceiveEndpoint = null!;

    [JsonProperty("send_enabled")]
    public bool SendEnabled;

    [JsonProperty("send_endpoint")]
    public string SendEndpoint = null!;

    [JsonConstructor]
    public SerialisableRouterInstanceV2()
    {
    }

    public SerialisableRouterInstanceV2(RouterInstance instance)
    {
        Name = instance.Name.Value;
        ReceiveEnabled = instance.ReceiveEnabled.Value;
        ReceiveEndpoint = instance.ReceiveEndpoint.Value;
        SendEnabled = instance.SendEnabled.Value;
        SendEndpoint = instance.SendEndpoint.Value;
    }
}