// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Utils;

public record Result<T>
{
    public Exception Exception { get; protected init; } = null!;
    public bool IsSuccess => Exception is null;
    public T Value { get; private init; } = default!;

    public static Result<T> Success(T value) => new()
    {
        Value = value
    };

    public static Result<T> Error(Exception exception) => new()
    {
        Exception = exception
    };

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Exception exception) => Error(exception);

    public static implicit operator bool(Result<T> result) => result.IsSuccess;

    public Result<TOther> To<TOther>()
    {
        if (!IsSuccess) return Exception;

        if (Value is TOther other) return other;

        throw new InvalidOperationException($"{typeof(T).GetFriendlyName()} cannot convert to {typeof(TOther).GetFriendlyName()}");
    }
}