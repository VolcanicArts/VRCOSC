using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using VRCOSC.Game.Graphics.Containers;

namespace VRCOSC.Game.Tests.Visual;

public class TestSenderFlowContainer : VRCOSCTestScene
{
    private SenderFlowContainer senderFlowContainer;

    [SetUpSteps]
    public void SetUp()
    {
        Add(senderFlowContainer = new SenderFlowContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding(20)
        });
    }

    [Test]
    public void TestAddSenderContainer()
    {
        AddStep("Add Sender Container", () => senderFlowContainer.AddSender());
    }
}
