// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public interface IGraphVariable
{
    Observable<string> Name { get; }

    void Reset();
    Guid GetId();
    string GetName();
    bool IsPersistent();
    Type GetValueType();
    object GetValue();
};

public class GraphVariable<T> : IGraphVariable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("New Variable");
    public bool Persistent { get; }
    public Observable<T> Value { get; } = new();
    public Type ValueType => typeof(T);

    public GraphVariable(string name, bool persistent)
    {
        Name.Value = name;
        Persistent = persistent;
    }

    public GraphVariable(Guid id, string name, bool persistent, T value)
    {
        Id = id;
        Name.Value = name;
        Persistent = persistent;
        Value.Value = value;
    }

    public GraphVariable(Guid id, string name, bool persistent)
    {
        Id = id;
        Name.Value = name;
        Persistent = persistent;
    }

    public void Reset()
    {
        if (Persistent) return;

        Value.SetDefault();
    }

    public Guid GetId() => Id;
    public string GetName() => Name.Value;
    public bool IsPersistent() => Persistent;
    public Type GetValueType() => ValueType;
    public object GetValue() => Value.Value!;
    public void Write(T newValue) => Value.Value = newValue;
}