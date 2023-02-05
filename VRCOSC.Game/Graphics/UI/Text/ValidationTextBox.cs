// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.UI.Text;

public abstract partial class ValidationTextBox<T> : VRCOSCTextBox
{
    public bool EmptyIsValid { get; init; } = true;

    private InvalidIcon invalidIcon = null!;

    public Action<T>? OnValidEntry;

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(invalidIcon = new InvalidIcon
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit
        });

        Current.BindValueChanged(e =>
        {
            if (validate(e.NewValue))
            {
                invalidIcon.Hide();
                OnValidEntry?.Invoke(GetConvertedText());
            }
            else
            {
                invalidIcon.Show();
            }
        }, true);
    }

    private bool validate(string text)
    {
        if (EmptyIsValid && string.IsNullOrEmpty(text)) return true;

        return IsTextValid(text);
    }

    protected abstract bool IsTextValid(string text);

    protected abstract T GetConvertedText();

    private partial class InvalidIcon : VisibilityContainer
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding(5);

            Child = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Colour4.White,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Icon = FontAwesome.Solid.ExclamationCircle,
                        Colour = Colour4.Red
                    },
                }
            };
        }

        protected override void PopIn()
        {
            Child.ScaleTo(1, 400, Easing.OutBounce);
        }

        protected override void PopOut()
        {
            Child.ScaleTo(0, 400, Easing.InQuart);
        }
    }
}
