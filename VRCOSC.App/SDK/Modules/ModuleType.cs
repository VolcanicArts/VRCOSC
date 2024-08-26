// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Media;

namespace VRCOSC.App.SDK.Modules;

public enum ModuleType
{
    Generic,
    Health,
    Integrations,
    SteamVR,
    NSFW
}

public static class ModuleTypeExtensions
{
    public static Brush ToColour(this ModuleType type) => type switch
    {
        ModuleType.Generic => Brushes.White,
        ModuleType.Health => (Brush)Application.Current.FindResource("CRed"),
        ModuleType.Integrations => (Brush)Application.Current.FindResource("CYellow"),
        ModuleType.SteamVR => Brushes.Purple,
        ModuleType.NSFW => Brushes.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
