// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Modules.SDK.Attributes;

public abstract class ModuleAttribute
{
    /// <summary>
    /// The enabled value of this <see cref="ModuleAttribute"/>
    /// </summary>
    public Bindable<bool> Enabled = new(true);

    /// <summary>
    /// The title of this <see cref="ModuleAttribute"/>
    /// </summary>
    internal readonly string Title;

    /// <summary>
    /// The description of this <see cref="ModuleAttribute"/>
    /// </summary>
    internal readonly string Description;

    private readonly Type drawableModuleAttributeType;

    /// <summary>
    /// The GUI component associated with this <see cref="ModuleAttribute"/>
    /// </summary>
    internal Container GetDrawableModuleAttribute() => (Container)Activator.CreateInstance(drawableModuleAttributeType, this)!;

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

    protected ModuleAttribute(string title, string description, Type drawableModuleAttributeType)
    {
        Title = title;
        Description = description;
        this.drawableModuleAttributeType = drawableModuleAttributeType;
    }
}

public abstract class ValueModuleAttribute<T> : ModuleAttribute
{
    public Bindable<T> Attribute { get; private set; } = null!;

    protected readonly T DefaultValue;

    protected abstract Bindable<T> CreateBindable();

    internal override void Load() => Attribute = CreateBindable();
    internal override bool IsDefault() => Attribute.IsDefault;
    internal override void SetDefault() => Attribute.SetDefault();

    internal override object? GetRawValue() => Attribute.Value;

    protected ValueModuleAttribute(string title, string description, Type drawableModuleAttributeType, T defaultValue)
        : base(title, description, drawableModuleAttributeType)
    {
        DefaultValue = defaultValue;
    }
}

public class BoolModuleAttribute : ValueModuleAttribute<bool>
{
    protected override BindableBool CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not bool boolIngestValue) return false;

        Attribute.Value = boolIngestValue;
        return true;
    }

    public BoolModuleAttribute(string title, string description, Type drawableModuleAttributeType, bool defaultValue)
        : base(title, description, drawableModuleAttributeType, defaultValue)
    {
    }
}

public class IntModuleAttribute : ValueModuleAttribute<int>
{
    protected override BindableNumber<int> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not int intIngestValue) return false;

        Attribute.Value = intIngestValue;
        return true;
    }

    internal IntModuleAttribute(string title, string description, Type drawableModuleAttributeType, int defaultValue)
        : base(title, description, drawableModuleAttributeType, defaultValue)
    {
    }
}

public class FloatModuleAttribute : ValueModuleAttribute<float>
{
    protected override BindableNumber<float> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not float floatIngestValue) return false;

        Attribute.Value = floatIngestValue;
        return true;
    }

    internal FloatModuleAttribute(string title, string description, Type drawableModuleAttributeType, float defaultValue)
        : base(title, description, drawableModuleAttributeType, defaultValue)
    {
    }
}

public class StringModuleAttribute : ValueModuleAttribute<string>
{
    protected override Bindable<string> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not string stringIngestValue) return false;

        Attribute.Value = stringIngestValue;
        return true;
    }

    internal StringModuleAttribute(string title, string description, Type drawableModuleAttributeType, string defaultValue)
        : base(title, description, drawableModuleAttributeType, defaultValue)
    {
    }
}

public class EnumModuleAttribute<TEnum> : ValueModuleAttribute<TEnum> where TEnum : Enum
{
    protected override Bindable<TEnum> CreateBindable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not int intIngestValue) return false;

        Attribute.Value = (TEnum)Enum.ToObject(typeof(TEnum), intIngestValue);
        return true;
    }

    internal override object GetRawValue() => Convert.ToInt32(Attribute.Value);

    internal EnumModuleAttribute(string title, string description, Type drawableModuleAttributeType, TEnum defaultValue)
        : base(title, description, drawableModuleAttributeType, defaultValue)
    {
    }
}

public class RangedIntModuleAttribute : IntModuleAttribute
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

    internal RangedIntModuleAttribute(string title, string description, Type drawableModuleAttributeType, int defaultValue, int minValue, int maxValue)
        : base(title, description, drawableModuleAttributeType, defaultValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}

public class RangedFloatModuleAttribute : FloatModuleAttribute
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

    internal RangedFloatModuleAttribute(string title, string description, Type drawableModuleAttributeType, float defaultValue, float minValue, float maxValue)
        : base(title, description, drawableModuleAttributeType, defaultValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}

public abstract class ListModuleAttribute<T> : ModuleAttribute
{
    public BindableList<T> Attribute = null!;

    private readonly IEnumerable<T> defaultValues;

    internal override object GetRawValue() => Attribute.ToList();

    internal override void Load() => Attribute = new BindableList<T>(getClonedDefaults());
    internal override bool IsDefault() => Attribute.SequenceEqual(defaultValues);
    internal override void SetDefault() => Attribute.ReplaceRange(0, Attribute.Count, getClonedDefaults());

    private IEnumerable<T> getClonedDefaults() => defaultValues.Select(CloneValue);
    private IEnumerable<T> jArrayToEnumerable(JArray array) => array.Select(ConstructValue);

    protected abstract T CloneValue(T value);
    protected abstract T ConstructValue(JToken token);

    internal override bool Deserialise(object value)
    {
        Attribute.ReplaceRange(0, Attribute.Count, jArrayToEnumerable((JArray)value));
        return true;
    }

    protected ListModuleAttribute(string title, string description, Type drawableModuleAttributeType, IEnumerable<T> defaultValues)
        : base(title, description, drawableModuleAttributeType)
    {
        this.defaultValues = defaultValues;
    }
}

public abstract class ListBindableModuleAttribute<T> : ListModuleAttribute<Bindable<T>>
{
    internal override object GetRawValue() => Attribute.Select(bindable => bindable.Value).ToList();

    protected override Bindable<T> CloneValue(Bindable<T> value) => value.GetUnboundCopy();
    protected override Bindable<T> ConstructValue(JToken token) => new(token.Value<T>()!);

    protected ListBindableModuleAttribute(string title, string description, Type drawableModuleAttributeType, IEnumerable<Bindable<T>> defaultValues)
        : base(title, description, drawableModuleAttributeType, defaultValues)
    {
    }
}

public class ListStringModuleAttribute : ListBindableModuleAttribute<string>
{
    public ListStringModuleAttribute(string title, string description, Type drawableModuleAttributeType, IEnumerable<string> defaultValues)
        : base(title, description, drawableModuleAttributeType, defaultValues.Select(value => new Bindable<string>(value)))
    {
    }
}

public class ListMutableKeyValuePairModuleAttribute : ListModuleAttribute<MutableKeyValuePair>
{
    protected override MutableKeyValuePair CloneValue(MutableKeyValuePair value) => new(value);
    protected override MutableKeyValuePair ConstructValue(JToken token) => token.ToObject<MutableKeyValuePair>()!;

    public ListMutableKeyValuePairModuleAttribute(string title, string description, Type drawableModuleAttributeType, IEnumerable<MutableKeyValuePair> defaultValues)
        : base(title, description, drawableModuleAttributeType, defaultValues)
    {
    }
}
