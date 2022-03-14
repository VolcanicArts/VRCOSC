using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.Containers.Terminal;

namespace VRCOSC.Game.Tests.Visual;

public class TestTerminalContainerScene : VRCOSCTestScene
{
    private TerminalContainer TerminalContainer;

    public TestTerminalContainerScene()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray5
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = TerminalContainer = new TerminalContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 20
                }
            }
        };
    }

    [Test]
    public void TestTextLogging()
    {
        AddStep("Add nominal text", () => TerminalContainer.Log("This is some text"));
        AddStep("Add important text", () => TerminalContainer.Log("This is important text", LogState.Important));
        AddStep("Add error text", () => TerminalContainer.Log("This is an error", LogState.Error));
    }

    [Test]
    public void TestClearTerminalFlow()
    {
        AddStep("Clear terminal", () => TerminalContainer.ClearTerminal());
    }
}
