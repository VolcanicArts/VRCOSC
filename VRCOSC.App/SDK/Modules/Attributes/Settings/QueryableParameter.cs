// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public class QueryResult
{
    public bool IsValid { get; internal set; }
    public bool JustBecameValid { get; internal set; }
    public bool JustBecameInvalid { get; internal set; }
}

public class ActionableQueryResult : QueryResult
{
    private readonly object actionResult;

    public ActionableQueryResult(QueryResult other, object actionResult)
    {
        IsValid = other.IsValid;
        JustBecameValid = other.JustBecameValid;
        JustBecameInvalid = other.JustBecameInvalid;
        this.actionResult = actionResult;
    }

    public T GetActionResult<T>()
    {
        return (T)actionResult;
    }
}

public class ActionableQueryableParameter : QueryableParameter, IEquatable<ActionableQueryableParameter>
{
    public ActionableQueryableParameter(Type actionEnumType)
    {
        ActionEnumType = actionEnumType;
    }

    internal Type ActionEnumType { get; }

    public Observable<object> Action { get; } = new();

    public new ActionableQueryResult Evaluate(ReceivedParameter parameter)
    {
        return parameter.Type switch
        {
            ParameterType.Bool => Evaluate(parameter.GetValue<bool>()),
            ParameterType.Int => Evaluate(parameter.GetValue<int>()),
            ParameterType.Float => Evaluate(parameter.GetValue<float>()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected new ActionableQueryResult Evaluate(bool value)
    {
        var queryResult = base.Evaluate(value);
        return new ActionableQueryResult(queryResult, Action.Value);
    }

    protected new ActionableQueryResult Evaluate(int value)
    {
        var queryResult = base.Evaluate(value);
        return new ActionableQueryResult(queryResult, Action.Value);
    }

    protected new ActionableQueryResult Evaluate(float value)
    {
        var queryResult = base.Evaluate(value);
        return new ActionableQueryResult(queryResult, Action.Value);
    }

    public bool Equals(ActionableQueryableParameter? other) => base.Equals(other) && Action.Equals(other.Action);
}

public class QueryableParameter : IEquatable<QueryableParameter>
{
    public Observable<string> Name { get; } = new(string.Empty);
    public Observable<ParameterType> Type { get; } = new();
    public Observable<ComparisonOperation> Comparison { get; } = new();

    public Observable<bool> BoolValue { get; } = new();
    public Observable<int> IntValue { get; } = new();
    public Observable<float> FloatValue { get; } = new();

    // TODO: Should parameters be cached globally and this can just reference that?
    // The cache can just update all the parameters after module update
    private bool previousBoolValue;
    private int previousIntValue;
    private float previousFloatValue;

    private bool previousValid;

    public QueryResult Evaluate(ReceivedParameter parameter)
    {
        return parameter.Type switch
        {
            ParameterType.Bool => Evaluate(parameter.GetValue<bool>()),
            ParameterType.Int => Evaluate(parameter.GetValue<int>()),
            ParameterType.Float => Evaluate(parameter.GetValue<float>()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected QueryResult Evaluate(bool value)
    {
        var queryResult = new QueryResult();

        var valid = evaluate(value);

        queryResult.IsValid = valid;
        queryResult.JustBecameValid = valid && !previousValid;
        queryResult.JustBecameInvalid = !valid && previousValid;

        previousBoolValue = value;
        previousValid = valid;

        return queryResult;
    }

    protected QueryResult Evaluate(int value)
    {
        var queryResult = new QueryResult();

        var valid = evaluate(value);

        queryResult.IsValid = valid;
        queryResult.JustBecameValid = valid && !previousValid;
        queryResult.JustBecameInvalid = !valid && previousValid;

        previousIntValue = value;
        previousValid = valid;

        return queryResult;
    }

    protected QueryResult Evaluate(float value)
    {
        var queryResult = new QueryResult();

        var valid = evaluate(value);

        queryResult.IsValid = valid;
        queryResult.JustBecameValid = valid && !previousValid;
        queryResult.JustBecameInvalid = !valid && previousValid;

        previousFloatValue = value;
        previousValid = valid;

        return queryResult;
    }

    private bool evaluate(object value) => Comparison.Value switch
    {
        ComparisonOperation.Changed => handleChangedOperation(value),
        ComparisonOperation.EqualTo => handleEqualToOperation(value),
        ComparisonOperation.NotEqualTo => !handleEqualToOperation(value),
        ComparisonOperation.LessThan => handleLessThanOperation(value),
        ComparisonOperation.GreaterThan => handleMoreThanOperation(value),
        ComparisonOperation.LessThanOrEqualTo => false,
        ComparisonOperation.GreaterThanOrEqualTo => false,
        _ => throw new ArgumentOutOfRangeException()
    };

    private bool handleChangedOperation(object value) => value switch
    {
        bool boolValue => previousBoolValue != boolValue,
        int intValue => previousIntValue != intValue,
        float floatValue => Math.Abs(previousFloatValue - floatValue) > float.Epsilon,
        _ => throw new InvalidOperationException()
    };

    private bool handleEqualToOperation(object value) => value switch
    {
        bool boolValue => BoolValue.Value == boolValue && previousBoolValue != boolValue,
        int intValue => IntValue.Value == intValue && previousIntValue != intValue,
        float floatValue => Math.Abs(FloatValue.Value - floatValue) < float.Epsilon && Math.Abs(previousFloatValue - floatValue) > float.Epsilon,
        _ => throw new ArgumentOutOfRangeException()
    };

    private bool handleLessThanOperation(object value) => value switch
    {
        bool => throw new InvalidOperationException(),
        int intValue => previousIntValue >= IntValue.Value && intValue < IntValue.Value,
        float floatValue => previousFloatValue >= FloatValue.Value && floatValue < FloatValue.Value,
        _ => throw new ArgumentOutOfRangeException()
    };

    private bool handleMoreThanOperation(object value) => value switch
    {
        bool => throw new InvalidOperationException(),
        int intValue => previousIntValue <= IntValue.Value && intValue > IntValue.Value,
        float floatValue => previousFloatValue <= FloatValue.Value && floatValue > FloatValue.Value,
        _ => throw new ArgumentOutOfRangeException()
    };

    public bool Equals(QueryableParameter? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name.Equals(other.Name) && Type.Equals(other.Type) && Comparison.Equals(other.Comparison) && BoolValue.Equals(other.BoolValue) && IntValue.Equals(other.IntValue) && FloatValue.Equals(other.FloatValue);
    }
}