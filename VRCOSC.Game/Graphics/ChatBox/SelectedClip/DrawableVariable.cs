// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class DrawableVariable : Container
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    private readonly ClipVariableMetadata clipVariable;

    public DrawableVariable(ClipVariableMetadata clipVariable)
    {
        this.clipVariable = clipVariable;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];

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
                Padding = new MarginPadding(3),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Width = 0.5f,
                        Padding = new MarginPadding(2),
                        Child = new SpriteText
                        {
                            Font = FrameworkFont.Regular.With(size: 20),
                            Text = clipVariable.Name + ":",
                            Colour = ThemeManager.Current[ThemeAttribute.Text]
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new LocalTextBox
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                Height = 25,
                                CornerRadius = 5,
                                Text = clipVariable.DisplayableFormat,
                                ReadOnly = true
                            },
                            new Container
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Padding = new MarginPadding(3),
                                Child = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Icon = FontAwesome.Solid.Copy,
                                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                                    IconShadow = true,
                                    IconPadding = 4,
                                    Circular = true,
                                    Action = () => host.GetClipboard()?.SetText(clipVariable.DisplayableFormat)
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private partial class LocalTextBox : VRCOSCTextBox
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundUnfocused = ThemeManager.Current[ThemeAttribute.Mid];
            BackgroundFocused = ThemeManager.Current[ThemeAttribute.Mid];
        }
    }
}
