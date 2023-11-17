// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.Game.Modules.SDK.Attributes;

namespace VRCOSC.Game.Modules.SDK.Parameters;

/// <summary>
/// Base class of all parameters received from VRChat
/// </summary>
public class ReceivedParameter
{
    /// <summary>
    /// The received name of the parameter
    /// </summary>
    public readonly string Name;

    internal readonly object Value;

    internal ReceivedParameter(string name, object value)
    {
        Name = name;
        Value = value;
    }

    internal ReceivedParameter(ReceivedParameter other)
    {
        Name = other.Name;
        Value = other.Value;
    }

    /// <summary>
    /// Retrieves the value as type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type to get the value as</typeparam>
    /// <returns>The value as type <typeparamref name="T"/> if applicable, otherwise default</returns>
    public virtual T? GetValue<T>()
    {
        if (Value is T valueAsType)
        {
            return valueAsType;
        }

        return default;
    }

    /// <summary>
    /// Checks if the received value is of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type to check against the value</typeparam>
    /// <returns>True if the value is exactly the type passed, otherwise false</returns>
    public bool IsValueType<T>() => IsValueType(typeof(T));

    /// <summary>
    /// Checks if the received value is of type <paramref name="type"/>
    /// </summary>
    /// <returns>True if the value is exactly the type passed, otherwise false</returns>
    public bool IsValueType(Type type) => Value.GetType() == type;
}

/// <summary>
/// A <see cref="ReceivedParameter"/> that is associated with a <see cref="ModuleParameter"/>
/// </summary>
public class RegisteredParameter : ReceivedParameter
{
    /// <summary>
    /// The lookup of the module parameter
    /// </summary>
    public readonly Enum Lookup;

    private readonly ModuleParameter moduleParameter;
    private readonly List<Wildcard> wildcards = new();

    internal RegisteredParameter(ReceivedParameter other, Enum lookup, ModuleParameter moduleParameter)
        : base(other)
    {
        Lookup = lookup;
        this.moduleParameter = moduleParameter;

        decodeWildcards();
    }

    internal RegisteredParameter(RegisteredParameter other)
        : base(other)
    {
        Lookup = other.Lookup;
        moduleParameter = other.moduleParameter;

        decodeWildcards();
    }

    private void decodeWildcards()
    {
        var referenceSections = Name.Split("/");
        var originalSections = moduleParameter.Name.Value.Split("/");

        for (int i = 0; i < originalSections.Length; i++)
        {
            if (originalSections[i] != "*") continue;

            var referenceValue = referenceSections[i];

            if (int.TryParse(referenceValue, out var referenceValueInt))
            {
                wildcards.Add(new Wildcard(referenceValueInt));
                continue;
            }

            if (float.TryParse(referenceValue, out var referenceValueFloat))
            {
                wildcards.Add(new Wildcard(referenceValueFloat));
                continue;
            }

            wildcards.Add(new Wildcard(referenceValue));
        }
    }

    public override T GetValue<T>()
    {
        if (typeof(T) != moduleParameter.Metadata.ExpectedType)
            throw new InvalidCastException($"Parameter's value was expected as {moduleParameter.Metadata.ExpectedType.ToReadableName()} and you're trying to use it as {typeof(T).ToReadableName()}");

        return base.GetValue<T>()!;
    }

    /// <summary>
    /// Gets a wildcard value as a specified type <typeparamref name="T"/> at <paramref name="position"/>
    /// </summary>
    public T? WildcardAs<T>(int position) => wildcards[position].GetValue<T>();

    public bool IsWildcardType<T>(int position) => wildcards[position].IsValueType<T>();
}

public class Wildcard
{
    private readonly object value;

    internal Wildcard(object value)
    {
        this.value = value;
    }

    public T? GetValue<T>()
    {
        if (value is T valueAsType)
        {
            return valueAsType;
        }

        return default;
    }

    /// <summary>
    /// Checks if the received value is of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type to check against the value</typeparam>
    /// <returns>True if the value is exactly the type passed, otherwise false</returns>
    public bool IsValueType<T>() => IsValueType(typeof(T));

    /// <summary>
    /// Checks if the received value is of type <paramref name="type"/>
    /// </summary>
    /// <returns>True if the value is exactly the type passed, otherwise false</returns>
    public bool IsValueType(Type type) => value.GetType() == type;
}
