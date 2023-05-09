// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Run;

public partial class RunScreenFooter : Container
{
    private IconButton startButton = null!;
    private IconButton stopButton = null!;
    private IconButton restartButton = null!;

    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 3;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(8),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(8, 0),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                FillAspectRatio = 3,
                                Child = startButton = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Icon = FontAwesome.Solid.Play,
                                    IconShadow = true,
                                    Masking = true,
                                    CornerRadius = 10,
                                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Success],
                                    Action = gameManager.Start
                                }
                            }
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(7, 0),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                FillAspectRatio = 3,
                                Child = stopButton = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Icon = FontAwesome.Solid.Stop,
                                    IconShadow = true,
                                    Masking = true,
                                    CornerRadius = 10,
                                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Failure],
                                    Action = gameManager.Stop
                                }
                            },
                            new Container
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                FillAspectRatio = 3,
                                Child = restartButton = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Icon = FontAwesome.Solid.Redo,
                                    IconShadow = true,
                                    Masking = true,
                                    CornerRadius = 10,
                                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                                    Action = gameManager.Restart
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        gameManager.State.BindValueChanged(_ =>
        {
            restartButton.Enabled.Value = gameManager.State.Value == GameManagerState.Started;
            stopButton.Enabled.Value = gameManager.State.Value == GameManagerState.Started;
            startButton.Enabled.Value = gameManager.State.Value == GameManagerState.Stopped;
        }, true);
    }
}
