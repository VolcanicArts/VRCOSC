// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace VRCOSC.App.Utils;

public interface IRef
{
    Type ValueType { get; }
    object? GetValue();
    void SetValue(object? value);
}

public class Ref<T> : IRef, IEqualityComparer<T>
{
    public T Value;
    public Type ValueType => typeof(T);

    public Ref(T startValue = default!)
    {
        Value = startValue;
    }

    // Required for Activator.CreateInstance
    public Ref()
    {
        Value = default!;
    }

    public object? GetValue() => Value;
    public void SetValue(object? value) => Value = (T)value!;

    public bool Equals(T? x, T? y) => EqualityComparer<T>.Default.Equals(x, y);

    public int GetHashCode([DisallowNull] T obj) => EqualityComparer<T>.Default.GetHashCode(Value!);

    public override bool Equals(object? obj) => obj is Ref<T> other && Equals(Value, other.Value);
}