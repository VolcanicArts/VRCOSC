// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FastOSC;
using Newtonsoft.Json;
using VRCOSC.App.OSC.Query;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Settings;

namespace VRCOSC.App.OSC.VRChat;

public class VRChatOSCClient
{
    public Action<VRChatOSCMessage>? OnVRChatOSCMessageSent;
    public Func<VRChatOSCMessage, Task>? OnVRChatOSCMessageReceived;

    private readonly HttpClient client = new();

    private ConnectionManager connectionManager = null!;

    private readonly OSCSender sender = new();
    private readonly OSCReceiver receiver = new();

    public IPEndPoint? SendEndpoint { get; private set; }
    public IPEndPoint? ReceiveEndpoint { get; private set; }

    public VRChatOSCClient()
    {
        client.Timeout = TimeSpan.FromSeconds(2);

        receiver.OnPacketReceived += async packet =>
        {
            if (packet is OSCBundle) return;

            var message = (packet as OSCMessage)!;
            var data = new VRChatOSCMessage(message);
            if (data.Arguments.Length == 0) return;

            if (OnVRChatOSCMessageReceived is null) return;

            await OnVRChatOSCMessageReceived.Invoke(data);
        };
    }

    public void Send(string address, params object?[] values)
    {
        var message = new OSCMessage(address, values);
        sender.Send(message);
        OnVRChatOSCMessageSent?.Invoke(new VRChatOSCMessage(message));
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

    public async Task<OSCQueryNode?> RequestNode(string address)
    {
        var connectionMode = SettingsManager.GetInstance().GetValue<ConnectionMode>(VRCOSCSetting.ConnectionMode);

        // OSCQuery from VRChat is only broadcast on loopback so we'll turn it off for non-local modes
        if (connectionMode != ConnectionMode.Local || !connectionManager.IsConnected) return null;

        address = address.Replace(" ", "%20");
        var url = $"http://{connectionManager.VRChatIP}:{connectionManager.VRChatQueryPort}{address}";

        try
        {
            var response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OSCQueryNode>(content);
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<VRChatParameter>> RequestAllParameters()
    {
        var rootNode = await RequestNode(VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS);
        if (rootNode is null) return [];

        var parameters = new List<VRChatParameter>();
        auditNode(rootNode, parameters);
        return parameters;
    }

    private static void auditNode(OSCQueryNode node, List<VRChatParameter> parameters)
    {
        if (node.Value is not null)
        {
            var rawName = node.FullPath[(VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS.Length + 1)..];

            VRChatParameter parameter = node.OscType switch
            {
                "f" => new VRChatParameter(rawName, (float)(double)node.Value[0]),
                "i" => new VRChatParameter(rawName, (int)(long)node.Value[0]),
                "T" => new VRChatParameter(rawName, (bool)node.Value[0]), // T gets returned for true and false
                _ => throw new InvalidOperationException($"Unknown type '{node.OscType}'")
            };

            parameters.Add(parameter);
        }

        if (node.Contents is null) return;

        foreach (var innerNode in node.Contents.Values)
        {
            auditNode(innerNode, parameters);
        }
    }

    public async Task<string?> RequestCurrentAvatar()
    {
        var node = await RequestNode(VRChatOSCConstants.ADDRESS_AVATAR_CHANGE);
        if (node?.Value is null || node.Value.Length == 0) return null;

        return (string)node.Value[0];
    }
}