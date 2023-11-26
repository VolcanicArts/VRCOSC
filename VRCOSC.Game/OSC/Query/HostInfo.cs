// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace VRCOSC.Game.OSC.Query;

public class HostInfo
{
    [JsonProperty("NAME")]
    public string Name = "VRCOSC";

    [JsonProperty("OSC_IP")]
    public string OSCIP = IPAddress.Loopback.ToString();

    [JsonProperty("OSC_PORT")]
    public int OSCPort;

    [JsonProperty("OSC_TRANSPORT")]
    public string OSCTransport = "UDP";

    [JsonConstructor]
    public HostInfo()
    {
    }

    public HostInfo(int oscPort)
    {
        OSCPort = oscPort;
    }
}
