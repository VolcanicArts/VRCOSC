// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Logging;
using VRC.OSCQuery;
using VRCOSC.Game.OSC.Client;
using Zeroconf;

namespace VRCOSC.Game.OSC.VRChat;

public class VRChatOscClient : OscClient
{
    public Action<VRChatOscMessage>? OnParameterSent;
    public Action<VRChatOscMessage>? OnParameterReceived;

    private readonly HttpClient client = new();
    private int? port;

    public VRChatOscClient()
    {
        OnMessageSent += message => { OnParameterSent?.Invoke(new VRChatOscMessage(message)); };

        OnMessageReceived += message =>
        {
            var data = new VRChatOscMessage(message);
            if (!data.Values.Any()) return;

            OnParameterReceived?.Invoke(data);
        };
    }

    public async Task CheckForVRChatOSCQuery()
    {
        var hosts = await ZeroconfResolver.ResolveAsync("_oscjson._tcp.local.");
        var host = hosts.FirstOrDefault();

        if (host is null)
        {
            Logger.Log("No OscJson host found");
            port = null;
            return;
        }

        if (!host.Services.Any(s => s.Value.ServiceName.Contains("VRChat-Client")))
        {
            Logger.Log("No VRChat-Client found");
            port = null;
            return;
        }

        var service = host.Services.Single(s => s.Value.ServiceName.Contains("VRChat-Client"));

        port = service.Value.Port;
    }

    public async Task<object?> FindParameterValue(string parameterName)
    {
        if (port is null) return null;

        var url = $"http://127.0.0.1:{port}/avatar/parameters/{parameterName}";

        var response = await client.GetAsync(new Uri(url));
        var content = await response.Content.ReadAsStringAsync();
        var node = JsonConvert.DeserializeObject<OSCQueryNode>(content);

        if (node is null) return null;

        return node.OscType switch
        {
            "f" => Convert.ToSingle(node.Value[0]),
            "i" => Convert.ToInt32(node.Value[0]),
            "b" => Convert.ToBoolean(node.Value[0]),
            _ => null
        };
    }

    public async Task<TypeCode?> FindParameterType(string parameterName)
    {
        if (port is null) return null;

        var url = $"http://127.0.0.1:{port}/avatar/parameters/{parameterName}";

        var response = await client.GetAsync(new Uri(url));
        var content = await response.Content.ReadAsStringAsync();
        var node = JsonConvert.DeserializeObject<OSCQueryNode>(content);

        if (node is null) return null;

        return node.OscType switch
        {
            "f" => TypeCode.Single,
            "i" => TypeCode.Int32,
            "b" => TypeCode.Boolean,
            _ => null
        };
    }
}
