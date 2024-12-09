// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router.Serialisation;

public class RouterManagerSerialiser : ProfiledSerialiser<RouterManager, SerialisableRouterManager>
{
    protected override string Directory => "configuration";
    protected override string FileName => "router.json";

    public RouterManagerSerialiser(Storage storage, RouterManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableRouterManager data)
    {
        Reference.Routes.AddRange(data.Routes);

        return false;
    }
}