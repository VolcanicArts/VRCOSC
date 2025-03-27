// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

/// <summary>
/// For use with value types
/// </summary>
public abstract class ValueModuleSetting<T> : ModuleSetting where T : notnull
{
    public Observable<T> Attribute { get; }

    protected ValueModuleSetting(string title, string description, Type viewType, T defaultValue)
        : base(title, description, viewType)
    {
        Attribute = new Observable<T>(defaultValue);
        Attribute.Subscribe(_ => OnSettingChange?.Invoke());
    }

    protected override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not T tValue) return false;

        Attribute.Value = tValue;
        return true;
    }

    protected override object Serialise() => Attribute.Value;

    protected override bool IsDefault() => Attribute.IsDefault;

    public override TOut GetValue<TOut>()
    {
        if (Attribute.Value is not TOut castValue) throw new InvalidCastException($"Unable to cast {typeof(T).Name} to {typeof(TOut).Name}");

        return castValue;
    }
}

public class BoolModuleSetting : ValueModuleSetting<bool>
{
    public BoolModuleSetting(string title, string description, Type viewType, bool defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }
}

public class StringModuleSetting : ValueModuleSetting<string>
{
    public StringModuleSetting(string title, string description, Type viewType, string defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }
}

public class IntModuleSetting : ValueModuleSetting<int>
{
    public IntModuleSetting(string title, string description, Type viewType, int defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    // json stores as long
    protected override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is long longValue)
        {
            return base.Deserialise((int)longValue);
        }

        if (ingestValue is int intValue)
        {
            return base.Deserialise(intValue);
        }

        return false;
    }
}

public class FloatModuleSetting : ValueModuleSetting<float>
{
    public FloatModuleSetting(string title, string description, Type viewType, float defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    // json stores as double
    protected override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is double doubleValue)
        {
            return base.Deserialise((float)doubleValue);
        }

        if (ingestValue is float floatValue)
        {
            return base.Deserialise(floatValue);
        }

        if (ingestValue is long longValue)
        {
            return base.Deserialise((float)longValue);
        }

        if (ingestValue is int intValue)
        {
            return base.Deserialise((float)intValue);
        }

        return false;
    }
}

public class EnumModuleSetting : IntModuleSetting
{
    internal readonly Type EnumType;

    public EnumModuleSetting(string title, string description, Type viewType, int defaultValue, Type enumType)
        : base(title, description, viewType, defaultValue)
    {
        EnumType = enumType;
    }

    public override TOut GetValue<TOut>()
    {
        if (typeof(TOut) != EnumType) throw new InvalidCastException($"{typeof(TOut).Name} is not the stored enum type {EnumType.Name}");

        return (TOut)Enum.ToObject(EnumType, Attribute.Value);
    }
}

public class DropdownListModuleSetting : StringModuleSetting
{
    internal readonly Type ItemType;

    public string TitlePath { get; }
    public string ValuePath { get; }

    public IEnumerable<object> Items { get; }

    public DropdownListModuleSetting(string title, string description, Type viewType, IEnumerable<object> items, string defaultValue, string titlePath, string valuePath)
        : base(title, description, viewType, defaultValue)
    {
        ItemType = items.GetType().GenericTypeArguments[0];

        TitlePath = titlePath;
        ValuePath = valuePath;

        // take a copy to stop developers holding a reference
        Items = items.ToList().AsReadOnly();
    }

    // this specifically returns the selected value
    public override TOut GetValue<TOut>()
    {
        if (typeof(TOut) != ItemType) throw new InvalidCastException($"{typeof(TOut).Name} is not the stored item type {ItemType.Name}");

        var valueProperty = ItemType.GetProperty(ValuePath);
        return (TOut)Items.First(item => valueProperty!.GetValue(item)!.ToString()! == Attribute.Value);
    }
}

public class SliderModuleSetting : FloatModuleSetting
{
    public Type ValueType { get; }
    public float MinValue { get; }
    public float MaxValue { get; }
    public float TickFrequency { get; }

    public SliderModuleSetting(string title, string description, Type viewType, float defaultValue, float minValue, float maxValue, float tickFrequency)
        : base(title, description, viewType, defaultValue)
    {
        ValueType = typeof(float);

        MinValue = minValue;
        MaxValue = maxValue;
        TickFrequency = tickFrequency;
    }

    public SliderModuleSetting(string title, string description, Type viewType, int defaultValue, int minValue, int maxValue, int tickFrequency)
        : base(title, description, viewType, defaultValue)
    {
        ValueType = typeof(int);

        MinValue = minValue;
        MaxValue = maxValue;
        TickFrequency = tickFrequency;
    }

    protected override bool Deserialise(object? ingestValue)
    {
        if (!base.Deserialise(ingestValue)) return false;

        Attribute.Value = Math.Clamp(Attribute.Value, MinValue, MaxValue);

        return true;
    }

    public override TOut GetValue<TOut>()
    {
        if (ValueType == typeof(float))
        {
            if (Attribute.Value is TOut castValue)
            {
                return castValue;
            }

            throw new InvalidCastException($"{typeof(TOut).Name} cannot be cast to a float");
        }

        if (ValueType == typeof(int))
        {
            if ((int)Attribute.Value is TOut castValue)
            {
                return castValue;
            }

            throw new InvalidCastException($"{typeof(TOut).Name} cannot be cast to an int");
        }

        throw new InvalidOperationException($"{ValueType.Name} is neither an int nor a float");
    }
}

/// <summary>
/// Serialises the time directly in UTC and converts to a local timezone's DateTimeOffset on deserialise
/// </summary>
public class DateTimeModuleSetting : ValueModuleSetting<DateTimeOffset>
{
    public DateTimeModuleSetting(string title, string description, Type viewType, DateTimeOffset defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    protected override object Serialise() => Attribute.Value.UtcTicks;

    protected override bool Deserialise(object? ingestValue)
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
}