// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Dolly.Serialisation;

internal class DollyManagerSerialiser : ProfiledSerialiser<DollyManager, SerialisableDollyManager>
{
    protected override string FileName => "dollies.json";

    internal DollyManagerSerialiser(Storage storage, DollyManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableDollyManager data)
    {
        Reference.Dollies.AddRange(data.Dollies.Select(serialisableDolly => new Dolly(Guid.Parse(serialisableDolly.Id))
        {
            Name = { Value = serialisableDolly.Name }
        }));

        return false;
    }
}