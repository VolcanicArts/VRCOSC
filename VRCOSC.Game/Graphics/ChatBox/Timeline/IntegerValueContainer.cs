// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class IntegerValueContainer : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Padding = new MarginPadding(5);

        Children = new Drawable[]
        {
            new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(),
                    new Dimension(),
                    new Dimension(),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Icon = FontAwesome.Solid.AngleDoubleLeft,
                            Circular = true,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Darker],
                            Action = () => chatBoxManager.DecreaseTime(5)
                        },
                        new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Icon = FontAwesome.Solid.AngleLeft,
                            Circular = true,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Darker],
                            Action = () => chatBoxManager.DecreaseTime(1)
                        },
                        new IntegerDisplay(),
                        new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Icon = FontAwesome.Solid.AngleRight,
                            Circular = true,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Darker],
                            Action = () => chatBoxManager.IncreaseTime(1)
                        },
                        new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Icon = FontAwesome.Solid.AngleDoubleRight,
                            Circular = true,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Darker],
                            Action = () => chatBoxManager.IncreaseTime(5)
                        }
                    }
                }
            }
        };
    }

    private partial class IntegerDisplay : Container
    {
        [Resolved]
        private ChatBoxManager chatBoxManager { get; set; } = null!;

        private SpriteText text = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
            Masking = true;
            CornerRadius = 10;
            BorderThickness = 2;
            BorderColour = ThemeManager.Current[ThemeAttribute.Border];

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = ThemeManager.Current[ThemeAttribute.Darker],
                    RelativeSizeAxes = Axes.Both
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Padding = new MarginPadding(5),
                    Child = text = new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = FrameworkFont.Regular.With(size: 20),
                        Colour = ThemeManager.Current[ThemeAttribute.Text]
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            chatBoxManager.TimelineLength.BindValueChanged(_ => text.Text = chatBoxManager.TimelineLengthSeconds.ToString(), true);
        }
    }
}
