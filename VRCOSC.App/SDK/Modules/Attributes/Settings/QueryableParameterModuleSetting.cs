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

    public Type? ActionType { get; }

    public QueryableParameterListModuleSetting(string title, string description, Type? actionType)
        : base(title, description, typeof(QueryableParameterListModuleSettingView))
    {
        QueryableParameterList = createParameterList();
        ActionType = actionType;
    }

    protected override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not JArray jArrayValue) return false;

        QueryableParameterList.Parameters.Clear();

        foreach (var ingestItem in jArrayValue.Select(createQueryableParameter))
        {
            QueryableParameterList.Parameters.Add(ingestItem);
        }

        return true;
    }

    private object createQueryableParameter(JToken token) => ActionType is null ? token.ToObject<QueryableParameter>()! : token.ToObject(typeof(ActionableQueryableParameter<>).MakeGenericType(ActionType))!;
    private QueryableParameterList createParameterList() => ActionType is null ? new QueryableParameterList(typeof(QueryableParameter)) : new QueryableParameterList(typeof(ActionableQueryableParameter<>).MakeGenericType(ActionType));

    protected override object Serialise() => QueryableParameterList.Parameters;

    protected override bool IsDefault() => QueryableParameterList.Parameters.Count == 0;

    public override TOut GetValue<TOut>()
    {
        if (typeof(TOut) != typeof(QueryableParameterList)) throw new InvalidCastException($"Requested type must only be {nameof(QueryableParameterList)}");

        return (TOut)Convert.ChangeType(QueryableParameterList, typeof(TOut));
    }
}