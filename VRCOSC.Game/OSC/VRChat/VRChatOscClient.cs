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
    public int? QueryPort { get; private set; }

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
        if (QueryPort is not null) return;

        var hosts = await ZeroconfResolver.ResolveAsync("_oscjson._tcp.local.");
        var host = hosts.FirstOrDefault();

        if (host is null)
        {
            Logger.Log("No OscJson host found");
            QueryPort = null;
            return;
        }

        if (!host.Services.Any(s => s.Value.ServiceName.Contains("VRChat-Client")))
        {
            Logger.Log("No VRChat-Client found");
            QueryPort = null;
            return;
        }

        var service = host.Services.Single(s => s.Value.ServiceName.Contains("VRChat-Client"));

        QueryPort = service.Value.Port;
        Logger.Log($"Successfully found OscJson port: {QueryPort}");
    }

    public void Reset()
    {
        QueryPort = null;
    }

    private async Task<OSCQueryNode?> findParameter(string parameterName)
    {
        if (QueryPort is null) return null;

        var url = $"http://127.0.0.1:{QueryPort}/avatar/parameters/{parameterName}";

        var response = await client.GetAsync(new Uri(url));
        var content = await response.Content.ReadAsStringAsync();
        var node = JsonConvert.DeserializeObject<OSCQueryNode>(content);

        return node is null || node.Access == Attributes.AccessValues.NoValue ? null : node;
    }

    public async Task<object?> FindParameterValue(string parameterName)
    {
        var node = await findParameter(parameterName);
        if (node is null) return null;

        return node.OscType switch
        {
            "f" => Convert.ToSingle(node.Value[0]),
            "i" => Convert.ToInt32(node.Value[0]),
            "T" => Convert.ToBoolean(node.Value[0]),
            "F" => Convert.ToBoolean(node.Value[0]),
            _ => throw new InvalidOperationException($"Unknown type {node.OscType}")
        };
    }

    public async Task<TypeCode?> FindParameterType(string parameterName)
    {
        var node = await findParameter(parameterName);
        if (node is null) return null;

        return node.OscType switch
        {
            "f" => TypeCode.Single,
            "i" => TypeCode.Int32,
            "T" => TypeCode.Boolean,
            "F" => TypeCode.Boolean,
            _ => throw new InvalidOperationException($"Unknown type {node.OscType}")
        };
    }
}
