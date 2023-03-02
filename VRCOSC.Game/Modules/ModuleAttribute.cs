// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules;

public abstract class ModuleAttribute
{
    public readonly ModuleAttributeMetadata Metadata;
    private readonly Func<bool>? dependsOn;

    protected ModuleAttribute(ModuleAttributeMetadata metadata, Func<bool>? dependsOn)
    {
        Metadata = metadata;
        this.dependsOn = dependsOn;
    }

    public bool Enabled => dependsOn?.Invoke() ?? true;

    public abstract void SetDefault();

    public abstract bool IsDefault();
}

public class ModuleAttributeSingle : ModuleAttribute
{
    public readonly Bindable<object> Attribute;

    public ModuleAttributeSingle(ModuleAttributeMetadata metadata, object defaultValue, Func<bool>? dependsOn)
        : base(metadata, dependsOn)
    {
        Attribute = new Bindable<object>(defaultValue);
    }

    public override void SetDefault() => Attribute.SetDefault();

    public override bool IsDefault() => Attribute.IsDefault;
}

public sealed class ParameterAttribute : ModuleAttributeSingle
{
    public readonly ParameterMode Mode;
    public readonly Type ExpectedType;
    public readonly IEnumerable<string>? Extensions;

    public string RootName => (string)Attribute.Value;
    public string FormattedAddress => $"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/{RootName}";

    public ParameterAttribute(ParameterMode mode, IEnumerable<string>? extensions, ModuleAttributeMetadata metadata, string defaultName, Type expectedType, Func<bool>? dependsOn)
        : base(metadata, defaultName, dependsOn)
    {
        Mode = mode;
        ExpectedType = expectedType;
        Extensions = extensions;
    }
}

public sealed class ModuleAttributeSingleWithButton : ModuleAttributeSingle
{
    public readonly Action ButtonAction;
    public readonly string ButtonText;

    public ModuleAttributeSingleWithButton(ModuleAttributeMetadata metadata, object defaultValue, string buttonText, Action buttonAction, Func<bool>? dependsOn)
        : base(metadata, defaultValue, dependsOn)
    {
        ButtonText = buttonText;
        ButtonAction = buttonAction;
    }
}

public sealed class ModuleAttributeSingleWithBounds : ModuleAttributeSingle
{
    public readonly object MinValue;
    public readonly object MaxValue;

    public ModuleAttributeSingleWithBounds(ModuleAttributeMetadata metadata, object defaultValue, object minValue, object maxValue, Func<bool>? dependsOn)
        : base(metadata, defaultValue, dependsOn)
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
    public readonly bool CanBeEmpty;

    public ModuleAttributeList(ModuleAttributeMetadata metadata, IEnumerable<object> defaultValues, Type type, bool canBeEmpty, Func<bool>? dependsOn)
        : base(metadata, dependsOn)
    {
        AttributeList = new BindableList<Bindable<object>>();
        this.defaultValues = defaultValues;
        Type = type;
        CanBeEmpty = canBeEmpty;

        SetDefault();
    }

    public override void SetDefault()
    {
        AttributeList.Clear();
        var newValues = new List<Bindable<object>>();
        defaultValues.ForEach(value => newValues.Add(new Bindable<object>(value)));
        AttributeList.AddRange(newValues);
    }

    public override bool IsDefault() => AttributeList.Count == defaultValues.Count() && !AttributeList.Where((t, i) => !t.Value.Equals(defaultValues.ElementAt(i))).Any();

    public List<T> GetValueList<T>() => AttributeList.Select(attribute => (T)attribute.Value).ToList();

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
