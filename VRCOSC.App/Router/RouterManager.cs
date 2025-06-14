// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using FastOSC;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Router.Serialisation.V1;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router;

public class RouterManager
{
    private static RouterManager? instance;
    internal static RouterManager GetInstance() => instance ??= new RouterManager();

    public ObservableCollection<RouterInstance> Routes { get; } = new();
    private SerialisationManager serialisationManager = null!;

    private readonly List<(RouterInstance, OSCSender)> senders = new();
    private readonly List<(RouterInstance, OSCReceiver)> receivers = new();

    private bool started;

    public void Load()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new RouterManagerSerialiserV1(AppManager.GetInstance().Storage, this));

        started = false;
        Routes.Clear();

        serialisationManager.Deserialise();

        Routes.OnCollectionChanged((newItems, _) =>
        {
            foreach (var newInstance in newItems)
            {
                newInstance.Name.Subscribe(_ => serialisationManager.Serialise());
                newInstance.Mode.Subscribe(_ => serialisationManager.Serialise());
                newInstance.Endpoint.Subscribe(_ => serialisationManager.Serialise());
            }
        }, true);

        Routes.OnCollectionChanged((_, _) => serialisationManager.Serialise());
    }

    public async Task Start()
    {
        foreach (var route in Routes)
        {
            try
            {
                var address = route.Endpoint.Value.Split(":")[0];
                var port = int.Parse(route.Endpoint.Value.Split(":")[1]);

                var endpoint = new IPEndPoint(IPAddress.Parse(address), port);

                if (route.Mode.Value == RouterMode.Send)
                {
                    Logger.Log($"Starting sender router instance `{route.Name.Value}` on {endpoint}", LoggingTarget.Terminal);

                    var sender = new OSCSender();
                    await sender.ConnectAsync(endpoint);

                    senders.Add((route, sender));
                }

                if (route.Mode.Value == RouterMode.Receive)
                {
                    Logger.Log($"Starting receiver router instance `{route.Name.Value}` on {endpoint}", LoggingTarget.Terminal);

                    var receiver = new OSCReceiver();
                    receiver.OnMessageReceived += message => AppManager.GetInstance().VRChatOscClient.Send(message.Address, message.Arguments);
                    receiver.Connect(endpoint);

                    receivers.Add((route, receiver));
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Failed to start router instance named {route.Name.Value}");
            }
        }

        AppManager.GetInstance().VRChatOscClient.OnVRChatOSCMessageReceived += onParameterReceived;

        started = true;
    }

    public async Task Stop()
    {
        if (!started) return;

        AppManager.GetInstance().VRChatOscClient.OnVRChatOSCMessageReceived -= onParameterReceived;

        foreach (var (route, sender) in senders)
        {
            Logger.Log($"Stopping sender router instance '{route.Name.Value}'", LoggingTarget.Terminal);
            sender.Disconnect();
        }

        foreach (var (route, receiver) in receivers)
        {
            Logger.Log($"Stopping receive router instance '{route.Name.Value}'", LoggingTarget.Terminal);
            await receiver.DisconnectAsync();
        }

        senders.Clear();
        receivers.Clear();
    }

    private void onParameterReceived(VRChatOSCMessage message)
    {
        foreach (var (_, sender) in senders)
        {
            sender.Send(message);
        }
    }
}