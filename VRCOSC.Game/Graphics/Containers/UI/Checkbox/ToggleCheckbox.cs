// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.Containers.UI.Checkbox;

public class ToggleCheckbox : Container
{
    public Bindable<bool> State = new();
    public Action<bool> ValueChange;
    private Box ToggleColour;

    [BackgroundDependencyLoader]
    private void load()
    {
        State.BindValueChanged(_ =>
        {
            ToggleColour.Colour = getColourFromState();
            ValueChange?.Invoke(State.Value);
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
