// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class DrawableEvent : Container
{
    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    private readonly ClipEvent clipEvent;

    public DrawableEvent(ClipEvent clipEvent)
    {
        this.clipEvent = clipEvent;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        IntTextBox lengthTextBox;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Light],
                RelativeSizeAxes = Axes.Both
            },
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(3),
                Spacing = new Vector2(0, 2),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Child = new ToggleButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    State = clipEvent.Enabled.GetBoundCopy()
                                }
                            },
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(3),
                                Child = new SpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = FrameworkFont.Regular.With(size: 20),
                                    Text = gameManager.ModuleManager.GetModuleName(clipEvent.Module) + " - " + clipEvent.Name + ":",
                                    Colour = ThemeManager.Current[ThemeAttribute.Text]
                                }
                            }
                        }
                    },
                    new GridContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        ColumnDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 3),
                            new Dimension(GridSizeMode.Relative, 0.1f)
                        },
                        Content = new[]
                        {
                            new Drawable?[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = new VRCOSCTextBox
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Current = clipEvent.Format.GetBoundCopy(),
                                        Masking = true,
                                        CornerRadius = 5
                                    }
                                },
                                null,
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = lengthTextBox = new IntTextBox
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Text = clipEvent.Length.Value.ToString(),
                                        Masking = true,
                                        CornerRadius = 5,
                                        PlaceholderText = "Length"
                                    }
                                },
                            }
                        }
                    }
                }
            }
        };

        lengthTextBox.OnValidEntry = newLength => clipEvent.Length.Value = newLength;
    }
}
