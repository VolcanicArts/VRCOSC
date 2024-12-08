// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using Newtonsoft.Json;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router;

public class RouterInstance
{
    [JsonProperty("name")]
    public Observable<string> Name { get; } = new("My Label");

    [JsonProperty("endpoint")]
    public Observable<string> Endpoint { get; } = new($"{IPAddress.Loopback}:9000");
}
