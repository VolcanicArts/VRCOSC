// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Parameters.Queryable;
using VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public class QueryableParameterListModuleSetting<T> : ListModuleSetting<T> where T : QueryableParameter, IEquatable<T>, new()
{
    public QueryableParameterListModuleSetting(string title, string description)
        : base(title, description, typeof(QueryableParameterListModuleSettingView), [])
    {
    }

    protected override T CreateItem() => new();
}