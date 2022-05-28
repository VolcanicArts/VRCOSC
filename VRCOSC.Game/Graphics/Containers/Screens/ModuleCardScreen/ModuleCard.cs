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

public class ModuleCard : Container
{
    private const float active_alpha = 1.0f;
    private const float inactive_alpha = 0.4f;
    private static readonly EdgeEffectParameters active_edge_effect = VRCOSCEdgeEffects.BasicShadow;
    private static readonly EdgeEffectParameters inactive_edge_effect = VRCOSCEdgeEffects.NoShadow;

    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    public Modules.Module SourceModule { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 20;

        ToggleSwitch toggleCheckBox;

        InternalChildren = new Drawable[]
        {
            new TrianglesBackground
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = 75,
                ColourLight = SourceModule.Colour,
                ColourDark = SourceModule.Colour.Darken(0.25f)
            },
            new Container
            {
                Name = "Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Horizontal = 10,
                    Vertical = 7
                },
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Font = FrameworkFont.Regular.With(size: 35),
                        Shadow = true,
                        Text = SourceModule.Title
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = FrameworkFont.Regular.With(size: 25),
                        Shadow = true,
                        Text = SourceModule.Description
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Padding = new MarginPadding(5),
                                Child = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Icon = FontAwesome.Solid.Edit,
                                    CornerRadius = 10,
                                    Action = () => ScreenManager.EditModule(SourceModule)
                                }
                            },
                            toggleCheckBox = new ToggleSwitch
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.8f),
                                FillMode = FillMode.Fit,
                                FillAspectRatio = 2,
                                State = { Value = SourceModule.DataManager.Enabled }
                            }
                        }
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
