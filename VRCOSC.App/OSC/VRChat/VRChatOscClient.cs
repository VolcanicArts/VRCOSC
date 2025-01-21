// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FastOSC;
using Newtonsoft.Json;
using VRCOSC.App.OSC.Query;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OSC.VRChat;

public class VRChatOscClient
{
    public Action<VRChatOscMessage>? OnParameterSent;
    public Action<VRChatOscMessage>? OnParameterReceived;

    private readonly HttpClient client = new();

    private ConnectionManager connectionManager = null!;

    private readonly OSCSender sender = new();
    private readonly OSCReceiver receiver = new();

    private IPEndPoint? sendEndpoint;
    private IPEndPoint? receiveEndpoint;

    public VRChatOscClient()
    {
        client.Timeout = TimeSpan.FromSeconds(0.1f);

        receiver.OnMessageReceived += message =>
        {
            var data = new VRChatOscMessage(message);
            if (data.Arguments.Length == 0) return;

            OnParameterReceived?.Invoke(data);
        };
    }

    public void Send(string address, params object?[] values)
    {
        var message = new OSCMessage(address, values);
        sender.Send(message);
        OnParameterSent?.Invoke(new VRChatOscMessage(message));
    }

    public void Initialise(IPEndPoint send, IPEndPoint receive)
    {
        sendEndpoint = send;
        receiveEndpoint = receive;
    }

    public Task EnableSend()
    {
        if (sendEndpoint is null) throw new InvalidOperationException($"Please call {nameof(Initialise)} first");

        return sender.ConnectAsync(sendEndpoint);
    }

    public void EnableReceive()
    {
        if (receiveEndpoint is null) throw new InvalidOperationException($"Please call {nameof(Initialise)} first");

        receiver.Connect(receiveEndpoint);
    }

    public void DisableSend() => sender.Disconnect();
    public Task DisableReceive() => receiver.DisconnectAsync();

    public void Init(ConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    private Task<OSCQueryNode?> findParameter(string parameterName) => findAddress(VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX + parameterName);

    private async Task<OSCQueryNode?> findAddress(string address)
    {
        try
        {
            if (!connectionManager.IsConnected) return null;

            address = address.Replace(" ", "%20");
            var url = $"http://127.0.0.1:{connectionManager.VRChatQueryPort}{address}";

            var response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var node = JsonConvert.DeserializeObject<OSCQueryNode>(content);

            return node is null || node.Access == OSCQueryNodeAccess.NoValue ? null : node;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Exception when trying to find parameter: {address}");
            return null;
        }
    }

    public async Task<ReceivedParameter?> FindParameter(string parameterName)
    {
        var node = await findParameter(parameterName);
        if (node?.Value is null || node.Value.Length == 0) return null;

        object parameterValue = node.OscType switch
        {
            "f" => Convert.ToSingle(node.Value[0]),
            "i" => Convert.ToInt32(node.Value[0]),
            "T" => Convert.ToBoolean(node.Value[0]),
            "F" => Convert.ToBoolean(node.Value[0]),
            _ => throw new InvalidOperationException($"Unknown type '{node.OscType}'")
        };

        return new ReceivedParameter(parameterName, parameterValue);
    }

    [Obsolete($"Use {nameof(FindParameter)} instead", true)]
    public async Task<object?> FindParameterValue(string parameterName) => (await FindParameter(parameterName))?.Value;

    [Obsolete($"Use {nameof(FindParameter)} instead", true)]
    public async Task<TypeCode?> FindParameterType(string parameterName) => (await FindParameter(parameterName))?.Type switch
    {
        ParameterType.Bool => TypeCode.Boolean,
        ParameterType.Int => TypeCode.Int32,
        ParameterType.Float => TypeCode.Single,
        _ => throw new ArgumentOutOfRangeException()
    };
}