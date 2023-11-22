// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Bindables;

namespace VRCOSC.Game.SDK.Attributes.Settings;

/// <summary>
/// For use with value types
/// </summary>
public abstract class ValueModuleSetting<T> : ModuleSetting
{
    public virtual Bindable<T> Attribute { get; private set; } = null!;

    protected readonly T DefaultValue;

    protected abstract Bindable<T> CreateBindable();

    internal override void Load()
    {
        Attribute = CreateBindable();
        Attribute.BindValueChanged(_ => RequestSerialisation?.Invoke());
    }

    internal override bool IsDefault() => Attribute.IsDefault;
    internal override void SetDefault() => Attribute.SetDefault();

    internal override object GetRawValue() => Attribute.Value!;

    protected ValueModuleSetting(ModuleSettingMetadata metadata, T defaultValue)
        : base(metadata)
    {
        DefaultValue = defaultValue;
    }
}

public class BoolModuleSetting : ValueModuleSetting<bool>
{
    protected override BindableBool CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not bool boolIngestValue) return false;

        Attribute.Value = boolIngestValue;
        return true;
    }

    public BoolModuleSetting(ModuleSettingMetadata metadata, bool defaultValue)
        : base(metadata, defaultValue)
    {
    }
}

public class IntModuleSetting : ValueModuleSetting<int>
{
    protected override BindableNumber<int> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not long intIngestValue) return false;

        Attribute.Value = (int)intIngestValue;
        return true;
    }

    internal IntModuleSetting(ModuleSettingMetadata metadata, int defaultValue)
        : base(metadata, defaultValue)
    {
    }
}

public class RangedIntModuleSetting : IntModuleSetting
{
    private readonly int minValue;
    private readonly int maxValue;

    public override BindableNumber<int> Attribute => (BindableNumber<int>)base.Attribute;

    protected override BindableNumber<int> CreateBindable() => new(DefaultValue)
    {
        MinValue = minValue,
        MaxValue = maxValue
    };

    internal RangedIntModuleSetting(ModuleSettingMetadata metadata, int defaultValue, int minValue, int maxValue)
        : base(metadata, defaultValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}

public class FloatModuleSetting : ValueModuleSetting<float>
{
    protected override BindableNumber<float> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not double floatIngestValue) return false;

        Attribute.Value = (float)floatIngestValue;
        return true;
    }

    internal FloatModuleSetting(ModuleSettingMetadata metadata, float defaultValue)
        : base(metadata, defaultValue)
    {
    }
}

public class RangedFloatModuleSetting : FloatModuleSetting
{
    private readonly float minValue;
    private readonly float maxValue;

    public override BindableNumber<float> Attribute => (BindableNumber<float>)base.Attribute;

    protected override BindableNumber<float> CreateBindable() => new(DefaultValue)
    {
        MinValue = minValue,
        MaxValue = maxValue
    };

    internal RangedFloatModuleSetting(ModuleSettingMetadata metadata, float defaultValue, float minValue, float maxValue)
        : base(metadata, defaultValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}

public class StringModuleSetting : ValueModuleSetting<string>
{
    internal readonly bool EmptyIsValid;

    protected override Bindable<string> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not string stringIngestValue) return false;

        Attribute.Value = stringIngestValue;
        return true;
    }

    internal StringModuleSetting(ModuleSettingMetadata metadata, bool emptyIsValid, string defaultValue)
        : base(metadata, defaultValue)
    {
        EmptyIsValid = emptyIsValid;
    }
}

public class EnumModuleSetting<TEnum> : ValueModuleSetting<TEnum> where TEnum : Enum
{
    protected override Bindable<TEnum> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not long intIngestValue) return false;

        Attribute.Value = (TEnum)Enum.ToObject(typeof(TEnum), (int)intIngestValue);
        return true;
    }

    internal EnumModuleSetting(ModuleSettingMetadata metadata, TEnum defaultValue)
        : base(metadata, defaultValue)
    {
    }
}
