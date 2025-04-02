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
using VRCOSC.App.Settings;
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

    public IPEndPoint? SendEndpoint { get; private set; }
    public IPEndPoint? ReceiveEndpoint { get; private set; }

    public VRChatOscClient()
    {
        client.Timeout = TimeSpan.FromMilliseconds(50);

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
        SendEndpoint = send;
        ReceiveEndpoint = receive;
    }

    public Task EnableSend()
    {
        if (SendEndpoint is null) throw new InvalidOperationException($"Please call {nameof(Initialise)} first");

        return sender.ConnectAsync(SendEndpoint);
    }

    public void EnableReceive()
    {
        if (ReceiveEndpoint is null) throw new InvalidOperationException($"Please call {nameof(Initialise)} first");

        receiver.Connect(ReceiveEndpoint);
    }

    public void DisableSend() => sender.Disconnect();
    public Task DisableReceive() => receiver.DisconnectAsync();

    public void Init(ConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public async Task<OSCQueryNode?> FindAddress(string address)
    {
        var oscMode = SettingsManager.GetInstance().GetValue<ConnectionMode>(VRCOSCSetting.ConnectionMode);

        // OSCQuery from VRChat is only broadcast on loopback so we'll turn it off for non-local modes
        if (oscMode != ConnectionMode.Local || !connectionManager.IsConnected) return null;

        address = address.Replace(" ", "%20");
        var url = $"http://{connectionManager.VRChatIP}:{connectionManager.VRChatQueryPort}{address}";

        try
        {
            var response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var node = JsonConvert.DeserializeObject<OSCQueryNode>(content);

            return node is null || node.Access == OSCQueryNodeAccess.NoValue ? null : node;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Exception when trying to find parameter: {url}");
            return null;
        }
    }

    public async Task<ReceivedParameter?> FindParameter(string parameterName)
    {
        var node = await FindAddress(VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX + parameterName);
        if (node?.Value is null || node.Value.Length == 0) return null;

        object parameterValue = node.OscType switch
        {
            "f" => Convert.ToSingle(node.Value[0]),
            "i" => Convert.ToInt32(node.Value[0]),
            // T gets returned for true and false
            "T" => Convert.ToBoolean(node.Value[0]),
            _ => throw new InvalidOperationException($"Unknown type '{node.OscType}'")
        };

        return new ReceivedParameter(parameterName, parameterValue);
    }

    public async Task<string?> FindCurrentAvatar()
    {
        var node = await FindAddress(VRChatOscConstants.ADDRESS_AVATAR_CHANGE);
        if (node?.Value is null || node.Value.Length == 0) return null;

        return Convert.ToString(node.Value[0]);
    }
}