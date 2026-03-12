// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics.CodeAnalysis;

namespace VRCOSC.App.Utils;

public record Result
{
    public Exception? Exception { get; protected init; }

    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsSuccess => Exception is null;

    public static Result Success() => new();

    public static Result Error(Exception exception) => new()
    {
        Exception = exception
    };

    public static Result Error(string errorMessage) => new()
    {
        Exception = new Exception(errorMessage)
    };

    public static implicit operator Result(bool success) => success ? Success() : Error("Failed");

    public static implicit operator Result(Exception exception) => Error(exception);
}

public record Result<T> : Result
{
    public T Value { get; private init; } = default!;

    public static Result<T> Success(T value) => new()
    {
        Value = value
    };

    public new static Result<T> Error(Exception exception) => new()
    {
        Exception = exception,
    };

    public new static Result<T> Error(string errorMessage) => new()
    {
        Exception = new Exception(errorMessage)
    };

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Exception exception) => Error(exception);

    public Result<TOther> To<TOther>()
    {
        if (!IsSuccess) return Exception;

        if (Value is TOther other) return other;

        throw new InvalidOperationException($"{typeof(T).GetFriendlyName()} cannot convert to {typeof(TOther).GetFriendlyName()}");
    }
}