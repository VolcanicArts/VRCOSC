// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Newtonsoft.Json;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Utils;

[JsonObject(MemberSerialization.OptIn)]
public class Keybind
{
    [JsonProperty("modifiers")]
    public List<Key> Modifiers { get; set; } = [];

    [JsonProperty("keys")]
    public List<Key> Keys { get; set; } = [];

    [JsonIgnore]
    public Key[] AllKeys => field ??= Modifiers.Concat(Keys).ToArray();

    public override string ToString()
    {
        if (Modifiers.Count == 0 && Keys.Count == 0) return "None";

        return string.Join(" + ", AllKeys.Select(key => key.ToReadableString()));
    }
}