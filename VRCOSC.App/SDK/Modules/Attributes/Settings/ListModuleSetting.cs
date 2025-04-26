// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public abstract IEnumerable GetEnumerable();
    public abstract void Add();
    public abstract void Remove(object item);
}

public abstract class ListModuleSetting<T> : ListModuleSetting where T : IEquatable<T>
{
    protected readonly IEnumerable<T> DefaultValues;
    public ObservableCollection<T> Attribute { get; }

    protected ListModuleSetting(string title, string description, Type viewType, IEnumerable<T> defaultValues)
        : base(title, description, viewType)
    {
        DefaultValues = defaultValues;
        Attribute = new ObservableCollection<T>(defaultValues);
        Attribute.OnCollectionChanged((_, _) => OnSettingChange?.Invoke());
    }

    protected override bool IsDefault() => Attribute.SequenceEqual(DefaultValues);

    public override IEnumerable GetEnumerable() => Attribute;

    public override void Add() => Attribute.Add(CreateItem());

    public override void Remove(object item)
    {
        if (item is not T castItem) throw new InvalidOperationException($"Cannot remove type {item.GetType().GetFriendlyName()} from list with type {typeof(T).GetFriendlyName()}");

        Attribute.Remove(castItem);
    }

    protected abstract T CreateItem();

    public override TOut GetValue<TOut>()
    {
        if (Attribute.ToList() is not TOut castList) throw new InvalidCastException($"{typeof(List<T>).Name} cannot be cast to {typeof(TOut).Name}");

        return castList;
    }

    protected override object Serialise() => Attribute.ToList();

    protected override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not JArray jArrayValue) return false;

        Attribute.Clear();
        Attribute.AddRange(jArrayValue.Select(token => token.ToObject<T>()!));
        return true;
    }
}

public abstract class ValueListModuleSetting<T> : ListModuleSetting<Observable<T>> where T : notnull
{
    protected ValueListModuleSetting(string title, string description, Type viewType, IEnumerable<T> defaultValues)
        : base(title, description, viewType, defaultValues.Select(value => new Observable<T>(value)))
    {
    }

    public override TOut GetValue<TOut>()
    {
        if (Attribute.Select(o => o.Value).ToList() is not TOut castList) throw new InvalidCastException($"{typeof(List<T>).Name} cannot be cast to {typeof(TOut).Name}");

        return castList;
    }

    protected override bool IsDefault() => base.IsDefault() && Attribute.All(o => o.IsDefault);

    protected override Observable<T> CreateItem() => new();
}

public class StringListModuleSetting : ValueListModuleSetting<string>
{
    public StringListModuleSetting(string title, string description, Type viewType, IEnumerable<string> defaultValues)
        : base(title, description, viewType, defaultValues)
    {
    }

    protected override Observable<string> CreateItem() => new(string.Empty);
}

public class IntListModuleSetting : ValueListModuleSetting<int>
{
    public IntListModuleSetting(string title, string description, Type viewType, IEnumerable<int> defaultValues)
        : base(title, description, viewType, defaultValues)
    {
    }
}

public class FloatListModuleSetting : ValueListModuleSetting<float>
{
    public FloatListModuleSetting(string title, string description, Type viewType, IEnumerable<float> defaultValues)
        : base(title, description, viewType, defaultValues)
    {
    }
}