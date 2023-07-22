// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Modules;

public class AvatarParameter
{
    private readonly ModuleParameter? moduleParameter;
    public string Name { get; private set; }
    private object value { get; set; }
    public Enum? Lookup { get; private set; }
    private readonly List<Wildcard> wildcards = new();

    internal AvatarParameter(Enum? lookup, string receivedParameter, object value)
    {
        Name = receivedParameter;
        this.value = value;
        Lookup = lookup;
    }

    internal AvatarParameter(ModuleParameter moduleParameter, Enum? lookup, string receivedParameter, object value)
        : this(lookup, receivedParameter, value)
    {
        this.moduleParameter = moduleParameter;
        wildcards.Clear();

        var referenceSections = receivedParameter.Split("/");
        var originalSections = moduleParameter.ParameterName.Split("/");

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

    /// <summary>
    /// Gets a wildcard value as a specified type <typeparamref name="T"/> at <paramref name="position"/>
    /// </summary>
    public T WildcardAs<T>(int position) => wildcards[position].ValueAs<T>();

    public T ValueAs<T>()
    {
        if (moduleParameter is not null && typeof(T) != moduleParameter.ExpectedType)
            throw new InvalidCastException($"Parameter's value was expected as {moduleParameter.ExpectedType.ToReadableName()} and you're trying to use it as {typeof(T).ToReadableName()}");

        return (T)Convert.ChangeType(value, typeof(T));
    }

    public bool IsValueType<T>() => value.GetType() == typeof(T);
}

public class Wildcard
{
    private readonly object value;

    internal Wildcard(object value)
    {
        this.value = value;
    }

    public T ValueAs<T>() => (T)Convert.ChangeType(value, typeof(T));
}
