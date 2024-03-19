// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Types;

public class MutableKeyValuePair : IEquatable<MutableKeyValuePair>
{
    [JsonProperty("key")]
    public Observable<string> Key { get; } = new(string.Empty);

    [JsonProperty("value")]
    public Observable<string> Value { get; } = new(string.Empty);

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

public class MutableKeyValuePairSettingMetadata : ModuleSettingMetadata
{
    public string KeyTitle { get; }
    public string ValueTitle { get; }

    public MutableKeyValuePairSettingMetadata(string title, string description, Type pageType, string keyTitle, string valueTitle)
        : base(title, description, pageType)
    {
        KeyTitle = keyTitle;
        ValueTitle = valueTitle;
    }
}

public class MutableKeyValuePairListModuleSetting : ListModuleSetting<MutableKeyValuePair>
{
    public new MutableKeyValuePairSettingMetadata Metadata => (MutableKeyValuePairSettingMetadata)base.Metadata;

    public MutableKeyValuePairListModuleSetting(MutableKeyValuePairSettingMetadata metadata, IEnumerable<MutableKeyValuePair> defaultValues, bool rowNumberVisible)
        : base(metadata, defaultValues, rowNumberVisible)
    {
    }

    protected override MutableKeyValuePair CloneValue(MutableKeyValuePair value) => new(value);

    protected override MutableKeyValuePair ConstructValue(JToken token)
    {
        var instance = token.ToObject<MutableKeyValuePair>()!;
        instance.Key.Subscribe(_ => RequestSerialisation?.Invoke());
        instance.Value.Subscribe(_ => RequestSerialisation?.Invoke());
        return instance;
    }

    protected override MutableKeyValuePair CreateNewItem()
    {
        var instance = new MutableKeyValuePair();
        instance.Key.Subscribe(_ => RequestSerialisation?.Invoke());
        instance.Value.Subscribe(_ => RequestSerialisation?.Invoke());
        return instance;
    }
}
