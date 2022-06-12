// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public class ModuleFilterOption : Container
{
    public Action<bool>? OnOptionChange { get; init; }

    public bool InitialState { get; init; }
    public string Text { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 50;
        Padding = new MarginPadding(5);

        Checkbox checkBox;
        Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 10,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray2
                },
                new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(5),
                    FillMode = FillMode.Fit,
                    Child = checkBox = new Checkbox
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = 7,
                        IconPadding = 5,
                        State = { Value = InitialState }
                    },
                },
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(5),
                    Child = new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = Text,
                        Colour = VRCOSCColour.GrayC
                    }
                }
            }
        };

        checkBox.State.BindValueChanged(e => OnOptionChange?.Invoke(e.NewValue));
    }
}
