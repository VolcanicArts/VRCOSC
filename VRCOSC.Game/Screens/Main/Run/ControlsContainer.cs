// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class ControlsContainer : Container
{
    private IconButton startButton = null!;
    private IconButton stopButton = null!;
    private IconButton restartButton = null!;

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY7
            },
            new FillFlowContainer
            {
                Name = "Left Flow",
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    startButton = new IconButton
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = 150,
                        BackgroundColour = Colours.GREEN0,
                        Icon = FontAwesome.Solid.Play,
                        Masking = true,
                        CornerRadius = 10,
                        Action = () => appManager.ModuleManager.Start()
                    }
                }
            },
            new FillFlowContainer
            {
                Name = "Right Flow",
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    stopButton = new IconButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = 150,
                        BackgroundColour = Colours.RED0,
                        Icon = FontAwesome.Solid.Stop,
                        Masking = true,
                        CornerRadius = 10,
                        Action = () => appManager.ModuleManager.Stop()
                    },
                    restartButton = new IconButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = 150,
                        BackgroundColour = Colours.BLUE0,
                        Icon = FontAwesome.Solid.Redo,
                        Masking = true,
                        CornerRadius = 10,
                        Action = () => appManager.ModuleManager.Restart()
                    }
                }
            }
        };

        appManager.ModuleManager.State.BindValueChanged(onModuleManagerStateChange);
    }

    private void onModuleManagerStateChange(ValueChangedEvent<ModuleManagerState> e)
    {
        switch (e.NewValue)
        {
            case ModuleManagerState.Starting:
                startButton.Enabled.Value = false;
                break;

            case ModuleManagerState.Started:
                restartButton.Enabled.Value = true;
                stopButton.Enabled.Value = true;
                break;

            case ModuleManagerState.Stopping:
                restartButton.Enabled.Value = true;
                stopButton.Enabled.Value = true;
                break;

            case ModuleManagerState.Stopped:
                startButton.Enabled.Value = true;
                break;
        }
    }
}
