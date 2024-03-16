// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Media;

namespace VRCOSC.App.Modules;

public enum ModuleType
{
    Generic,
    Integrations,
    Health,
    NSFW
}

public static class ModuleTypeExtensions
{
    public static Brush ToColour(this ModuleType type) => type switch
    {
        ModuleType.Generic => Brushes.White,
        ModuleType.Integrations => (Brush)Application.Current.FindResource("CYellow"),
        ModuleType.Health => (Brush)Application.Current.FindResource("CRed"),
        ModuleType.NSFW => Brushes.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
