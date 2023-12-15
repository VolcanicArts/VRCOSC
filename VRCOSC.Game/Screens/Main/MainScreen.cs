// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.Screens.Main.Tabs;

namespace VRCOSC.Screens.Main;

public partial class MainScreen : Container
{
    private const int tab_bar_size = 70;
    private BufferedContainer bufferedContainer = null!;

    public bool ShouldBlur;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        AddInternal(bufferedContainer = new BufferedContainer
        {
            RelativeSizeAxes = Axes.Both,
            BackgroundColour = Colours.BLACK,
            Children = new Drawable[]
            {
                new TabContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Left = tab_bar_size
                    }
                },
                new TabBar
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = tab_bar_size
                }
            }
        });
    }

    protected override void Update()
    {
        bufferedContainer.TransformTo(nameof(BufferedContainer.BlurSigma), ShouldBlur ? new Vector2(5) : new Vector2(0), 500);
    }
}
