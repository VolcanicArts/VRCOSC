// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router.Serialisation.V2;

internal class RouterManagerSerialiserV2 : Serialiser<RouterManager, SerialisableRouterManagerV2>
{
    protected override string Directory => "configuration";
    protected override string FileName => "router.json";

    public RouterManagerSerialiserV2(Storage storage, RouterManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableRouterManagerV2 data)
    {
        Reference.Routes.AddRange(data.Routes.Select(serialisableInstance => new RouterInstance
        {
            Name = { Value = serialisableInstance.Name },
            ReceiveEnabled = { Value = serialisableInstance.ReceiveEnabled },
            ReceiveEndpoint = { Value = serialisableInstance.ReceiveEndpoint },
            SendEnabled = { Value = serialisableInstance.SendEnabled },
            SendEndpoint = { Value = serialisableInstance.SendEndpoint }
        }));

        return false;
    }
}