using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Screens;

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

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = 50,
                Colour = VRCOSCColour.Gray6
            },
            new Container
            {
                Name = "Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new TextFlowContainer
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        TextAnchor = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.Both,
                        Text = SourceModule.Title
                    }
                }
            }
        };
    }

    protected override bool OnClick(ClickEvent e)
    {
        screenStack.Push(new ModuleScreen
        {
            SourceModule = SourceModule
        });
        return true;
    }
}
