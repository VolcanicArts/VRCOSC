using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osuTK;

namespace VRCOSC.Game.Graphics.EdgeEffects;

public static class BasicEdgeEffects
{
    public static readonly EdgeEffectParameters NoShadow = new()
    {
        Colour = new Colour4(0, 0, 0, 0),
        Radius = 0,
        Type = EdgeEffectType.Shadow,
        Offset = Vector2.Zero
    };

    public static readonly EdgeEffectParameters BasicShadow = new()
    {
        Colour = Colour4.Black,
        Radius = 2.5f,
        Type = EdgeEffectType.Shadow,
        Offset = new Vector2(0.0f, 1.5f)
    };
}
