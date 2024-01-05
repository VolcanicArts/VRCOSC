// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace VRCOSC.Graphics.UI;

public partial class IconButton : ClickableContainer
{
    public Color4 BackgroundColour { get; set; } = Color4.Black;
    public IconUsage Icon { get; set; } = FontAwesome.Regular.Angry;
    public float IconSize { get; set; } = 20;
    public Color4 IconColour { get; set; } = Color4.White;
    public new float CornerRadius = 0;

    private Box background = null!;
    private SpriteIcon icon = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Enabled.Value = true;

        InternalChild = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            BorderThickness = 3,
            BorderColour = BackgroundColour,
            CornerRadius = CornerRadius,
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = BackgroundColour,
                    Alpha = 0,
                    AlwaysPresent = true
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = Icon,
                    Size = new Vector2(IconSize),
                    Colour = IconColour,
                    Shadow = true,
                    ShadowColour = Colours.BLACK.Opacity(0.75f),
                    ShadowOffset = new Vector2(0, 1)
                }
            }
        };

        Enabled.BindValueChanged(onEnabledChange, true);
    }

    private void onEnabledChange(ValueChangedEvent<bool> e) => Scheduler.Add(() =>
    {
        InternalChild.FadeTo(e.NewValue ? 1f : 0.25f, 250, Easing.OutQuint);

        if (e.NewValue)
        {
            if (IsHovered)
                fadeInBackground();
            else
                fadeOutBackground();
        }
        else
        {
            fadeOutBackground();
        }
    });

    protected override bool OnHover(HoverEvent e)
    {
        if (!Enabled.Value) return true;

        fadeInBackground();
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        if (!Enabled.Value) return;

        if (e.IsPressed(MouseButton.Left)) return;

        fadeOutBackground();
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (!Enabled.Value) return true;

        if (e.Button != MouseButton.Left) return true;

        InternalChild.ScaleTo(0.95f, 500, Easing.OutQuart);
        return true;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        if (!Enabled.Value) return;

        if (e.Button != MouseButton.Left) return;

        InternalChild.ScaleTo(1f, 500, Easing.OutQuart);

        if (!IsHovered)
        {
            fadeOutBackground();
        }
    }

    private void fadeInBackground()
    {
        background.FadeIn(100, Easing.OutQuart);
        icon.TransformTo(nameof(SpriteText.ShadowOffset), new Vector2(0, 0.05f));
    }

    private void fadeOutBackground()
    {
        background.FadeOut(100, Easing.OutQuart);
        icon.TransformTo(nameof(SpriteText.ShadowOffset), Vector2.Zero);
    }
}
