// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Effects;
using osuTK;
using osuTK.Graphics;
using VRCOSC.Game.Graphics.Themes;

// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Graphics;

public static class VRCOSCEdgeEffects
{
    public static EdgeEffectParameters NoShadow => new()
    {
        Colour = new Color4(0, 0, 0, 0),
        Radius = 0f,
        Type = EdgeEffectType.Shadow,
        Offset = Vector2.Zero
    };

    public static EdgeEffectParameters BasicShadow => new()
    {
        Colour = ThemeManager.Current[ThemeAttribute.Darker].Opacity(0.6f),
        Radius = 2.5f,
        Type = EdgeEffectType.Shadow,
        Offset = new Vector2(0.0f, 1.5f)
    };

    public static EdgeEffectParameters UniformShadow => new()
    {
        Colour = ThemeManager.Current[ThemeAttribute.Darker].Opacity(0.6f),
        Radius = 5f,
        Type = EdgeEffectType.Shadow,
        Offset = new Vector2(0f),
        Hollow = false
    };

    public static EdgeEffectParameters DispersedShadow => new()
    {
        Colour = ThemeManager.Current[ThemeAttribute.Darker].Opacity(0.75f),
        Radius = 15f,
        Type = EdgeEffectType.Shadow,
        Offset = new Vector2(0f)
    };
}
