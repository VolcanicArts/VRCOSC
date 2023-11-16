// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
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
    /// Retrieves the value for this <see cref="ModuleAttribute"/> using a provided expected type
    /// </summary>
    /// <typeparam name="TValueType">The type to attempt to convert the value to</typeparam>
    /// <returns>True if the value was converted successfully, otherwise false</returns>
    internal bool GetValue<TValueType>(out TValueType? outValue)
    {
        var value = GetRawValue();

        if (value is TValueType valueAsType)
        {
            outValue = valueAsType;
            return true;
        }

        outValue = default;
        return false;
    }

    /// <summary>
    /// Retrieves the unknown raw typed value for this <see cref="ModuleAttribute"/>.
    /// </summary>
    internal abstract object? GetRawValue();

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

    internal override object? GetRawValue() => Attribute.Value;

    protected BindableModuleAttribute(string title, string description, T defaultValue)
        : base(title, description)
    {
        DefaultValue = defaultValue;
    }
}

public class BindableBoolModuleAttribute : BindableModuleAttribute<bool>
{
    protected override BindableBool CreateBindable() => new(DefaultValue);

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

public class BindableIntModuleAttribute : BindableModuleAttribute<int>
{
    protected override BindableNumber<int> CreateBindable() => new(DefaultValue);

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

public class BindableFloatModuleAttribute : BindableModuleAttribute<float>
{
    protected override BindableNumber<float> CreateBindable() => new(DefaultValue);

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

public class BindableStringModuleAttribute : BindableModuleAttribute<string>
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

public class BindableEnumModuleAttribute<TEnum> : BindableModuleAttribute<TEnum> where TEnum : Enum
{
    protected override Bindable<TEnum> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not int intIngestValue) return false;

        Attribute.Value = (TEnum)Enum.ToObject(typeof(TEnum), intIngestValue);
        return true;
    }

    internal override object GetRawValue() => Convert.ToInt32(Attribute.Value);

    internal BindableEnumModuleAttribute(string title, string description, TEnum defaultValue)
        : base(title, description, defaultValue)
    {
    }
}

public class BindableRangedIntModuleAttribute : BindableIntModuleAttribute
{
    private readonly int minValue;
    private readonly int maxValue;

    protected override BindableNumber<int> CreateBindable()
    {
        var baseBindable = base.CreateBindable();
        baseBindable.MinValue = minValue;
        baseBindable.MaxValue = maxValue;
        return baseBindable;
    }

    internal BindableRangedIntModuleAttribute(string title, string description, int defaultValue, int minValue, int maxValue)
        : base(title, description, defaultValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}

public class BindableRangedFloatModuleAttribute : BindableFloatModuleAttribute
{
    private readonly float minValue;
    private readonly float maxValue;

    protected override BindableNumber<float> CreateBindable()
    {
        var baseBindable = base.CreateBindable();
        baseBindable.MinValue = minValue;
        baseBindable.MaxValue = maxValue;
        return baseBindable;
    }

    internal BindableRangedFloatModuleAttribute(string title, string description, float defaultValue, float minValue, float maxValue)
        : base(title, description, defaultValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}

public abstract class BindableListModuleAttribute<T> : ModuleAttribute
{
    public BindableList<T> Attribute = null!;

    protected readonly IEnumerable<T> DefaultValues;

    internal override object GetRawValue() => Attribute.ToList();

    internal override void Load() => Attribute = new BindableList<T>(getClonedDefaults());
    internal override bool IsDefault() => Attribute.SequenceEqual(DefaultValues);
    internal override void SetDefault() => Attribute.ReplaceRange(0, Attribute.Count, getClonedDefaults());

    private IEnumerable<T> getClonedDefaults() => DefaultValues.Select(CloneValue);
    private IEnumerable<T> jArrayToEnumerable(JArray array) => array.Select(ConstructValue);

    protected abstract T CloneValue(T value);
    protected abstract T ConstructValue(JToken token);

    internal override bool Deserialise(object value)
    {
        Attribute.ReplaceRange(0, Attribute.Count, jArrayToEnumerable((JArray)value));
        return true;
    }

    protected BindableListModuleAttribute(string title, string description, IEnumerable<T> defaultValues)
        : base(title, description)
    {
        DefaultValues = defaultValues;
    }
}

public abstract class BindableListBindableModuleAttribute<T> : BindableListModuleAttribute<Bindable<T>>
{
    internal override object GetRawValue() => Attribute.Select(bindable => bindable.Value).ToList();

    protected override Bindable<T> CloneValue(Bindable<T> value) => value.GetUnboundCopy();
    protected override Bindable<T> ConstructValue(JToken token) => new(token.Value<T>()!);

    protected BindableListBindableModuleAttribute(string title, string description, IEnumerable<Bindable<T>> defaultValues)
        : base(title, description, defaultValues)
    {
    }
}

public class BindableListBindableStringModuleAttribute : BindableListBindableModuleAttribute<string>
{
    public BindableListBindableStringModuleAttribute(string title, string description, IEnumerable<string> defaultValues)
        : base(title, description, defaultValues.Select(value => new Bindable<string>(value)))
    {
    }
}

public class BindableListMutableKeyValuePairModuleAttribute : BindableListModuleAttribute<MutableKeyValuePair>
{
    protected override MutableKeyValuePair CloneValue(MutableKeyValuePair value) => new(value);
    protected override MutableKeyValuePair ConstructValue(JToken token) => token.ToObject<MutableKeyValuePair>()!;

    public BindableListMutableKeyValuePairModuleAttribute(string title, string description, IEnumerable<MutableKeyValuePair> defaultValues)
        : base(title, description, defaultValues)
    {
    }
}
