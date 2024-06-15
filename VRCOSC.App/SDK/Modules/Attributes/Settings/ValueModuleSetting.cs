// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

/// <summary>
/// For use with value types
/// </summary>
public abstract class ValueModuleSetting<T> : ModuleSetting
{
    public Observable<T> Attribute { get; private set; } = null!;

    protected readonly T DefaultValue;

    protected abstract Observable<T> CreateObservable();

    public override void PreDeserialise()
    {
        Attribute = CreateObservable();
        Attribute.Subscribe(_ =>
        {
            OnSettingChange?.Invoke();
            RequestSerialisation?.Invoke();
        });
    }

    public override bool IsDefault() => Attribute.IsDefault;
    public override void SetDefault() => Attribute.SetDefault();

    public override object GetRawValue() => Attribute.Value!;

    protected ValueModuleSetting(ModuleSettingMetadata metadata, T defaultValue)
        : base(metadata)
    {
        DefaultValue = defaultValue;
    }
}

public class BoolModuleSetting : ValueModuleSetting<bool>
{
    protected override Observable<bool> CreateObservable() => new(DefaultValue);

    public override bool Deserialise(object ingestValue)
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

public class StringModuleSetting : ValueModuleSetting<string>
{
    protected override Observable<string> CreateObservable() => new(DefaultValue);

    public override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not string stringIngestValue) return false;

        Attribute.Value = stringIngestValue;
        return true;
    }

    internal StringModuleSetting(ModuleSettingMetadata metadata, string defaultValue)
        : base(metadata, defaultValue)
    {
    }
}

public class IntModuleSetting : ValueModuleSetting<int>
{
    protected override Observable<int> CreateObservable() => new(DefaultValue);

    public override bool Deserialise(object ingestValue)
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

public class FloatModuleSetting : ValueModuleSetting<float>
{
    protected override Observable<float> CreateObservable() => new(DefaultValue);

    public override bool Deserialise(object ingestValue)
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

public class EnumModuleSetting : ValueModuleSetting<int>
{
    protected override Observable<int> CreateObservable() => new(DefaultValue);

    public override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not long intIngestValue) return false;

        Attribute.Value = (int)intIngestValue;
        return true;
    }

    public override bool GetValue<TValueType>(out TValueType? outValue) where TValueType : default
    {
        if (typeof(TValueType) == EnumType)
        {
            outValue = (TValueType)Enum.Parse(EnumType, Attribute.Value.ToString());
            return true;
        }

        outValue = default;
        return false;
    }

    public Type EnumType { get; }

    internal EnumModuleSetting(ModuleSettingMetadata metadata, int defaultValue, Type enumType)
        : base(metadata, defaultValue)
    {
        EnumType = enumType;
    }
}

public class SliderModuleSetting : ValueModuleSetting<float>
{
    public Type ValueType;

    public float MinValue { get; }
    public float MaxValue { get; }
    public float TickFrequency { get; }

    protected override Observable<float> CreateObservable() => new(DefaultValue);

    public override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not double floatIngestValue) return false;

        Attribute.Value = Math.Clamp((float)floatIngestValue, MinValue, MaxValue);
        return true;
    }

    public override bool GetValue<TValueType>(out TValueType? outValue) where TValueType : default
    {
        var value = (float)GetRawValue();

        if (typeof(TValueType) == typeof(float))
        {
            outValue = (TValueType)Convert.ChangeType(value, TypeCode.Single);
            return true;
        }

        if (typeof(TValueType) == typeof(int))
        {
            outValue = (TValueType)Convert.ChangeType(value, TypeCode.Int32);
            return true;
        }

        outValue = default;
        return false;
    }

    internal SliderModuleSetting(ModuleSettingMetadata metadata, float defaultValue, float minValue, float maxValue, float tickFrequency)
        : base(metadata, defaultValue)
    {
        ValueType = typeof(float);

        MinValue = minValue;
        MaxValue = maxValue;
        TickFrequency = tickFrequency;
    }

    internal SliderModuleSetting(ModuleSettingMetadata metadata, int defaultValue, int minValue, int maxValue, int tickFrequency)
        : base(metadata, defaultValue)
    {
        ValueType = typeof(int);

        MinValue = minValue;
        MaxValue = maxValue;
        TickFrequency = tickFrequency;
    }
}

/// <summary>
/// Serialises the time directly in UTC and converts to a local timezone's DateTimeOffset on deserialise
/// </summary>
public class DateTimeModuleSetting : ValueModuleSetting<DateTimeOffset>
{
    public DateTimeModuleSetting(ModuleSettingMetadata metadata, DateTimeOffset defaultValue)
        : base(metadata, defaultValue)
    {
    }

    public override object GetSerialisableValue() => Attribute.Value.UtcTicks;

    public override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not long ingestUtcTicks) return false;

        // Since we're storing the UTC ticks we have to do some conversions to adjust it to local time on load
        // This allows people to share configs and have it automatically adjust to their timezones

        var utcDateTime = new DateTime(ingestUtcTicks, DateTimeKind.Utc);
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
        var dateTimeOffset = new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));

        Attribute.Value = dateTimeOffset;

        return true;
    }

    protected override Observable<DateTimeOffset> CreateObservable() => new(DefaultValue);
}
