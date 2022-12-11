// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Effects;
using osuTK;
using osuTK.Graphics;

// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Graphics;

public static class VRCOSCEdgeEffects
{
    public static readonly EdgeEffectParameters NoShadow = new()
    {
        Colour = new Color4(0, 0, 0, 0),
        Radius = 0,
        Type = EdgeEffectType.Shadow,
        Offset = Vector2.Zero
    };

    public static readonly EdgeEffectParameters BasicShadow = new()
    {
        Colour = Color4.Black.Opacity(0.6f),
        Radius = 2.5f,
        Type = EdgeEffectType.Shadow,
        Offset = new Vector2(0.0f, 1.5f)
    };

    public static readonly EdgeEffectParameters DispersedShadow = new()
    {
        Colour = Color4.Black.Opacity(0.75f),
        Radius = 15,
        Type = EdgeEffectType.Shadow,
        Offset = new Vector2(0)
    };
}
