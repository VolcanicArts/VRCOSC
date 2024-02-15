// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.App.Serialisation;

public class SerialisableVersion
{
    [JsonProperty("version")]
    public int Version;

    [JsonConstructor]
    public SerialisableVersion()
    {
    }
}
