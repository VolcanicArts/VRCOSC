using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;
using VRCOSC.Game.Graphics.Drawables.Triangles;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleCard : Container
{
    [Resolved]
    private ScreenStack screenStack { get; set; }

    public Modules.Module SourceModule { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        CornerRadius = 10;
        EdgeEffect = VRCOSCEdgeEffects.BasicShadow;
        BorderThickness = 3;

        Children = new Drawable[]
        {
            new TrianglesBackground
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = 75,
                ColourLight = SourceModule.Colour,
                ColourDark = SourceModule.Colour.Darken(0.25f)
            },
            new Container
            {
                Name = "Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Horizontal = 10,
                    Vertical = 7
                },
                Children = new Drawable[]
                {
                    new TextFlowContainer(t =>
                    {
                        t.Font = FrameworkFont.Regular.With(size: 35);
                        t.Shadow = true;
                        t.ShadowColour = Colour4.Black.Opacity(0.5f);
                        t.ShadowOffset = new Vector2(0.0f, 0.025f);
                    })
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        TextAnchor = Anchor.TopLeft,
                        AutoSizeAxes = Axes.Both,
                        Text = SourceModule.Title
                    },
                    new TextFlowContainer(t =>
                    {
                        t.Font = FrameworkFont.Regular.With(size: 25);
                        t.Shadow = true;
                        t.ShadowColour = Colour4.Black.Opacity(0.5f);
                        t.ShadowOffset = new Vector2(0.0f, 0.025f);
                    })
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        TextAnchor = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.Both,
                        Text = SourceModule.Description
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            new IconButton
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Size = new Vector2(50),
                                Icon = FontAwesome.Solid.Edit,
                                Action = OnEditClick
                            },
                            new ToggleCheckbox
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Size = new Vector2(50),
                                State = SourceModule.Enabled.GetBoundCopy()
                            }
                        }
                    }
                }
            }
        };
    }

    private void OnEditClick()
    {
        screenStack.Push(new ModuleScreen
        {
            SourceModule = SourceModule
        });
    }
}
