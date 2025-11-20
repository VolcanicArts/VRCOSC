// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public interface IGraphVariable
{
    public void Reset();
    public Guid GetId();
    public string GetName();
    public bool IsPersistent();
    public Type GetValueType();
    public object GetValue();
}

public class GraphVariable<T> : IGraphVariable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("New Variable");
    public Observable<bool> Persistent { get; } = new();
    public Observable<T> Value { get; } = new();
    public Type ValueType => typeof(T);

    public GraphVariable(string name, bool persistent)
    {
        Name.Value = name;
        Persistent.Value = persistent;
    }

    public GraphVariable(Guid id, string name, bool persistent, T value)
    {
        Id = id;
        Name.Value = name;
        Persistent.Value = persistent;
        Value.Value = value;
    }

    public GraphVariable(Guid id, string name, bool persistent)
    {
        Id = id;
        Name.Value = name;
        Persistent.Value = persistent;
    }

    public void Reset()
    {
        if (Persistent.Value) return;

        Value.SetDefault();
    }

    public Guid GetId() => Id;

    public string GetName() => Name.Value;

    public bool IsPersistent() => Persistent.Value;

    public Type GetValueType() => typeof(T);

    public object GetValue() => Value.Value!;

    public void Write(T newValue)
    {
        Value.Value = newValue;
    }
}