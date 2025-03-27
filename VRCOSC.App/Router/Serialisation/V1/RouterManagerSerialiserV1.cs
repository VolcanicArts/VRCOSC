// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router.Serialisation.V1;

internal class RouterManagerSerialiserV1 : Serialiser<RouterManager, SerialisableRouterManagerV1>
{
    protected override string Directory => "configuration";
    protected override string FileName => "router.json";

    public RouterManagerSerialiserV1(Storage storage, RouterManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableRouterManagerV1 data)
    {
        Reference.Routes.AddRange(data.Routes.Select(serialisableInstance => new RouterInstance
        {
            Name = { Value = serialisableInstance.Name },
            Mode = { Value = serialisableInstance.Mode },
            Endpoint = { Value = serialisableInstance.Endpoint }
        }));

        return false;
    }
}