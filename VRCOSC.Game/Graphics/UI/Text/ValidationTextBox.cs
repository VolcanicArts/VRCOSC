// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace VRCOSC.Game.Graphics.UI.Text;

public abstract partial class ValidationTextBox<T> : TextBox
{
    /// <summary>
    /// This bindable gets updated whenever there has been a valid input. Bind to this for only valid inputs
    /// </summary>
    public Bindable<T> ValidCurrent { get; set; } = new();

    private InvalidIcon invalidIcon = null!;

    public bool IsCurrentValid { get; private set; }

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

        ValidCurrent.BindValueChanged(e => Current.Value = e.NewValue?.ToString(), true);

        Current.BindValueChanged(e =>
        {
            if (IsTextValid(e.NewValue))
            {
                invalidIcon.Hide();
                var value = GetConvertedText();
                ValidCurrent.Value = value;
                IsCurrentValid = true;
            }
            else
            {
                invalidIcon.Show();
                IsCurrentValid = false;
            }
        }, true);
    }

    /// <summary>
    /// This is where you should apply validation on the raw string from <see cref="osu.Framework.Graphics.UserInterface.TextBox.Current"/>
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    protected abstract bool IsTextValid(string text);

    /// <summary>
    /// This should convert <see cref="osu.Framework.Graphics.UserInterface.TextBox.Current"/> entries to T
    /// </summary>
    /// <returns></returns>
    protected abstract T GetConvertedText();

    private partial class InvalidIcon : VisibilityContainer
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding(5);

            InternalChild = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colours.RED0
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.7f),
                        Icon = FontAwesome.Solid.Exclamation,
                        Colour = Colours.WHITE0,
                    }
                }
            };
        }

        protected override void PopIn()
        {
            InternalChild.ScaleTo(1, 400, Easing.OutBounce);
        }

        protected override void PopOut()
        {
            InternalChild.ScaleTo(0, 400, Easing.InQuart);
        }
    }
}
