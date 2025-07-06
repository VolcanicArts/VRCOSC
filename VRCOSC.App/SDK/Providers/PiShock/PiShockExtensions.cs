// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Providers.PiShock;

public static class PiShockExtensions
{
    internal static string ToCode(this PiShockMode mode) => mode switch
    {
        PiShockMode.Shock => "s",
        PiShockMode.Vibrate => "v",
        PiShockMode.Beep => "b",
        _ => throw new ArgumentOutOfRangeException()
    };
}