// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using NUnit.Framework;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Tests.Visual;

public class TestLoadingSpriteText : VRCOSCTestScene
{
    private LoadingSpriteText spriteText = null!;

    [SetUp]
    public void SetUp()
    {
        Clear();
        Add(spriteText = new LoadingSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        });
    }

    [Test]
    public void TestLoading()
    {
        AddStep("set text", () => spriteText.CurrentText.Value = "This is a test");
        AddAssert("ensure animation is playing", () => spriteText.ShouldAnimate.Value);
        AddStep("pause animation", () => spriteText.ShouldAnimate.Value = false);
        AddAssert("ensure animation is stopped", () => !spriteText.ShouldAnimate.Value);
        AddToggleStep("toggle animation", e => spriteText.ShouldAnimate.Value = e);
    }
}
