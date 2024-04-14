// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips.Variables;

// TODO: Should this extend so I can have default values for variable options?
public class ClipVariableReference
{
    internal string ModuleID { get; init; } = null!;
    internal string VariableID { get; init; } = null!;
    internal Type ClipVariableType { get; init; } = null!;
    internal Type ValueType { get; init; } = null!;

    public Observable<string> DisplayName { get; } = new("INVALID");

    internal Observable<object?> Value = new();

    internal void SetValue<T>(T value)
    {
        if (typeof(T) != ValueType)
            throw new InvalidOperationException($"The provided value type `{typeof(T).Name}` doesn't match the expected value type `{ValueType.Name}` for variable {ModuleID} - {VariableID}");

        Value.Value = value;
    }

    internal ClipVariable CreateInstance() => (ClipVariable)Activator.CreateInstance(ClipVariableType, this)!;
}
