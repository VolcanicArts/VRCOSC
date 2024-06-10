// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using Newtonsoft.Json;

namespace VRCOSC.App.Router;

public class RouterInstance
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("address")]
    public IPAddress Address { get; set; } = IPAddress.Loopback;

    [JsonProperty("port")]
    public int Port { get; set; }
}
