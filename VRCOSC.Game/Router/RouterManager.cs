// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.OSC.Client;
using VRCOSC.OSC.VRChat;
using VRCOSC.Router.Serialisation;
using VRCOSC.Serialisation;

namespace VRCOSC.Router;

public class RouterManager
{
    private readonly VRChatOscClient oscClient;
    private readonly SerialisationManager serialisationManager;
    private readonly List<OscSender> senders = new();

    public BindableList<Route> Routes = new();

    public RouterManager(AppManager appManager, Storage storage, VRChatOscClient oscClient)
    {
        this.oscClient = oscClient;
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new RouterSerialiser(storage, this, appManager.ProfileManager.ActiveProfile));
    }

    public void Load()
    {
        serialisationManager.Deserialise();

        Routes.BindCollectionChanged((_, _) => serialisationManager.Serialise());

        Routes.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is null) return;

            foreach (Route route in e.NewItems)
            {
                route.Name.BindValueChanged(_ => serialisationManager.Serialise());
                route.Endpoint.BindValueChanged(_ => serialisationManager.Serialise());
            }
        }, true);
    }

    public void Start()
    {
        Routes.ForEach(route =>
        {
            var sender = new OscSender();
            sender.Initialise(route.Endpoint.Value);
            senders.Add(sender);

            Logger.Log($"[Router] Initialising route {route.Name.Value} on {route.Endpoint.Value}", TerminalLogger.TARGET_NAME);
        });

        senders.ForEach(sender =>
        {
            oscClient.OnMessageReceived += message => sender.Send(OscEncoder.Encode(message));
            sender.Enable();
        });
    }

    public void Stop()
    {
        foreach (var sender in senders) sender.Disable();
        senders.Clear();
    }
}
