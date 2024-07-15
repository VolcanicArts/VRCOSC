// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace VRCOSC.App.Utils;

public class ObservableKeyValuePair<TKey, TValue> : INotifyPropertyChanged
{
    #region properties

    private TKey key;
    private TValue value;

    public TKey Key
    {
        get => key;
        set
        {
            key = value;
            OnPropertyChanged("Key");
        }
    }

    public TValue Value
    {
        get => value;
        set
        {
            this.value = value;
            OnPropertyChanged("Value");
        }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string name)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion
}

public class ObservableDictionary<TKey, TValue> : ObservableCollection<ObservableKeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
{
    #region IDictionary<TKey,TValue> Members

    public void Add(TKey key, TValue value)
    {
        if (ContainsKey(key))
        {
            throw new ArgumentException("The dictionary already contains the key");
        }

        base.Add(new ObservableKeyValuePair<TKey, TValue> { Key = key, Value = value });
    }

    public bool ContainsKey(TKey key)
    {
        var r = thisAsCollection().FirstOrDefault(i => Equals(key, i.Key));
        return !Equals(default, r);
    }

    public bool Equals(TKey a, TKey b) => EqualityComparer<TKey>.Default.Equals(a, b);

    private ObservableCollection<ObservableKeyValuePair<TKey, TValue>> thisAsCollection() => this;

    public ICollection<TKey> Keys => (from i in thisAsCollection() select i.Key).ToList();

    public bool Remove(TKey key)
    {
        var remove = thisAsCollection().Where(pair => Equals(key, pair.Key)).ToList();

        foreach (var pair in remove)
        {
            thisAsCollection().Remove(pair);
        }

        return remove.Count > 0;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        value = default!;
        var r = getKvpByTheKey(key);

        if (r is null)
            return false;

        value = r.Value;
        return true;
    }

    private ObservableKeyValuePair<TKey, TValue>? getKvpByTheKey(TKey key)
    {
        return thisAsCollection().FirstOrDefault(i => i.Key!.Equals(key));
    }

    public ICollection<TValue> Values => (from i in thisAsCollection() select i.Value).ToList();

    public TValue this[TKey key]
    {
        get
        {
            if (!TryGetValue(key, out var result))
            {
                throw new ArgumentException("Key not found");
            }

            return result;
        }
        set
        {
            if (ContainsKey(key))
            {
                getKvpByTheKey(key)!.Value = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        var r = getKvpByTheKey(item.Key);
        return r is not null && Equals(r.Value, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool IsReadOnly => false;

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var r = getKvpByTheKey(item.Key);

        if (r is null)
            return false;

        return Equals(r.Value, item.Value) && thisAsCollection().Remove(r);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => (from i in thisAsCollection() select new KeyValuePair<TKey, TValue>(i.Key, i.Value)).ToList().GetEnumerator();

    #endregion
}
