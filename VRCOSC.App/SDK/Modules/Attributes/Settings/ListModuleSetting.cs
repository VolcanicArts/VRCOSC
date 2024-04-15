// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Newtonsoft.Json.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public abstract class ListModuleSetting<T> : ModuleSetting
{
    #region UI

    public Visibility RowNumberVisibility => rowNumberVisible ? Visibility.Visible : Visibility.Collapsed;

    #endregion

    private readonly bool rowNumberVisible;

    public ObservableCollection<T> Attribute { get; private set; } = null!;
    protected readonly IEnumerable<T> DefaultValues;

    public override object GetRawValue() => Attribute.ToList();

    public override void Load()
    {
        Attribute = new ObservableCollection<T>(getClonedDefaults());
        Attribute.CollectionChanged += (_, _) => RequestSerialisation?.Invoke();
    }

    public override bool IsDefault() => Attribute.SequenceEqual(DefaultValues);

    public override void SetDefault()
    {
        Attribute.Clear();
        getClonedDefaults().ForEach(item => Attribute.Add(item));
    }

    private IEnumerable<T> getClonedDefaults() => DefaultValues.Select(CloneValue);
    private IEnumerable<T> jArrayToEnumerable(JArray array) => array.Select(ConstructValue);

    protected abstract T CloneValue(T value);
    protected abstract T ConstructValue(JToken token);

    internal void AddItem() => Attribute.Add(CreateNewItem());
    protected abstract T CreateNewItem();

    public override bool Deserialise(object value)
    {
        Attribute.Clear();
        jArrayToEnumerable((JArray)value).ForEach(item => Attribute.Add(item));
        return true;
    }

    protected ListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<T> defaultValues, bool rowNumberVisible)
        : base(metadata)
    {
        DefaultValues = defaultValues;
        this.rowNumberVisible = rowNumberVisible;
    }
}

public abstract class ValueListModuleSetting<T> : ListModuleSetting<Observable<T>>
{
    public override object GetRawValue() => Attribute.Select(observable => observable.Value).ToList();
    public override bool IsDefault() => Attribute.Select(observable => observable.Value).SequenceEqual(DefaultValues.Select(defaultObservable => defaultObservable.Value));

    public override void Load()
    {
        base.Load();

        Attribute.CollectionChanged += (_, e) => subscribeToNewItems(e);
        return;

        void subscribeToNewItems(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null) return;

            foreach (Observable<T> newItem in e.NewItems)
            {
                newItem.Subscribe(_ => RequestSerialisation?.Invoke());
            }
        }
    }

    protected override Observable<T> CloneValue(Observable<T> value) => new(value.Value!);
    protected override Observable<T> ConstructValue(JToken token) => new(token.Value<T>()!);

    protected ValueListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<Observable<T>> defaultValues, bool rowNumberVisible)
        : base(metadata, defaultValues, rowNumberVisible)
    {
    }
}

public class StringListModuleSetting : ValueListModuleSetting<string>
{
    public StringListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<string> defaultValues, bool rowNumberVisible)
        : base(metadata, defaultValues.Select(value => new Observable<string>(value)), rowNumberVisible)
    {
    }

    protected override Observable<string> CreateNewItem() => new(string.Empty);
}

public class IntListModuleSetting : ValueListModuleSetting<int>
{
    public IntListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<int> defaultValues, bool rowNumberVisible)
        : base(metadata, defaultValues.Select(value => new Observable<int>(value)), rowNumberVisible)
    {
    }

    protected override Observable<int> CreateNewItem() => new();
}

public class FloatListModuleSetting : ValueListModuleSetting<float>
{
    public FloatListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<float> defaultValues, bool rowNumberVisible)
        : base(metadata, defaultValues.Select(value => new Observable<float>(value)), rowNumberVisible)
    {
    }

    protected override Observable<float> CreateNewItem() => new();
}
