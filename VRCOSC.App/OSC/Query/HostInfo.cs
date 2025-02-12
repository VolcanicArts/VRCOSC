// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace VRCOSC.App.OSC.Query;

public class HostInfo
{
    [JsonProperty("NAME")]
    public string? Name;

    [JsonProperty("OSC_IP")]
    public string? OSCIP;

    [JsonProperty("OSC_PORT")]
    public int? OSCPort;

    [JsonProperty("OSC_TRANSPORT")]
    public string? OSCTransport;

    [JsonProperty("EXTENSIONS")]
    public HostInfoExtensions? Extensions;

    [JsonConstructor]
    public HostInfo()
    {
    }

    public HostInfo(string oscIp, int oscPort)
    {
        Name = AppManager.APP_NAME;
        OSCIP = oscIp;
        OSCPort = oscPort;
        OSCTransport = "UDP";
        Extensions = new HostInfoExtensions();
    }
}

public class HostInfoExtensions
{
    [JsonProperty("ACCESS")]
    public bool Access;

    [JsonProperty("VALUE")]
    public bool Value;

    [JsonProperty("RANGE")]
    public bool Range;

    [JsonProperty("DESCRIPTION")]
    public bool Description;

    [JsonProperty("TAGS")]
    public bool Tags;

    [JsonProperty("EXTENDED_TYPE")]
    public bool ExtendedType;

    [JsonProperty("UNIT")]
    public bool Unit;

    [JsonProperty("CRITICAL")]
    public bool Critical;

    [JsonProperty("CLIPMODE")]
    public bool ClipMode;

    [JsonProperty("OVERLOADS")]
    public bool Overloads;

    [JsonProperty("LISTEN")]
    public bool Listen;

    [JsonProperty("PATH_CHANGED")]
    public bool PathChanged;
}