// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Modules.Attributes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

/// <summary>
/// For use with value types
/// </summary>
public abstract class ValueModuleSetting<T> : ModuleSetting
{
    public Observable<T> Attribute { get; }

    protected ValueModuleSetting(string title, string description, Type viewType, T defaultValue)
        : base(title, description, viewType)
    {
        Attribute = new Observable<T>(defaultValue);
        Attribute.Subscribe(_ => OnSettingChange?.Invoke());
    }

    internal override bool IsDefault() => Attribute.IsDefault;
}

public class BoolModuleSetting : ValueModuleSetting<bool>
{
    public BoolModuleSetting(string title, string description, Type viewType, bool defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not bool boolValue) return false;

        Attribute.Value = boolValue;
        return true;
    }

    internal override object Serialise() => Attribute.Value;

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(bool))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(false, typeof(T));
        return false;
    }
}

public class StringModuleSetting : ValueModuleSetting<string>
{
    public StringModuleSetting(string title, string description, Type viewType, string defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not string stringValue) return false;

        Attribute.Value = stringValue;
        return true;
    }

    internal override object Serialise() => Attribute.Value;

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(string))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(string.Empty, typeof(T));
        return false;
    }
}

public class IntModuleSetting : ValueModuleSetting<int>
{
    public IntModuleSetting(string title, string description, Type viewType, int defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override bool Deserialise(object? ingestValue)
    {
        // json stores as long
        if (ingestValue is not long longValue) return false;

        Attribute.Value = (int)longValue;
        return true;
    }

    internal override object Serialise() => Attribute.Value;

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(int))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(0, typeof(T));
        return false;
    }
}

public class FloatModuleSetting : ValueModuleSetting<float>
{
    public FloatModuleSetting(string title, string description, Type viewType, float defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override bool Deserialise(object? ingestValue)
    {
        // json stores as double
        if (ingestValue is not double doubleValue) return false;

        Attribute.Value = (float)doubleValue;
        return true;
    }

    internal override object Serialise() => Attribute.Value;

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(float))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(0f, typeof(T));
        return false;
    }
}

public class EnumModuleSetting : ValueModuleSetting<int>
{
    internal readonly Type EnumType;

    public EnumModuleSetting(string title, string description, Type viewType, int defaultValue, Type enumType)
        : base(title, description, viewType, defaultValue)
    {
        EnumType = enumType;
    }

    internal override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not int intValue) return false;

        Attribute.Value = intValue;
        return true;
    }

    internal override object Serialise() => Attribute.Value;

    public override bool GetValue<TOut>(out TOut returnValue)
    {
        if (EnumType == typeof(TOut))
        {
            returnValue = (TOut)Enum.ToObject(EnumType, Attribute.Value);
            return true;
        }

        returnValue = (TOut)Enum.ToObject(typeof(TOut), 0);
        return false;
    }
}

public class DropdownListModuleSetting : StringModuleSetting
{
    public IEnumerable<DropdownItem> Items { get; internal set; }

    public DropdownListModuleSetting(string title, string description, Type viewType, IEnumerable<DropdownItem> items, DropdownItem defaultItem)
        : base(title, description, viewType, defaultItem.ID)
    {
        // take a copy to stop developers holding a reference
        Items = items.ToList().AsReadOnly();
    }
}

public class SliderModuleSetting : ValueModuleSetting<float>
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

    internal override object Serialise() => Attribute.Value;

    internal override bool Deserialise(object? ingestValue)
    {
        // json stores as double
        if (ingestValue is not double doubleValue) return false;

        var floatValue = (float)doubleValue;
        Attribute.Value = Math.Clamp(floatValue, MinValue, MaxValue);

        return true;
    }

    public override bool GetValue<TOut>(out TOut returnValue)
    {
        if (typeof(TOut) == typeof(float) && ValueType == typeof(float))
        {
            returnValue = (TOut)Convert.ChangeType(Attribute.Value, typeof(TOut));
            return true;
        }

        if (typeof(TOut) == typeof(int) && ValueType == typeof(int))
        {
            returnValue = (TOut)Convert.ChangeType(Attribute.Value, typeof(TOut));
            return true;
        }

        returnValue = (TOut)Convert.ChangeType(0f, typeof(TOut));
        return false;
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

    internal override object Serialise() => Attribute.Value.UtcTicks;

    internal override bool Deserialise(object? ingestValue)
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

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(DateTimeOffset))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(DateTimeOffset.FromUnixTimeSeconds(0), typeof(T));
        return false;
    }
}