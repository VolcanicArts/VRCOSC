// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace VRCOSC.App.Utils;

[JsonConverter(typeof(ObservableConverter))]
public interface IObservable
{
    public Type GetValueType();
    public object GetValue();
    public void SetValue(object value);
    public void Subscribe(Action noValueAction, bool runOnceImmediately = false);

    internal void SerializeTo(JsonWriter writer, JsonSerializer serializer);
    internal void DeserializeFrom(JsonReader reader, JsonSerializer serializer);
}

public class ObservableConverter : JsonConverter<IObservable>
{
    public override void WriteJson(JsonWriter writer, IObservable? value, JsonSerializer serializer)
    {
        value?.SerializeTo(writer, serializer);
    }

    public override IObservable ReadJson(JsonReader reader, Type objectType, IObservable? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var observable = existingValue ?? (IObservable)Activator.CreateInstance(objectType, true)!;
        observable.DeserializeFrom(reader, serializer);
        return observable;
    }
}

public sealed class Observable<T> : IObservable, INotifyPropertyChanged, IEquatable<Observable<T>> where T : notnull
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

    private readonly object notifyLock = new();
    private readonly List<Action> noValueActions = new();
    private readonly List<Action<T>> actions = new();

    [JsonConstructor]
    private Observable()
    {
    }

    public Observable(T defaultValue = default!)
    {
        DefaultValue = defaultValue;
        Value = defaultValue;
    }

    public Observable(Observable<T> other)
    {
        DefaultValue = other.DefaultValue;
        Value = other.Value;
    }

    public Type GetValueType() => typeof(T);

    public object GetValue() => Value;

    public void SetValue(object newValue)
    {
        if (newValue is not T castValue) throw new InvalidOperationException($"Attempted to set anonymous value of type {newValue.GetType().ToReadableName()} for type {typeof(T).ToReadableName()}");

        Value = castValue;
    }

    public void Subscribe(Action noValueAction, bool runOnceImmediately = false)
    {
        lock (notifyLock)
        {
            noValueActions.Add(noValueAction);
        }

        if (runOnceImmediately) noValueAction.Invoke();
    }

    public void Subscribe(Action<T> action, bool runOnceImmediately = false)
    {
        lock (notifyLock)
        {
            actions.Add(action);
        }

        if (runOnceImmediately) action.Invoke(Value);
    }

    public void Unsubscribe(Action noValueAction)
    {
        lock (notifyLock)
        {
            noValueActions.Remove(noValueAction);
        }
    }

    public void Unsubscribe(Action<T> action)
    {
        lock (notifyLock)
        {
            actions.Remove(action);
        }
    }

    public bool IsDefault => EqualityComparer<T>.Default.Equals(Value, DefaultValue);

    public void SetDefault()
    {
        Value = DefaultValue;
    }

    private void notifyObservers()
    {
        lock (notifyLock)
        {
            foreach (var noValueAction in noValueActions)
            {
                noValueAction.Invoke();
            }

            foreach (var action in actions)
            {
                action.Invoke(Value);
            }
        }
    }

    public void SerializeTo(JsonWriter writer, JsonSerializer serializer)
    {
        try
        {
            serializer.Serialize(writer, Value);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Observable could not serialise value");
        }
    }

    public void DeserializeFrom(JsonReader reader, JsonSerializer serializer)
    {
        try
        {
            Value = serializer.Deserialize<T>(reader)!;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Observable could not deserialise value");
        }
    }

    public bool Equals(Observable<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}