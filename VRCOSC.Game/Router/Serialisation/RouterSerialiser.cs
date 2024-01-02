// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Net;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Profiles;
using VRCOSC.Serialisation;

namespace VRCOSC.Router.Serialisation;

public class RouterSerialiser : ProfiledSerialiser<RouterManager, SerialisableRouterManager>
{
    protected override string FileName => "router.json";

    public RouterSerialiser(Storage storage, RouterManager routerManager, Bindable<Profile> activeProfile)
        : base(storage, routerManager, activeProfile)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableRouterManager data)
    {
        Reference.Routes.ReplaceRange(0, Reference.Routes.Count, data.Data.Select(routerData => new Route
        {
            Name = { Value = routerData.Name },
            Endpoint = { Value = new IPEndPoint(IPAddress.Parse(routerData.Address), routerData.Port) }
        }));

        return false;
    }
}
