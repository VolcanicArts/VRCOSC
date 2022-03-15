using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Drawables;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditScreen;

public class ModuleEditInnerContainer : Container
{
    public Modules.Module SourceModule { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChild = new BasicScrollContainer
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
                Spacing = new Vector2(0, 10),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new FillFlowContainer<SpriteText>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new SpriteText[]
                        {
                            new()
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Font = FrameworkFont.Regular.With(size: 75),
                                Text = SourceModule.Title
                            },
                            new()
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Font = FrameworkFont.Regular.With(size: 40),
                                Colour = VRCOSCColour.Gray9,
                                Text = SourceModule.Description
                            },
                        }
                    },
                    new LineSeparator
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(0.95f, 5)
                    },
                    new ModuleEditSettingsContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        SourceModule = SourceModule
                    },
                    new LineSeparator
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(0.95f, 5)
                    },
                    new ModuleEditParametersContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        SourceModule = SourceModule
                    }
                }
            }
        };
    }
}
