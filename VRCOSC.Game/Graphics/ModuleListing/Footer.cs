// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class Footer : Container
{
    private Bindable<bool> autoStartStop = null!;

    public Footer()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding
        {
            Top = 5
        };
    }

    [BackgroundDependencyLoader]
    private void load(VRCOSCConfigManager configManager, VRCOSCGame game)
    {
        TextButton runButton;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray4
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillAspectRatio = 4,
                Child = runButton = new TextButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.75f),
                    Masking = true,
                    CornerRadius = 5,
                    Text = "Run",
                    BackgroundColour = VRCOSCColour.Green,
                    Action = () => game.ModulesRunning.Value = true
                }
            }
        };

        autoStartStop = configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);
        autoStartStop.BindValueChanged(e => runButton.Enabled.Value = !e.NewValue, true);
    }
}
