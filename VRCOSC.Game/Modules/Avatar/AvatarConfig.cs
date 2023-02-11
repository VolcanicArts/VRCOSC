// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Avatar;

public class AvatarConfig
{
    [JsonProperty("id")]
    public string Id = null!;

    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("parameters")]
    public List<AvatarConfigParameter> Parameters = null!;
}

public class AvatarConfigParameter
{
    [JsonProperty("name")]
    public string Name = null!;
}
