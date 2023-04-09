// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class DrawableAssociatedModule : Container
{
    public required string ModuleName { get; init; }
    public readonly BindableBool State = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        Height = 50;

        Children = new Drawable[]
        {
            new FillFlowContainer
            {
                Direction = FillDirection.Horizontal,
                RelativeSizeAxes = Axes.Both,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Child = new ToggleButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            State = State
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Child = new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 20))
                        {
                            RelativeSizeAxes = Axes.Both,
                            TextAnchor = Anchor.CentreLeft,
                            Text = ModuleName
                        }
                    }
                }
            }
        };
    }
}
