using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.Containers.UI.Checkbox;

public class ToggleCheckbox : Container
{
    public Bindable<bool> State { get; init; }
    private Box ToggleColour;

    [BackgroundDependencyLoader]
    private void load()
    {
        State.BindValueChanged(_ =>
        {
            ToggleColour.Colour = getColourFromState();
        });
        Child = new CircularContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Masking = true,
            BorderThickness = 3,
            BorderColour = Colour4.Black,
            RelativeSizeAxes = Axes.Both,
            Child = new ClickableContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Action = () => State.Value = !State.Value,
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
                        Padding = new MarginPadding(8),
                        Child = new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Child = ToggleColour = new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = getColourFromState()
                            }
                        }
                    }
                }
            }
        };
    }

    private Colour4 getColourFromState()
    {
        return State.Value ? VRCOSCColour.Green : VRCOSCColour.Red;
    }
}
