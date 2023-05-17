// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.OSC;
using VRCOSC.Game.Router.Serialisation.Legacy;
using VRCOSC.Game.Router.Serialisation.V1;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Managers;

public class RouterManager
{
    public BindableList<RouterData> Store = new();

    private readonly SerialisationManager serialisationManager;

    public RouterManager(Storage storage, NotificationContainer notification)
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(0, new LegacyRouterSerialiser(storage, notification, this));
        serialisationManager.RegisterSerialiser(1, new RouterSerialiser(storage, notification, this));
    }

    public void Load()
    {
        Deserialise();

        Store.BindCollectionChanged((_, _) => Serialise());

        Store.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (RouterData newRouterData in e.NewItems)
                {
                    newRouterData.Label.BindValueChanged(_ => Serialise());
                    newRouterData.Endpoints.SendAddress.BindValueChanged(_ => Serialise());
                    newRouterData.Endpoints.ReceiveAddress.BindValueChanged(_ => Serialise());
                    newRouterData.Endpoints.SendPort.BindValueChanged(_ => Serialise());
                    newRouterData.Endpoints.ReceivePort.BindValueChanged(_ => Serialise());
                }
            }
        }, true);
    }

    public void Deserialise()
    {
        serialisationManager.Deserialise();
    }

    public void Serialise()
    {
        serialisationManager.Serialise();
    }
}

public class RouterData
{
    public readonly Bindable<string> Label = new(string.Empty);
    public OSCRouterEndpoints Endpoints = new();
}
