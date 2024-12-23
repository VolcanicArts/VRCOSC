// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using VRCOSC.App.SDK.Parameters.Queryable;
using VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public class QueryableParameterListModuleSetting : ModuleSetting
{
    public QueryableParameterList QueryableParameterList { get; }

    public QueryableParameterListModuleSetting(string title, string description)
        : base(title, description, typeof(QueryableParameterListModuleSettingView))
    {
        QueryableParameterList = CreateParameterList();
    }

    internal override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not JArray jArrayValue) return false;

        QueryableParameterList.Parameters.Clear();

        foreach (var ingestItem in jArrayValue.Select(CreateQueryableParameter))
        {
            QueryableParameterList.Parameters.Add(ingestItem);
        }

        return true;
    }

    protected virtual object CreateQueryableParameter(JToken token) => token.ToObject<QueryableParameter>()!;

    internal override object Serialise() => QueryableParameterList.Parameters;

    internal override bool IsDefault() => QueryableParameterList.Parameters.Count == 0;

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) != typeof(QueryableParameterList))
        {
            returnValue = default(T);
            return false;
        }

        returnValue = (T)Convert.ChangeType(QueryableParameterList, typeof(T));
        return true;
    }

    protected virtual QueryableParameterList CreateParameterList() => new(typeof(QueryableParameter));
}

public sealed class ActionableQueryableParameterListModuleSetting : QueryableParameterListModuleSetting
{
    public Type ActionType { get; }

    public ActionableQueryableParameterListModuleSetting(string title, string description, Type actionType)
        : base(title, description)
    {
        ActionType = actionType;
    }

    protected override object CreateQueryableParameter(JToken token) => token.ToObject(typeof(ActionableQueryableParameter<>).MakeGenericType(ActionType))!;

    protected override QueryableParameterList CreateParameterList() => new(typeof(ActionableQueryableParameter<>).MakeGenericType(ActionType));
}