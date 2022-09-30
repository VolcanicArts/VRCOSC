// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Modules;

public abstract class ModuleAttribute
{
    public readonly ModuleAttributeMetadata Metadata;

    protected ModuleAttribute(ModuleAttributeMetadata metadata)
    {
        Metadata = metadata;
    }

    public abstract void SetDefault();

    public abstract bool IsDefault();
}

public class ModuleAttributeSingle : ModuleAttribute
{
    public readonly Bindable<object> Attribute;

    public ModuleAttributeSingle(ModuleAttributeMetadata metadata, object defaultValue)
        : base(metadata)
    {
        Attribute = new Bindable<object>(defaultValue);
    }

    public override void SetDefault()
    {
        Attribute.SetDefault();
    }

    public override bool IsDefault()
    {
        return Attribute.IsDefault;
    }
}

public sealed class ModuleAttributeSingleWithButton : ModuleAttributeSingle
{
    public readonly Action ButtonAction;
    public readonly string ButtonText;

    public ModuleAttributeSingleWithButton(ModuleAttributeMetadata metadata, object defaultValue, string buttonText, Action buttonAction)
        : base(metadata, defaultValue)
    {
        ButtonText = buttonText;
        ButtonAction = buttonAction;
    }
}

public sealed class ModuleAttributeSingleWithBounds : ModuleAttributeSingle
{
    public readonly object MinValue;
    public readonly object MaxValue;

    public ModuleAttributeSingleWithBounds(ModuleAttributeMetadata metadata, object defaultValue, object minValue, object maxValue)
        : base(metadata, defaultValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}

public sealed class ModuleAttributeList : ModuleAttribute
{
    public readonly BindableList<Bindable<object>> AttributeList;
    private readonly IEnumerable<object> defaultValues;
    public readonly Type Type;

    public ModuleAttributeList(ModuleAttributeMetadata metadata, IEnumerable<object> defaultValues, Type type)
        : base(metadata)
    {
        AttributeList = new BindableList<Bindable<object>>();
        this.defaultValues = defaultValues;
        Type = type;

        SetDefault();
    }

    public override void SetDefault()
    {
        AttributeList.Clear();
        var newValues = new List<Bindable<object>>();
        defaultValues.ForEach(value => newValues.Add(new Bindable<object>(value)));
        AttributeList.AddRange(newValues);
    }

    public override bool IsDefault()
    {
        return AttributeList.Count == defaultValues.Count() && !AttributeList.Where((t, i) => !t.Value.Equals(defaultValues.ElementAt(i))).Any();
    }

    public void AddAt(int index, Bindable<object> value)
    {
        try
        {
            AttributeList[index] = value;
        }
        catch (ArgumentOutOfRangeException)
        {
            AttributeList.Insert(index, value);
        }
    }

    public IEnumerable<T> GetValueList<T>()
    {
        List<T> list = new();
        AttributeList.ForEach(attribute => list.Add((T)attribute.Value));
        return list;
    }
}

public sealed class ModuleAttributeMetadata
{
    public readonly string DisplayName;
    public readonly string Description;

    public ModuleAttributeMetadata(string displayName, string description)
    {
        DisplayName = displayName;
        Description = description;
    }
}
