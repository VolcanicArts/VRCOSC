// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public abstract class ListModuleSetting : ModuleSetting
{
    protected ListModuleSetting(string title, string description, Type viewType)
        : base(title, description, viewType)
    {
    }

    public abstract int Count();
    public abstract void Add();
    public abstract void Remove(object instance);
}

public abstract class ListModuleSetting<T> : ListModuleSetting where T : ICloneable, IEquatable<T>, new()
{
    public ObservableCollection<T> Attribute { get; private set; } = null!;
    protected IEnumerable<T> DefaultValues { get; }

    public override object GetRawValue() => Attribute.ToList();
    public override bool IsDefault() => Attribute.SequenceEqual(DefaultValues);

    public override void PreDeserialise()
    {
        Attribute = new ObservableCollection<T>(getClonedDefaults());
        Attribute.CollectionChanged += (_, _) => OnSettingChange?.Invoke();
    }

    public override void SetDefault()
    {
        Attribute.Clear();
        getClonedDefaults().ForEach(item => Attribute.Add(item));
    }

    private IEnumerable<T> getClonedDefaults() => DefaultValues.Select(value => (T)value.Clone());

    protected virtual T CreateItem() => new();

    public override bool Deserialise(object? value)
    {
        if (value is not JArray jArrayValue) return false;

        Attribute.Clear();
        Attribute.AddRange(jArrayValue.Select(token => token.ToObject<T>()!));
        return true;
    }

    protected ListModuleSetting(string title, string description, Type viewType, IEnumerable<T> defaultValues)
        : base(title, description, viewType)
    {
        DefaultValues = defaultValues;
    }

    public override int Count() => Attribute.Count;

    public override void Add()
    {
        Attribute.Add(CreateItem());
    }

    public override void Remove(object instance)
    {
        if (instance is not T castInstance) throw new InvalidOperationException($"Cannot remove type {instance.GetType()} from list with type {typeof(T)}");

        Attribute.Remove(castInstance);
    }
}

public abstract class ValueListModuleSetting<T> : ListModuleSetting<Observable<T>>
{
    public override object GetRawValue() => Attribute.Select(observable => observable.Value).ToList();

    public override void PreDeserialise()
    {
        base.PreDeserialise();

        Attribute.CollectionChanged += (_, e) => subscribeToNewItems(e);
        return;

        void subscribeToNewItems(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null) return;

            foreach (Observable<T> newItem in e.NewItems)
            {
                newItem.Subscribe(_ => OnSettingChange?.Invoke());
            }
        }
    }

    protected ValueListModuleSetting(string title, string description, Type viewType, IEnumerable<Observable<T>> defaultValues)
        : base(title, description, viewType, defaultValues)
    {
    }
}

public class StringListModuleSetting : ValueListModuleSetting<string>
{
    public StringListModuleSetting(string title, string description, Type viewType, IEnumerable<string> defaultValues)
        : base(title, description, viewType, defaultValues.Select(value => new Observable<string>(value)))
    {
    }

    protected override Observable<string> CreateItem() => new(string.Empty);
}

public class IntListModuleSetting : ValueListModuleSetting<int>
{
    public IntListModuleSetting(string title, string description, Type viewType, IEnumerable<int> defaultValues)
        : base(title, description, viewType, defaultValues.Select(value => new Observable<int>(value)))
    {
    }
}

public class FloatListModuleSetting : ValueListModuleSetting<float>
{
    public FloatListModuleSetting(string title, string description, Type viewType, IEnumerable<float> defaultValues)
        : base(title, description, viewType, defaultValues.Select(value => new Observable<float>(value)))
    {
    }
}
