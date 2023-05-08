// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Startup;

public partial class StartupContainer : Container
{
    [Resolved]
    private StartupManager startupManager { get; set; } = null!;

    private readonly StringTextBox textBox;

    protected override FillFlowContainer Content { get; }

    private readonly Bindable<string> filePath;

    public StartupContainer(Bindable<string> filePath)
    {
        this.filePath = filePath;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Padding = new MarginPadding
        {
            Horizontal = 20
        };

        InternalChildren = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreRight,
                Size = new Vector2(30, 60),
                Padding = new MarginPadding(5),
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Action = deleteSelf,
                    IconPadding = 4,
                    Circular = true,
                    BorderThickness = 2,
                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Failure],
                    Icon = FontAwesome.Solid.Get(0xf00d),
                    IconShadow = true
                }
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 10,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                BorderThickness = 2,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ThemeManager.Current[ThemeAttribute.Darker]
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(10),
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            Content = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 10),
                                Child = textBox = new StringTextBox
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 40,
                                    Masking = true,
                                    CornerRadius = 5,
                                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                                    BorderThickness = 2,
                                    Text = filePath.Value
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        textBox.OnValidEntry += text => filePath.Value = text;
    }

    private void deleteSelf()
    {
        startupManager.FilePaths.Remove(filePath);
        this.RemoveAndDisposeImmediately();
    }
}
