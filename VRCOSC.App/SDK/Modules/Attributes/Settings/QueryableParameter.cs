// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public interface IQueryableParameter
{
}

public class QueryableParameter<TAction> : QueryableParameter, IEquatable<QueryableParameter<TAction>> where TAction : Enum
{
    public Observable<TAction> ChosenResult { get; } = new();

    public bool Evaluate(bool value, out TAction result)
    {
        var evaluateResult = Evaluate(value);
        result = evaluateResult ? ChosenResult.Value : default!;
        return evaluateResult;
    }

    public bool Evaluate(int value, out TAction result)
    {
        var evaluateResult = Evaluate(value);
        result = evaluateResult ? ChosenResult.Value : default!;
        return evaluateResult;
    }

    public bool Evaluate(float value, out TAction result)
    {
        var evaluateResult = Evaluate(value);
        result = evaluateResult ? ChosenResult.Value : default!;
        return evaluateResult;
    }

    public bool Equals(QueryableParameter<TAction>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return ChosenResult.Equals(other.ChosenResult);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((QueryableParameter<TAction>)obj);
    }

    public override int GetHashCode() => ChosenResult.GetHashCode();
}

public class QueryableParameter : IQueryableParameter, ICloneable, IEquatable<QueryableParameter>
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

    internal bool Evaluate(ReceivedParameter parameter)
    {
        return parameter.Type switch
        {
            ParameterType.Bool => Evaluate(parameter.GetValue<bool>()),
            ParameterType.Int => Evaluate(parameter.GetValue<int>()),
            ParameterType.Float => Evaluate(parameter.GetValue<float>()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public bool Evaluate(bool value)
    {
        // TODO: Instead of returning bool, return a query result
        // This allows for whether the query is true, and if it's
        // just become true for the first times since becoming false

        if (Type.Value != ParameterType.Bool) return false;

        var success = evaluate(value);
        previousBoolValue = value;
        return success;
    }

    public bool Evaluate(int value)
    {
        if (Type.Value != ParameterType.Int) return false;

        var success = evaluate(value);
        previousIntValue = value;
        return success;
    }

    public bool Evaluate(float value)
    {
        if (Type.Value != ParameterType.Float) return false;

        var success = evaluate(value);
        previousFloatValue = value;

        return success;
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

    public object Clone()
    {
        return this;
    }

    public bool Equals(QueryableParameter? other)
    {
        return false;
    }
}