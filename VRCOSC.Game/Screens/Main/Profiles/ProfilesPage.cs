// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Config;

namespace VRCOSC.Game.Screens.Main.Profiles;

public partial class ProfilesPage : Container
{
    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 8),
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new ProfilesToggle(configManager.GetBindable<bool>(VRCOSCSetting.AutomaticProfileSwitching), "Automatic Switching", "Automatic switching changes your selected profile to one that is linked to the avatar you’re wearing when you change avatar. If none is found, the default profile is used"),
                    new DefaultDropdownContainer()
                }
            }
        };
    }
}
