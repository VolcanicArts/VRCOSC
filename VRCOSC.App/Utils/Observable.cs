// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace VRCOSC.App.Utils;

using System;
using System.Collections.Generic;

[JsonConverter(typeof(ObservableConverter))]
public interface ISerialisableObservable
{
    void SerializeTo(JsonWriter writer, JsonSerializer serializer);
    void DeserializeFrom(JsonReader reader, JsonSerializer serializer);
}

public class ObservableConverter : JsonConverter<ISerialisableObservable>
{
    public override void WriteJson(JsonWriter writer, ISerialisableObservable? value, JsonSerializer serializer)
    {
        value?.SerializeTo(writer, serializer);
    }

    public override ISerialisableObservable ReadJson(JsonReader reader, Type objectType, ISerialisableObservable? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var observable = existingValue ?? (ISerialisableObservable)Activator.CreateInstance(objectType, true)!;
        observable.DeserializeFrom(reader, serializer);
        return observable;
    }
}

public sealed class Observable<T> : IObservable<T>, INotifyPropertyChanged, ISerialisableObservable
{
    private T value;

    public T Value
    {
        get => value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(value, this.value)) return;

            this.value = value;
            notifyObservers();
            OnPropertyChanged();
        }
    }

    public T DefaultValue { get; }

    private readonly List<IObserver<T?>> observers = new();
    private readonly List<Action<T?>> actions = new();

    private Observable()
    {
        DefaultValue = default;
    }

    public Observable(T initialValue = default)
    {
        value = initialValue;
        DefaultValue = initialValue;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (!observers.Contains(observer))
            observers.Add(observer);

        observer.OnNext(value);

        return new Unsubscriber(observers, observer);
    }

    public void Subscribe(Action<T> action, bool runOnceImmediately = false)
    {
        actions.Add(action);
        if (runOnceImmediately) action(value);
    }

    public void Unsubscribe(Action<T> action)
    {
        if (actions.Contains(action))
            actions.Remove(action);
    }

    public bool IsDefault => EqualityComparer<T>.Default.Equals(value, DefaultValue);

    public void SetDefault()
    {
        Value = DefaultValue;
    }

    private void notifyObservers()
    {
        foreach (var observer in observers)
        {
            observer.OnNext(value);
        }

        foreach (var action in actions)
        {
            action(value);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<T>> observers;
        private readonly IObserver<T> observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this.observers = observers;
            this.observer = observer;
        }

        public void Dispose()
        {
            if (observers.Contains(observer))
                observers.Remove(observer);
        }
    }

    public void SerializeTo(JsonWriter writer, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public void DeserializeFrom(JsonReader reader, JsonSerializer serializer)
    {
        Value = serializer.Deserialize<T>(reader);
    }
}
