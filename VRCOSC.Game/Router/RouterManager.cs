// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Router.Serialisation;
using VRCOSC.Serialisation;

namespace VRCOSC.Router;

public class RouterManager
{
    public BindableList<Route> Routes = new();

    private readonly SerialisationManager serialisationManager;

    public RouterManager(AppManager appManager, Storage storage)
    {
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
}
