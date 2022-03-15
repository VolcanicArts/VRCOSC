using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;
using VRCOSC.Game.Graphics.Drawables.Triangles;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleCard : Container
{
    public Modules.Module SourceModule { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new Drawable[]
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
                    new SpriteText
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Font = FrameworkFont.Regular.With(size: 35),
                        Shadow = true,
                        ShadowColour = Colour4.Black.Opacity(0.5f),
                        ShadowOffset = new Vector2(0.0f, 0.025f),
                        Text = SourceModule.Title
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = FrameworkFont.Regular.With(size: 25),
                        Shadow = true,
                        ShadowColour = Colour4.Black.Opacity(0.5f),
                        ShadowOffset = new Vector2(0.0f, 0.025f),
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
                                Icon = FontAwesome.Solid.Edit
                            },
                            new ToggleCheckbox
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Size = new Vector2(50),
                                State = { Value = SourceModule.Data.Enabled },
                                ValueChange = (state => SourceModule.SetEnabled(state))
                            }
                        }
                    }
                }
            }
        };
    }
}
