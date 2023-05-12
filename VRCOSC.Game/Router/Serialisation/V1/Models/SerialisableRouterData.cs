// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Router.Serialisation.V1.Models;

public class SerialisableRouterData
{
    [JsonProperty("label")]
    public string Label = string.Empty;

    [JsonProperty("receiveaddress")]
    public string ReceiveAddress = string.Empty;

    [JsonProperty("receiveport")]
    public int ReceivePort;

    [JsonProperty("sendaddress")]
    public string SendAddress = string.Empty;

    [JsonProperty("sendport")]
    public int SendPort;

    [JsonConstructor]
    public SerialisableRouterData()
    {
    }

    public SerialisableRouterData(RouterData data)
    {
        Label = data.Label.Value;
        ReceiveAddress = data.Endpoints.ReceiveAddress.Value;
        ReceivePort = data.Endpoints.ReceivePort.Value;
        SendAddress = data.Endpoints.SendAddress.Value;
        SendPort = data.Endpoints.SendPort.Value;
    }
}
