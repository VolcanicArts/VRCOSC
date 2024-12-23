// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCOSC.App.SDK.Parameters.Queryable;

[JsonConverter(typeof(QueryableParameterListConverter))]
public class QueryableParameterList
{
    private readonly Type queryableParameterType;

    public IList Parameters { get; internal set; }
    public Array ActionTypeSource => queryableParameterType.GenericTypeArguments.Length > 0 ? Enum.GetValues(queryableParameterType.GenericTypeArguments[0]) : Array.Empty<object>();

    public QueryableParameterList(Type queryableParameterType)
    {
        this.queryableParameterType = queryableParameterType;
        Parameters = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(queryableParameterType))!;
    }

    internal void Add()
    {
        Parameters.Add(Activator.CreateInstance(queryableParameterType)!);
    }

    public void Remove(object instance)
    {
        Parameters.Remove(instance);
    }
}

public class QueryableParameterListConverter : JsonConverter<QueryableParameterList>
{
    public override void WriteJson(JsonWriter writer, QueryableParameterList? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value!.Parameters);
    }

    public override QueryableParameterList ReadJson(JsonReader reader, Type objectType, QueryableParameterList? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        existingValue!.Parameters = (IList)serializer.Deserialize(reader, existingValue.Parameters.GetType())!;
        return existingValue;
    }
}