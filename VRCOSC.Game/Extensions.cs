// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game;

public static class EnumExtensions
{
    public static string ToLookup(this Enum key) => key.ToString().ToLowerInvariant();
}
