// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.SDK.Parameters;

public record ParameterDefinition
{
    /// <summary>
    /// The name of the parameter
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of the parameter
    /// </summary>
    public ParameterType Type { get; }

    internal ParameterDefinition(string name, ParameterType type)
    {
        Name = name;
        Type = type;
    }

    public virtual bool Equals(ParameterDefinition? other) => Name == other?.Name && Type == other.Type;

    public override int GetHashCode() => HashCode.Combine(Name, Type);
}

public partial record VRChatParameter
{
    [GeneratedRegex(@"^(?:VF\d+_)?(.+)$")]
    private static partial Regex NameRegex();

    /// <summary>
    /// The raw name of this parameter. This includes things like the VRCFury prefix
    /// </summary>
    public string RawName { get; }

    /// <summary>
    /// The name of this parameter
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of this parameter
    /// </summary>
    public ParameterType Type { get; }

    /// <summary>
    /// The raw value of this parameter. Useful when used alongside <see cref="Type"/>
    /// </summary>
    /// <remarks>If you want to get this as a specific value, call <see cref="GetValue{T}"/></remarks>
    public object Value { get; }

    internal VRChatParameter(string rawName, object value)
    {
        RawName = rawName;
        Name = NameRegex().Match(rawName).Groups[1].Captures[0].Value;
        Type = ParameterTypeFactory.CreateFrom(value);
        Value = value;
    }

    internal VRChatParameter(VRChatOSCMessage message)
        : this(message.ParameterName, message.ParameterValue)
    {
    }

    public ParameterDefinition GetDefinition() => new(Name, Type);

    /// <summary>
    /// Retrieves the value as type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type to get the value as</typeparam>
    /// <returns>The value as type <typeparamref name="T"/> if applicable</returns>
    public T GetValue<T>()
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

    public virtual bool Equals(VRChatParameter? other) => GetDefinition() == other?.GetDefinition();

    public override int GetHashCode() => HashCode.Combine(Name, Type);
}

public record TemplatedVRChatParameter : VRChatParameter
{
    public static Regex TemplateAsRegex(string template) => new($"^(?:{Regex.Escape(template).Replace(@"\*", @"(\S*?)")})$");

    private Regex templateRegex { get; }

    private List<Wildcard> wildcards { get; } = [];

    internal TemplatedVRChatParameter(Regex templateRegex, VRChatParameter other)
        : base(other)
    {
        this.templateRegex = templateRegex;
        decodeWildcards();
    }

    internal TemplatedVRChatParameter(string template, VRChatParameter other)
        : base(other)
    {
        templateRegex = TemplateAsRegex(template);
        decodeWildcards();
    }

    internal bool IsMatch() => templateRegex.IsMatch(Name);

    private void decodeWildcards()
    {
        if (!IsMatch()) return;

        var match = templateRegex.Match(Name);

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

            if (bool.TryParse(value, out var valueBool))
            {
                wildcards.Add(new Wildcard(valueBool));
                continue;
            }

            wildcards.Add(new Wildcard(value));
        }
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

public record RegisteredParameter : TemplatedVRChatParameter
{
    public Enum Lookup { get; }

    internal RegisteredParameter(Enum lookup, TemplatedVRChatParameter other)
        : base(other)
    {
        Lookup = lookup;
    }
}

public record Wildcard
{
    private readonly object value;

    internal Wildcard(object value)
    {
        this.value = value;
    }

    public T GetValue<T>()
    {
        if (value is T valueAsType) return valueAsType;

        throw new InvalidOperationException($"Please call {nameof(TemplatedVRChatParameter.IsWildcardType)} to validate a wildcard's type before calling {nameof(TemplatedVRChatParameter.GetWildcard)}");
    }

    /// <summary>
    /// Checks if the received value is of type <paramref name="type"/>
    /// </summary>
    /// <returns>True if the value is exactly the type passed, otherwise false</returns>
    internal bool IsValueType(Type type) => value.GetType() == type;
}