// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Bindables;

namespace VRCOSC.Game.Modules.SDK.Attributes;

public abstract class ModuleAttribute
{
    /// <summary>
    /// The title of this <see cref="ModuleAttribute"/>
    /// </summary>
    internal readonly string Title;

    /// <summary>
    /// The description of this <see cref="ModuleAttribute"/>
    /// </summary>
    internal readonly string Description;

    /// <summary>
    /// Initialises this <see cref="ModuleAttribute"/> before <see cref="Deserialise"/> is ran
    /// </summary>
    internal abstract void Load();

    /// <summary>
    /// Resets this <see cref="ModuleAttribute"/>'s value to its default value
    /// </summary>
    internal abstract void SetDefault();

    /// <summary>
    /// If this <see cref="ModuleAttribute"/>'s value is currently the default value
    /// </summary>
    /// <returns></returns>
    internal abstract bool IsDefault();

    /// <summary>
    /// Attempts to deserialise an object into this <see cref="ModuleAttribute"/>'s value's type
    /// </summary>
    /// <param name="ingestValue">The value to attempt to deserialise</param>
    /// <returns>True if the deserialisation was successful, otherwise false</returns>
    internal abstract bool Deserialise(object ingestValue);

    /// <summary>
    /// Retrieves the unknown typed value for this <see cref="ModuleAttribute"/>. Useful for when you don't care about the value's type
    /// </summary>
    internal abstract object? GetValue();

    /// <summary>
    /// Retrieves the value for this <see cref="ModuleAttribute"/>
    /// </summary>
    /// <typeparam name="TValueType">The type to attempt to convert the value to</typeparam>
    /// <returns>True if the value was converted successfully, otherwise false</returns>
    internal abstract bool GetValue<TValueType>(out TValueType? outValue);

    protected ModuleAttribute(string title, string description)
    {
        Title = title;
        Description = description;
    }
}

public abstract class BindableModuleAttribute<T> : ModuleAttribute
{
    public Bindable<T> Attribute { get; private set; } = null!;

    protected readonly T DefaultValue;

    protected abstract Bindable<T> CreateBindable();

    internal override void Load() => Attribute = CreateBindable();
    internal override bool IsDefault() => Attribute.IsDefault;
    internal override void SetDefault() => Attribute.SetDefault();

    internal override object? GetValue() => Attribute.Value;

    internal override bool GetValue<TValueType>(out TValueType? outValue) where TValueType : default
    {
        if (Attribute.Value is not TValueType valueAsType)
        {
            outValue = default;
            return false;
        }

        outValue = valueAsType;
        return true;
    }

    protected BindableModuleAttribute(string title, string description, T defaultValue)
        : base(title, description)
    {
        DefaultValue = defaultValue;
    }
}

internal class BindableBoolModuleAttribute : BindableModuleAttribute<bool>
{
    protected override Bindable<bool> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not bool boolIngestValue) return false;

        Attribute.Value = boolIngestValue;
        return true;
    }

    public BindableBoolModuleAttribute(string title, string description, bool defaultValue)
        : base(title, description, defaultValue)
    {
    }
}

internal class BindableIntModuleAttribute : BindableModuleAttribute<int>
{
    protected override Bindable<int> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not int intIngestValue) return false;

        Attribute.Value = intIngestValue;
        return true;
    }

    internal BindableIntModuleAttribute(string title, string description, int defaultValue)
        : base(title, description, defaultValue)
    {
    }
}

internal class BindableFloatModuleAttribute : BindableModuleAttribute<float>
{
    protected override Bindable<float> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not float floatIngestValue) return false;

        Attribute.Value = floatIngestValue;
        return true;
    }

    internal BindableFloatModuleAttribute(string title, string description, float defaultValue)
        : base(title, description, defaultValue)
    {
    }
}

internal class BindableStringModuleAttribute : BindableModuleAttribute<string>
{
    protected override Bindable<string> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not string stringIngestValue) return false;

        Attribute.Value = stringIngestValue;
        return true;
    }

    internal BindableStringModuleAttribute(string title, string description, string defaultValue)
        : base(title, description, defaultValue)
    {
    }
}

internal class BindableEnumModuleAttribute<TEnum> : BindableModuleAttribute<TEnum> where TEnum : Enum
{
    protected override Bindable<TEnum> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not int intIngestValue) return false;

        Attribute.Value = (TEnum)Enum.ToObject(typeof(TEnum), intIngestValue);
        return true;
    }

    internal override object GetValue() => Convert.ToInt32(Attribute.Value);

    internal BindableEnumModuleAttribute(string title, string description, TEnum defaultValue)
        : base(title, description, defaultValue)
    {
    }
}
