// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osuTK;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Game.Tests.Visual;

public class TestProgressBar : VRCOSCTestScene
{
    private ProgressBar progressBar;

    [SetUp]
    public void SetUp()
    {
        Clear();
        Add(progressBar = new ProgressBar
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(200, 20),
            Masking = true,
            CornerRadius = 5,
            BorderThickness = 2
        });
    }

    [Test]
    public void TestProgress()
    {
        AddSliderStep("set progress", 0f, 1f, 0f, v =>
        {
            if (progressBar != null)
                progressBar.Progress.Value = v;
        });
    }
}
