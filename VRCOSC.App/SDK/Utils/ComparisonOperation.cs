// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

namespace VRCOSC.App.SDK.Utils;

public enum ComparisonOperation
{
    Changed,
    EqualTo,
    NotEqualTo,
    GreaterThan,
    LessThan,
    GreaterThanOrEqualTo,
    LessThanOrEqualTo
}

public static class ComparisonOperationUtils
{
    public static readonly IEnumerable<KeyValuePair<string, ComparisonOperation>> DISPLAY_LIST = new List<KeyValuePair<string, ComparisonOperation>>
    {
        new("Changed", ComparisonOperation.Changed),
        new("Equal To", ComparisonOperation.EqualTo),
        new("Not Equal To", ComparisonOperation.NotEqualTo),
        new("Greater Than", ComparisonOperation.GreaterThan),
        new("Less Than", ComparisonOperation.LessThan),
        new("Greater Than Or Equal To", ComparisonOperation.GreaterThanOrEqualTo),
        new("Less Than Or Equal To", ComparisonOperation.LessThanOrEqualTo)
    };
}