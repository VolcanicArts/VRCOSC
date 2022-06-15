// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Containers.UI.Button;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public class ModuleSelectionSidePanel : Container
{
    [Resolved]
    private ScreenManager screenManager { get; set; }

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; }

    private BindableBool runButtonEnableBindable;

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        EdgeEffect = new EdgeEffectParameters
        {
            Colour = Color4.Black.Opacity(0.6f),
            Radius = 5f,
            Type = EdgeEffectType.Shadow
        };

        runButtonEnableBindable = (BindableBool)configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);

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
                        new ModuleFilter
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both
                        }
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
                                BackgroundColour = { Value = VRCOSCColour.GreenDark },
                                Icon = { Value = FontAwesome.Solid.Play },
                                Action = screenManager.ShowTerminal
                            }
                        }
                    }
                }
            }
        };

        runButtonEnableBindable.BindValueChanged(e => runButton.Enabled.Value = !e.NewValue, true);
    }
}
