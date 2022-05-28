// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace VRCOSC.Game.Graphics.Containers.UI.Checkbox;

public class ToggleSwitch : Container
{
    public readonly BindableBool State = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Box background;

        Child = new CircularContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            Children = new Drawable[]
            {
                background = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both
                },
                new ClickableToggle
                {
                    State = State.Value,
                    OnClickAction = State.Toggle
                }
            }
        };

        State.BindValueChanged(e =>
        {
            background.Colour = e.NewValue ? Colour4.Green : Colour4.Red;
        }, true);
    }

    private class ClickableToggle : CircularContainer
    {
        public bool State { get; set; }
        public Action? OnClickAction { get; init; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.X;
            FillMode = FillMode.Fit;
            Size = new Vector2(0.8f);
            EdgeEffect = VRCOSCEdgeEffects.BasicShadow;
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White
                },
            };

            this.MoveToX(State ? 0.25f : -0.25f);
        }

        protected override bool OnClick(ClickEvent e)
        {
            OnClickAction?.Invoke();
            State = !State;
            updatePosition();
            return true;
        }

        private void updatePosition()
        {
            var newPosition = State ? 0.25f : -0.25f;
            this.MoveToX(newPosition, 100, Easing.InOutCirc);
        }
    }
}
