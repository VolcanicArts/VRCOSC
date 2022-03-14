using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;

namespace VRCOSC.Game.Graphics.Containers;

public class MainScreenFooter : Container
{
    public MainScreenFooter()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray5
            },
            new Container
            {
                Name = "Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Children = new Drawable[]
                {
                    new TextButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(150, 40),
                        CornerRadius = 5,
                        BackgroundColour = VRCOSCColour.GreenDark,
                        Text = "Run"
                    }
                }
            }
        };
    }
}
