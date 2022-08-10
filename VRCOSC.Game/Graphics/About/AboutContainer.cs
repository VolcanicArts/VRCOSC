// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.About;

public class AboutContainer : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        TextFlowContainer textFlow;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray5
            },
            textFlow = new TextFlowContainer
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                TextAnchor = Anchor.TopLeft,
                RelativeSizeAxes = Axes.Both,
                ParagraphSpacing = 0.75f,
                Padding = new MarginPadding(5)
            }
        };

        textFlow.AddText("VRCOSC", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 40);
        });
        textFlow.AddParagraph("Created and maintained by VolcanicArts");
        textFlow.AddParagraph("Repo URL", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
        });
        textFlow.AddParagraph("https://github.com/VolcanicArts/VRCOSC");
        textFlow.AddParagraph("Discord Server", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
        });
        textFlow.AddParagraph("https://discord.gg/vj4brHyvT5");
    }
}
