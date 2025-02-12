// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Parameters;

/// <summary>
/// Base class of all parameters received from VRChat
/// </summary>
public class ReceivedParameter
{
    /// <summary>
    /// The received name of the parameter
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The raw value of this parameter. Useful when used alongside <see cref="Type"/>
    /// </summary>
    /// <remarks>If you want to get this as a specific value, call <see cref="GetValue{T}"/></remarks>
    public object Value { get; }

    /// <summary>
    /// The type of this parameter
    /// </summary>
    public ParameterType Type { get; }

    internal ReceivedParameter(string name, object value)
    {
        Name = name;
        Value = value;
        Type = ParameterTypeFactory.CreateFrom(value);
    }

    internal ReceivedParameter(ReceivedParameter other)
    {
        Name = other.Name;
        Value = other.Value;
        Type = other.Type;
    }

    /// <summary>
    /// Retrieves the value as type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type to get the value as</typeparam>
    /// <returns>The value as type <typeparamref name="T"/> if applicable</returns>
    public virtual T GetValue<T>()
    {
        if (Value is T valueAsType) return valueAsType;

        throw new InvalidOperationException($"Please call {nameof(IsValueType)} to validate a value's type before calling {nameof(GetValue)}");
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
    public bool IsValueType(Type type) => ParameterTypeFactory.CreateFrom(type) == Type;
}

/// <summary>
/// A <see cref="ReceivedParameter"/> that is associated with a <see cref="ModuleParameter"/>
/// </summary>
public sealed class RegisteredParameter : ReceivedParameter
{
    /// <summary>
    /// The lookup of the module parameter
    /// </summary>
    public Enum Lookup { get; }

    private readonly ModuleParameter moduleParameter;
    private readonly Match match;

    private readonly List<Wildcard> wildcards = [];

    internal RegisteredParameter(ReceivedParameter other, Enum lookup, ModuleParameter moduleParameter, Match match)
        : base(other)
    {
        Lookup = lookup;
        this.moduleParameter = moduleParameter;
        this.match = match;

        decodeWildcards();
    }

    private void decodeWildcards()
    {
        for (var index = 1; index < match.Groups.Count; index++)
        {
            var group = match.Groups[index];
            var value = group.Captures[0].Value;

            if (int.TryParse(value, out var valueInt))
            {
                wildcards.Add(new Wildcard(valueInt));
                continue;
            }

            if (float.TryParse(value, out var valueFloat))
            {
                wildcards.Add(new Wildcard(valueFloat));
                continue;
            }

            wildcards.Add(new Wildcard(value));
        }
    }

    public override T GetValue<T>()
    {
        if (ParameterTypeFactory.CreateFrom<T>() != moduleParameter.ExpectedType)
            throw new InvalidCastException($"Parameter's value was expected as {moduleParameter.ExpectedType} and you're trying to use it as {typeof(T).ToReadableName()}");

        return base.GetValue<T>();
    }

    /// <summary>
    /// Gets a wildcard value as a specified type <typeparamref name="T"/> at <paramref name="position"/>
    /// </summary>
    public T GetWildcard<T>(int position) => wildcards[position].GetValue<T>();

    /// <summary>
    /// Checks a wildcard's value type <typeparamref name="T"/> at <paramref name="position"/>
    /// </summary>
    /// <returns>True if the value is exactly the type passed, otherwise false</returns>
    public bool IsWildcardType<T>(int position) => IsWildcardType(typeof(T), position);

    /// <summary>
    /// Checks a wildcard's value type <paramref name="type"/> at <paramref name="position"/>
    /// </summary>
    /// <returns>True if the value is exactly the type passed, otherwise false</returns>
    public bool IsWildcardType(Type type, int position) => wildcards[position].IsValueType(type);
}

public class Wildcard
{
    private readonly object value;

    internal Wildcard(object value)
    {
        this.value = value;
    }

    public T GetValue<T>()
    {
        if (value is T valueAsType) return valueAsType;

        throw new InvalidOperationException($"Please call {nameof(RegisteredParameter.IsWildcardType)} to validate a wildcard's type before calling {nameof(RegisteredParameter.GetWildcard)}");
    }

    /// <summary>
    /// Checks if the received value is of type <paramref name="type"/>
    /// </summary>
    /// <returns>True if the value is exactly the type passed, otherwise false</returns>
    public bool IsValueType(Type type) => value.GetType() == type;
}