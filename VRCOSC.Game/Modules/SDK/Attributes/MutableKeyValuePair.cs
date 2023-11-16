// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace VRCOSC.Game.Modules.SDK.Attributes;

public class MutableKeyValuePair : IEquatable<MutableKeyValuePair>
{
    [JsonProperty("key")]
    public Bindable<string> Key = new(string.Empty);

    [JsonProperty("value")]
    public Bindable<string> Value = new(string.Empty);

    [JsonConstructor]
    public MutableKeyValuePair()
    {
    }

    public MutableKeyValuePair(MutableKeyValuePair other)
    {
        Key.Value = other.Key.Value;
        Value.Value = other.Value.Value;
    }

    public bool Equals(MutableKeyValuePair? other)
    {
        Debug.Assert(other is not null);

        return Key.Value.Equals(other.Key.Value) && Value.Value.Equals(other.Value.Value);
    }
}
