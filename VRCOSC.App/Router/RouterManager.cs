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
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router;

public class RouterManager
{
    private static RouterManager? instance;
    public static RouterManager GetInstance() => instance ??= new RouterManager();

    public ObservableCollection<RouterInstance> Routes { get; } = new();
    private SerialisationManager serialisationManager = null!;

    private readonly List<(RouterInstance, OscSender)> senders = new();

    private bool started;

    public void Load()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new RouterManagerSerialiser(AppManager.GetInstance().Storage, this));

        started = false;
        Routes.Clear();

        serialisationManager.Deserialise();

        Routes.OnCollectionChanged((newItems, _) =>
        {
            foreach (var newInstance in newItems)
            {
                newInstance.Name.Subscribe(_ => serialisationManager.Serialise());
                newInstance.Endpoint.Subscribe(_ => serialisationManager.Serialise());
            }
        }, true);

        Routes.OnCollectionChanged((_, _) => serialisationManager.Serialise());
    }

    public void Start()
    {
        if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.EnableRouter)) return;

        foreach (var route in Routes)
        {
            try
            {
                var address = route.Endpoint.Value.Split(":")[0];
                var port = int.Parse(route.Endpoint.Value.Split(":")[1]);

                var endpoint = new IPEndPoint(IPAddress.Parse(address), port);

                Logger.Log($"Starting router instance `{route.Name.Value}` on {endpoint}", LoggingTarget.Terminal);

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

        started = true;
    }

    public void Stop()
    {
        if (!started) return;

        AppManager.GetInstance().VRChatOscClient.OnParameterReceived -= onParameterReceived;

        foreach (var (route, sender) in senders)
        {
            Logger.Log($"Stopping router instance '{route.Name.Value}'", LoggingTarget.Terminal);
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