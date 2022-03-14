using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Drawables.UI.Buttons;

namespace VRCOSC.Game.Graphics.Containers.Modules;

public class ModuleContainer : Container
{
    public ModuleContainer()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.DarkGray
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new TextFlowContainer(t =>
                    {
                        t.Font = FrameworkFont.Regular.With(size: 40);
                        t.Shadow = true;
                        t.ShadowColour = Colour4.Black.Opacity(0.5f);
                        t.ShadowOffset = new Vector2(0.04f);
                    })
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        TextAnchor = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both,
                        Text = "Title"
                    },
                    new IconButton
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Size = new Vector2(50),
                        Masking = true,
                        CornerRadius = 10,
                        Colour = Colour4.Gray.Darken(0.5f),
                        Icon = FontAwesome.Solid.Check
                    },
                    new IconButton
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Size = new Vector2(50),
                        Masking = true,
                        CornerRadius = 10,
                        Colour = Colour4.Gray.Darken(0.5f),
                        Icon = FontAwesome.Solid.Edit
                    }
                }
            }
        };
    }
}
