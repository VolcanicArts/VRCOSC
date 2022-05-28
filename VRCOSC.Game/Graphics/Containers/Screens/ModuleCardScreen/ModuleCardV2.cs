// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;
using VRCOSC.Game.Graphics.Drawables.Triangles;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public sealed class ModuleCardV2 : Container
{
    private const float active_alpha = 1.0f;
    private const float inactive_alpha = 0.4f;
    private static readonly EdgeEffectParameters active_edge_effect = VRCOSCEdgeEffects.BasicShadow;
    private static readonly EdgeEffectParameters inactive_edge_effect = VRCOSCEdgeEffects.NoShadow;

    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    public Modules.Module SourceModule { get; init; }

    public ModuleCardV2()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        Size = new Vector2(350, 200);
        Masking = true;
        CornerRadius = 10;
        EdgeEffect = VRCOSCEdgeEffects.BasicShadow;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ToggleSwitch toggleCheckBox;
        Children = new Drawable[]
        {
            new TrianglesBackground
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColourLight = SourceModule.Colour,
                ColourDark = SourceModule.Colour.Darken(0.25f)
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 35),
                    new Dimension(GridSizeMode.Absolute, 100),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = FrameworkFont.Regular.With(size: 35),
                            Shadow = true,
                            Text = SourceModule.Title
                        }
                    },
                    new Drawable[]
                    {
                        new TextFlowContainer(t =>
                        {
                            t.Font = FrameworkFont.Regular.With(size: 25);
                            t.Shadow = true;
                        })
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both,
                            TextAnchor = Anchor.TopCentre,
                            Padding = new MarginPadding(5),
                            Text = SourceModule.Description
                        },
                    },
                    new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(5),
                            Children = new Drawable[]
                            {
                                new IconButton
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    Icon = FontAwesome.Solid.Edit,
                                    FillMode = FillMode.Fit,
                                    CornerRadius = 5,
                                    Action = () => ScreenManager.EditModule(SourceModule)
                                },
                                toggleCheckBox = new ToggleSwitch
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                    FillAspectRatio = 2,
                                    State = { Value = SourceModule.DataManager.Enabled }
                                }
                            }
                        },
                    }
                }
            }
        };

        Alpha = (toggleCheckBox.State.Value) ? active_alpha : inactive_alpha;
        EdgeEffect = (toggleCheckBox.State.Value) ? active_edge_effect : inactive_edge_effect;

        toggleCheckBox.State.BindValueChanged(e =>
        {
            SourceModule.DataManager.SetEnabled(e.NewValue);

            const float transition_duration = 500;

            if (e.NewValue)
            {
                this.FadeTo(active_alpha, transition_duration, Easing.OutCubic);
                TweenEdgeEffectTo(active_edge_effect, transition_duration, Easing.OutCubic);
            }
            else
            {
                this.FadeTo(inactive_alpha, transition_duration, Easing.OutCubic);
                TweenEdgeEffectTo(inactive_edge_effect, transition_duration, Easing.OutCubic);
            }
        });
    }
}
