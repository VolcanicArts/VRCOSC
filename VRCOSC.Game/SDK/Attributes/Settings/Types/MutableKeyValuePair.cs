// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;

namespace VRCOSC.SDK.Attributes.Settings.Types;

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
        if (ReferenceEquals(null, other)) return false;

        return Key.Value.Equals(other.Key.Value) && Value.Value.Equals(other.Value.Value);
    }
}

public class MutableKeyValuePairSettingMetadata : ListModuleSettingMetadata
{
    public readonly string KeyTitle;
    public readonly string ValueTitle;

    public MutableKeyValuePairSettingMetadata(string title, string description, Type drawableModuleSettingType, Type drawableModuleSettingItemType, string keyTitle, string valueTitle)
        : base(title, description, drawableModuleSettingType, drawableModuleSettingItemType)
    {
        KeyTitle = keyTitle;
        ValueTitle = valueTitle;
    }
}

public class MutableKeyValuePairListModuleSetting : ListModuleSetting<MutableKeyValuePair>
{
    public new MutableKeyValuePairSettingMetadata Metadata => (MutableKeyValuePairSettingMetadata)base.Metadata;

    public MutableKeyValuePairListModuleSetting(MutableKeyValuePairSettingMetadata metadata, IEnumerable<MutableKeyValuePair> defaultValues)
        : base(metadata, defaultValues)
    {
    }

    protected override MutableKeyValuePair CloneValue(MutableKeyValuePair value) => new(value);
    protected override MutableKeyValuePair ConstructValue(JToken token) => token.ToObject<MutableKeyValuePair>()!;
    protected override MutableKeyValuePair CreateNewItem() => new();
}
