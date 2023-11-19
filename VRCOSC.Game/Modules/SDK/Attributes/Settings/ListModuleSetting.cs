// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;

namespace VRCOSC.Game.Modules.SDK.Attributes.Settings;

public abstract class ListModuleSetting<T> : ModuleSetting
{
    public BindableList<T> Attribute = null!;

    private readonly IEnumerable<T> defaultValues;

    internal override object GetRawValue() => Attribute.ToList();

    internal override void Load()
    {
        Attribute = new BindableList<T>(getClonedDefaults());
        Attribute.BindCollectionChanged((_, _) => RequestSerialisation?.Invoke());
    }

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

    protected ListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<T> defaultValues)
        : base(metadata)
    {
        this.defaultValues = defaultValues;
    }
}

public abstract class ListValueModuleSetting<T> : ListModuleSetting<Bindable<T>>
{
    internal override object GetRawValue() => Attribute.Select(bindable => bindable.Value).ToList();

    internal override void Load()
    {
        base.Load();

        Attribute.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is null) return;

            foreach (Bindable<T> newItem in e.NewItems)
            {
                newItem.BindValueChanged(_ => RequestSerialisation?.Invoke());
            }
        }, true);
    }

    protected override Bindable<T> CloneValue(Bindable<T> value) => value.GetUnboundCopy();
    protected override Bindable<T> ConstructValue(JToken token) => new(token.Value<T>()!);

    protected ListValueModuleSetting(ModuleSettingMetadata metadata, IEnumerable<Bindable<T>> defaultValues)
        : base(metadata, defaultValues)
    {
    }
}

public class ListStringModuleSetting : ListValueModuleSetting<string>
{
    public ListStringModuleSetting(ModuleSettingMetadata metadata, IEnumerable<string> defaultValues)
        : base(metadata, defaultValues.Select(value => new Bindable<string>(value)))
    {
    }
}
