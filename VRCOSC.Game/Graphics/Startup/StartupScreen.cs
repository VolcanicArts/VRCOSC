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

namespace VRCOSC.Game.Graphics.Startup;

public partial class StartupScreen : Container
{
    [Resolved]
    private StartupManager startupManager { get; set; } = null!;

    private FillFlowContainer listFlow = null!;
    private TextFlowContainer textFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Light]
            },
            new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ClampExtension = 20,
                ScrollbarVisible = false,
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0, 5),
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(5),
                    Children = new Drawable[]
                    {
                        textFlow = new TextFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                        listFlow = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(10),
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5f)
                        },
                        new IconButton
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(0.5f, 30),
                            Icon = FontAwesome.Solid.Plus,
                            Masking = true,
                            Circular = true,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Accent],
                            Action = addComponent
                        }
                    }
                }
            }
        };

        textFlow.AddText("Startup Screen", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 35);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });

        textFlow.AddParagraph("Here you can define exe paths for VRCOSC to automatically startup on module run. For example, Spotify", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 25);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });
    }

    protected override void LoadComplete()
    {
        startupManager.FilePaths.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (Bindable<string> newItem in e.NewItems)
                {
                    listFlow.Add(new StartupContainer(newItem));
                }
            }
        }, true);
    }

    private void addComponent()
    {
        startupManager.FilePaths.Add(new Bindable<string>(string.Empty));
    }
}
