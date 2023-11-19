// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Modules.SDK.Attributes.Settings;

public abstract class ListModuleSetting<T> : ModuleSetting
{
    /// <summary>
    /// The metadata for this <see cref="ModuleSetting"/>
    /// </summary>
    internal new ListModuleSettingMetadata Metadata => (ListModuleSettingMetadata)base.Metadata;

    /// <summary>
    /// The UI component associated with this <see cref="ListModuleSetting{T}"/>'s item.
    /// This creates a new instance each time this is called to allow for proper disposal of UI components
    /// </summary>
    internal Container GetItemDrawable(T item) => (Container)Activator.CreateInstance(Metadata.DrawableListModuleSettingItemType, item)!;

    public BindableList<T> Attribute = null!;
    protected readonly IEnumerable<T> DefaultValues;

    internal override object GetRawValue() => Attribute.ToList();

    internal override void Load()
    {
        Attribute = new BindableList<T>(getClonedDefaults());
        Attribute.BindCollectionChanged((_, _) => RequestSerialisation?.Invoke());
    }

    internal override bool IsDefault() => Attribute.SequenceEqual(DefaultValues);
    internal override void SetDefault() => Attribute.ReplaceRange(0, Attribute.Count, getClonedDefaults());

    private IEnumerable<T> getClonedDefaults() => DefaultValues.Select(CloneValue);
    private IEnumerable<T> jArrayToEnumerable(JArray array) => array.Select(ConstructValue);

    protected abstract T CloneValue(T value);
    protected abstract T ConstructValue(JToken token);

    internal void AddItem() => Attribute.Add(CreateNewItem());
    protected abstract T CreateNewItem();

    internal override bool Deserialise(object value)
    {
        Attribute.ReplaceRange(0, Attribute.Count, jArrayToEnumerable((JArray)value));
        return true;
    }

    protected ListModuleSetting(ListModuleSettingMetadata metadata, IEnumerable<T> defaultValues)
        : base(metadata)
    {
        this.DefaultValues = defaultValues;
    }
}

public abstract class ValueListModuleSetting<T> : ListModuleSetting<Bindable<T>>
{
    internal override object GetRawValue() => Attribute.Select(bindable => bindable.Value).ToList();
    internal override bool IsDefault() => Attribute.Select(bindable => bindable.Value).SequenceEqual(DefaultValues.Select(defaultBindable => defaultBindable.Value));

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

    protected ValueListModuleSetting(ListModuleSettingMetadata metadata, IEnumerable<Bindable<T>> defaultValues)
        : base(metadata, defaultValues)
    {
    }
}

public class StringListModuleSetting : ValueListModuleSetting<string>
{
    public StringListModuleSetting(ListModuleSettingMetadata metadata, IEnumerable<string> defaultValues)
        : base(metadata, defaultValues.Select(value => new Bindable<string>(value)))
    {
    }

    protected override Bindable<string> CreateNewItem() => new(string.Empty);
}
