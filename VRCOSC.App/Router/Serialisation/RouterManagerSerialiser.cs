// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Profiles;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router.Serialisation;

public class RouterManagerSerialiser : ProfiledSerialiser<RouterManager, SerialisableRouterManager>
{
    protected override string FileName => "router.json";

    public RouterManagerSerialiser(Storage storage, RouterManager reference, Observable<Profile> activeProfile)
        : base(storage, reference, activeProfile)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableRouterManager data)
    {
        Reference.Routes.AddRange(data.Routes);

        return false;
    }
}
