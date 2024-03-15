// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VRCOSC.App.Utils;

using System;
using System.Collections.Generic;

public sealed class Observable<T> : IObservable<T>, INotifyPropertyChanged
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
}
