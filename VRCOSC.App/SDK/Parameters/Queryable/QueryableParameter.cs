// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Parameters.Queryable;

public class QueryResult
{
    public readonly bool IsValid;
    public readonly bool JustBecameValid;
    public readonly bool JustBecameInvalid;

    public QueryResult(bool isValid, bool justBecameValid, bool justBecameInvalid)
    {
        IsValid = isValid;
        JustBecameValid = justBecameValid;
        JustBecameInvalid = justBecameInvalid;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class QueryableParameter : IEquatable<QueryableParameter>
{
    [JsonProperty("name")]
    public Observable<string> Name { get; } = new(string.Empty);

    [JsonProperty("type")]
    public Observable<ParameterType> Type { get; } = new();

    [JsonProperty("comparison")]
    public Observable<ComparisonOperation> Comparison { get; } = new();

    [JsonProperty("bool_value")]
    public Observable<bool> BoolValue { get; } = new();

    [JsonProperty("int_value")]
    public Observable<int> IntValue { get; } = new();

    [JsonProperty("float_value")]
    public Observable<float> FloatValue { get; } = new();

    private bool previousBoolValue;
    private int previousIntValue;
    private float previousFloatValue;

    private bool previousValid;

    public async Task Init()
    {
        previousValid = false;

        var parameter = await AppManager.GetInstance().VRChatOscClient.FindParameter(Name.Value, CancellationToken.None);
        if (parameter is null) return;

        switch (parameter.Type)
        {
            case ParameterType.Bool:
                previousBoolValue = parameter.GetValue<bool>();
                previousValid = evaluate(previousBoolValue);
                break;

            case ParameterType.Int:
                previousIntValue = parameter.GetValue<int>();
                previousValid = evaluate(previousIntValue);
                break;

            case ParameterType.Float:
                previousFloatValue = parameter.GetValue<float>();
                previousValid = evaluate(previousFloatValue);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public QueryResult Evaluate(VRChatParameter parameter)
    {
        try
        {
            var valid = parameter.Type switch
            {
                ParameterType.Bool => evaluate(parameter.GetValue<bool>()),
                ParameterType.Int => evaluate(parameter.GetValue<int>()),
                ParameterType.Float => evaluate(parameter.GetValue<float>()),
                _ => throw new ArgumentOutOfRangeException(nameof(parameter.Type), parameter.Type, null)
            };

            var queryResult = new QueryResult(valid, valid && !previousValid, !valid && previousValid);
            previousValid = valid;

            return queryResult;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(QueryableParameter)} has experienced an exception when evaluating");
            return new QueryResult(false, false, false);
        }
    }

    private bool evaluate(bool value)
    {
        var valid = eval(value);
        previousBoolValue = value;
        return valid;
    }

    private bool evaluate(int value)
    {
        var valid = eval(value);
        previousIntValue = value;
        return valid;
    }

    private bool evaluate(float value)
    {
        var valid = eval(value);
        previousFloatValue = value;
        return valid;
    }

    private bool eval(object value) => Comparison.Value switch
    {
        ComparisonOperation.Changed => handleChangedOperation(value),
        ComparisonOperation.EqualTo => handleEqualToOperation(value),
        ComparisonOperation.NotEqualTo => !handleEqualToOperation(value),
        ComparisonOperation.LessThan => handleLessThanOperation(value),
        ComparisonOperation.GreaterThan => handleGreaterThanOperation(value),
        ComparisonOperation.LessThanOrEqualTo => handleLessThanOrEqualToOperation(value),
        ComparisonOperation.GreaterThanOrEqualTo => handleGreaterThanOrEqualToOperation(value),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    private bool handleChangedOperation(object value) => value switch
    {
        bool boolValue => previousBoolValue != boolValue,
        int intValue => previousIntValue != intValue,
        float floatValue => Math.Abs(previousFloatValue - floatValue) > float.Epsilon,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    private bool handleEqualToOperation(object value) => value switch
    {
        bool boolValue => BoolValue.Value == boolValue && previousBoolValue != boolValue,
        int intValue => IntValue.Value == intValue && previousIntValue != intValue,
        float floatValue => Math.Abs(FloatValue.Value - floatValue) < float.Epsilon && Math.Abs(previousFloatValue - floatValue) > float.Epsilon,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    private bool handleLessThanOperation(object value) => value switch
    {
        bool => throw new InvalidOperationException(),
        int intValue => previousIntValue >= IntValue.Value && intValue < IntValue.Value,
        float floatValue => previousFloatValue >= FloatValue.Value && floatValue < FloatValue.Value,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    private bool handleGreaterThanOperation(object value) => value switch
    {
        bool => throw new InvalidOperationException(),
        int intValue => previousIntValue <= IntValue.Value && intValue > IntValue.Value,
        float floatValue => previousFloatValue <= FloatValue.Value && floatValue > FloatValue.Value,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    private bool handleLessThanOrEqualToOperation(object value) => value switch
    {
        bool => throw new InvalidOperationException(),
        int intValue => previousIntValue > IntValue.Value && intValue <= IntValue.Value,
        float floatValue => previousFloatValue > FloatValue.Value && floatValue <= FloatValue.Value,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    private bool handleGreaterThanOrEqualToOperation(object value) => value switch
    {
        bool => throw new InvalidOperationException(),
        int intValue => previousIntValue < IntValue.Value && intValue >= IntValue.Value,
        float floatValue => previousFloatValue < FloatValue.Value && floatValue >= FloatValue.Value,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public bool Equals(QueryableParameter? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name.Equals(other.Name) && Type.Equals(other.Type) && Comparison.Equals(other.Comparison) && BoolValue.Equals(other.BoolValue) && IntValue.Equals(other.IntValue)
               && FloatValue.Equals(other.FloatValue);
    }
}