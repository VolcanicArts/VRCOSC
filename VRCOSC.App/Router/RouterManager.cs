// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using VRCOSC.App.OSC.Client;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Router.Serialisation;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router;

public class RouterManager
{
    private static RouterManager? instance;
    public static RouterManager GetInstance() => instance ??= new RouterManager();

    public ObservableCollection<RouterInstance> Routes { get; } = new();
    private readonly SerialisationManager serialisationManager;

    private readonly List<(RouterInstance, OscSender)> senders = new();

    public RouterManager()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new RouterManagerSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Load()
    {
        Routes.Clear();

        Routes.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (RouterInstance routerInstance in e.NewItems)
                {
                    routerInstance.Name.Subscribe(_ => serialisationManager.Serialise());
                    routerInstance.Endpoint.Subscribe(_ => serialisationManager.Serialise());
                }
            }

            serialisationManager.Serialise();
        };

        serialisationManager.Deserialise();
    }

    public void Start()
    {
        foreach (var route in Routes)
        {
            try
            {
                var address = route.Endpoint.Value.Split(":")[0];
                var port = int.Parse(route.Endpoint.Value.Split(":")[1]);

                var endpoint = new IPEndPoint(IPAddress.Parse(address), port);

                Logger.Log($"Starting router instance `{route.Name.Value}` on {endpoint}");

                var sender = new OscSender();
                sender.Initialise(endpoint);
                sender.Enable();

                senders.Add((route, sender));
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Failed to start router instance named {route.Name.Value}");
            }
        }

        AppManager.GetInstance().VRChatOscClient.OnParameterReceived += onParameterReceived;
    }

    public void Stop()
    {
        AppManager.GetInstance().VRChatOscClient.OnParameterReceived -= onParameterReceived;

        foreach (var (route, sender) in senders)
        {
            Logger.Log($"Stopping router instance '{route.Name.Value}'");
            sender.Disable();
        }

        senders.Clear();
    }

    private void onParameterReceived(VRChatOscMessage message)
    {
        foreach (var (_, sender) in senders)
        {
            sender.Send(OscEncoder.Encode(message));
        }
    }
}
