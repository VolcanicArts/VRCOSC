// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Dropdown;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Slider;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Text;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Toggle;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules.Attributes;

public abstract class ModuleAttribute
{
    public required string Name { internal get; init; }
    public required string Description { internal get; init; }
    public Func<bool>? DependsOn { internal get; init; }

    public bool IsValueType<TCheckType>() => GetValueType() == typeof(TCheckType) || GetValueType().IsSubclassOf(typeof(TCheckType));
    public abstract Type GetValueType();

    public abstract object GetSerialisableValue();
    public abstract void Setup();
    public abstract void SetDefault();
    public abstract void DeserialiseValue(object value);
    public abstract bool IsDefault();
    public abstract Drawable GetAssociatedCard();

    public bool Enabled => DependsOn?.Invoke() ?? true;
}

public abstract class ModuleAttribute<T> : ModuleAttribute
{
    public Bindable<T> Attribute = null!;

    public required T Default { internal get; init; }

    public virtual T Value => throw new NotImplementedException();

    protected abstract Bindable<T> GetBindable();

    public override Type GetValueType() => typeof(T);

    public override object GetSerialisableValue() => Attribute.Value!;
    public override void Setup() => Attribute = GetBindable();
    public override void SetDefault() => Attribute.SetDefault();
    public override void DeserialiseValue(object value) => Attribute.Value = (T)value;
    public override bool IsDefault() => Attribute.IsDefault;
}

public class ModuleBoolAttribute : ModuleAttribute<bool>
{
    public override bool Value => Attribute.Value;
    protected override Bindable<bool> GetBindable() => new(Default);
    public override Drawable GetAssociatedCard() => new BoolAttributeCard(this);
}

public class ModuleIntAttribute : ModuleAttribute<int>
{
    public override int Value => Attribute.Value;
    protected override BindableNumber<int> GetBindable() => new(Default);
    public override Drawable GetAssociatedCard() => new IntTextAttributeCard(this);
    public override void DeserialiseValue(object value) => Attribute.Value = Convert.ToInt32(value);
}

public class ModuleFloatAttribute : ModuleAttribute<float>
{
    public override float Value => Attribute.Value;
    protected override BindableNumber<float> GetBindable() => new(Default);
    public override Drawable GetAssociatedCard() => new FloatTextAttributeCard(this);
    public override void DeserialiseValue(object value) => Attribute.Value = Convert.ToSingle(value);
}

public class ModuleStringAttribute : ModuleAttribute<string>
{
    public override string Value => Attribute.Value;
    protected override Bindable<string> GetBindable() => new(Default);
    public override Drawable GetAssociatedCard() => new StringTextAttributeCard(this);
}

public class ModuleStringWithButtonAttribute : ModuleStringAttribute
{
    public required string ButtonText { internal get; init; }
    public required Action ButtonCallback { internal get; init; }
    public override Drawable GetAssociatedCard() => new StringTextWithButtonAttributeCard(this);
}

public class ModuleEnumAttribute<T> : ModuleAttribute<T> where T : Enum
{
    public override T Value => Attribute.Value;
    protected override Bindable<T> GetBindable() => new(Default);
    public override Drawable GetAssociatedCard() => new DropdownAttributeCard<T>(this);
    public override void DeserialiseValue(object value) => Attribute.Value = (T)Enum.ToObject(typeof(T), Convert.ToInt32(value));
}

public class ModuleIntRangeAttribute : ModuleIntAttribute
{
    public required int Min { internal get; init; }
    public required int Max { internal get; init; }

    protected override BindableNumber<int> GetBindable()
    {
        var baseBindable = base.GetBindable();
        baseBindable.MinValue = Min;
        baseBindable.MaxValue = Max;
        return baseBindable;
    }

    public override Drawable GetAssociatedCard() => new IntSliderAttributeCard(this);
}

public class ModuleFloatRangeAttribute : ModuleFloatAttribute
{
    public required float Min { internal get; init; }
    public required float Max { internal get; init; }

    protected override BindableNumber<float> GetBindable()
    {
        var baseBindable = base.GetBindable();
        baseBindable.MinValue = Min;
        baseBindable.MaxValue = Max;
        return baseBindable;
    }

    public override Drawable GetAssociatedCard() => new FloatSliderAttributeCard(this);
}

public abstract class ModuleAttributeList<T> : ModuleAttribute
{
    public BindableList<T> Attribute = null!;
    public required List<T> Default { get; init; }

    public override Type GetValueType() => typeof(T);
    public override object GetSerialisableValue() => Attribute.ToList();
    public override void Setup() => Attribute = GetBindable();

    protected abstract BindableList<T> GetBindable();
    protected abstract IEnumerable<T> GetClonedDefaults();
    protected abstract IEnumerable<T> JArrayToType(JArray array);

    public override void SetDefault()
    {
        Attribute.Clear();
        Attribute.AddRange(GetClonedDefaults());
    }

    public override void DeserialiseValue(object value)
    {
        Attribute.Clear();
        Attribute.AddRange(JArrayToType((JArray)value));
    }
}

public abstract class ModuleAttributePrimitiveList<T> : ModuleAttributeList<Bindable<T>>
{
    public override Type GetValueType() => typeof(T);
    public override object GetSerialisableValue() => Attribute.ToList();
    public override void Setup() => Attribute = GetBindable();
    protected override IEnumerable<Bindable<T>> JArrayToType(JArray array) => array.Select(value => new Bindable<T>(value.Value<T>()!)).ToList();
}

public class ModuleStringListAttribute : ModuleAttributePrimitiveList<string>
{
    public override Drawable GetAssociatedCard() => new ListStringTextAttributeCard(this);
    public override bool IsDefault() => Attribute.Count == Default.Count && !Attribute.Where((t, i) => !t.Value.Equals(Default.ElementAt(i).Value)).Any();

    protected override BindableList<Bindable<string>> GetBindable() => new(Default);
    protected override IEnumerable<Bindable<string>> GetClonedDefaults() => Default.Select(defaultValue => defaultValue.GetUnboundCopy()).ToList();
}

public class ModuleParameter : ModuleStringAttribute
{
    public required ParameterMode Mode { internal get; init; }
    public required Type ExpectedType = null!;

    public string ParameterName => Attribute.Value;
    public string FormattedAddress => $"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/{ParameterName}";
}
