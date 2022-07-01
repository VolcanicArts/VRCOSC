// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Containers.UI.Button;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect.SidePanel;

public sealed class ModuleSelectSidePanel : Container
{
    private BindableBool runButtonEnableBindable = null!;

    public ModuleSelectSidePanel()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Depth = float.MinValue;
        Masking = true;
    }

    [BackgroundDependencyLoader]
    private void load(VRCOSCConfigManager configManager, ScreenManager screenManager)
    {
        IconButton runButton;
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray3
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 60)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new ModuleSelectSidePanelContent()
                    },
                    new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(7),
                            Child = runButton = new IconButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                CornerRadius = 10,
                                FillMode = FillMode.Fit,
                                FillAspectRatio = 4,
                                IconPadding = 5,
                                BackgroundColour = VRCOSCColour.GreenDark,
                                Icon = FontAwesome.Solid.Play,
                                Action = screenManager.ShowTerminal
                            }
                        }
                    }
                }
            }
        };

        runButtonEnableBindable = (BindableBool)configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);
        runButtonEnableBindable.BindValueChanged(e => runButton.Enabled.Value = !e.NewValue, true);
    }
}
