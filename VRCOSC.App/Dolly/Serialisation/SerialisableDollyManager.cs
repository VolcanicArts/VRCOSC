// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;

namespace VRCOSC.App.Dolly.Serialisation;

[JsonObject(MemberSerialization.OptIn)]
internal class SerialisableDollyManager : SerialisableVersion
{
    [JsonProperty("dollies")]
    public List<SerialisableDolly> Dollies = [];

    [JsonConstructor]
    public SerialisableDollyManager()
    {
    }

    public SerialisableDollyManager(DollyManager dollyManager)
    {
        Version = 1;

        Dollies.AddRange(dollyManager.Dollies.Select(dolly => new SerialisableDolly(dolly)));
    }
}

[JsonObject(MemberSerialization.OptIn)]
internal class SerialisableDolly
{
    [JsonProperty("id")]
    public string Id = null!;

    [JsonProperty("name")]
    public string Name = null!;

    [JsonConstructor]
    public SerialisableDolly()
    {
    }

    public SerialisableDolly(Dolly dolly)
    {
        Id = dolly.Id.ToString();
        Name = dolly.Name.Value;
    }
}