// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Button;
using VRCOSC.Game.Graphics.Drawables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class ModuleEditContent : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    [Resolved]
    private Bindable<Module> SourceModule { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        TextFlowContainer metadataTextFlow;
        LineSeparator settingsSeparator;
        Container<AttributeFlow> settingsFlowContainer;
        LineSeparator outputParametersSeparator;
        Container<AttributeFlow> outputParametersFlowContainer;

        BasicScrollContainer scrollContainer;

        Children = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Size = new Vector2(80),
                Padding = new MarginPadding(10),
                Depth = float.MinValue,
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 10,
                    Icon = { Value = FontAwesome.Solid.Get(0xf00d) },
                    Action = ScreenManager.FinishEditingModule
                },
            },
            scrollContainer = new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                ClampExtension = 20,
                Padding = new MarginPadding
                {
                    Horizontal = 26
                },
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        metadataTextFlow = new TextFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.Centre,
                            AutoSizeAxes = Axes.Both
                        },
                        settingsSeparator = new LineSeparator
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(0.95f, 5)
                        },
                        settingsFlowContainer = new Container<AttributeFlow>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                        outputParametersSeparator = new LineSeparator
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(0.95f, 5)
                        },
                        outputParametersFlowContainer = new Container<AttributeFlow>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                    }
                }
            }
        };

        SourceModule.BindValueChanged(_ =>
        {
            if (SourceModule.Value == null) return;

            scrollContainer.ScrollToStart();

            metadataTextFlow.Clear();
            metadataTextFlow.AddText(SourceModule.Value.Title, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 75);
            });
            metadataTextFlow.AddParagraph(SourceModule.Value.Description, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 40);
                t.Colour = VRCOSCColour.Gray9;
            });
            metadataTextFlow.AddParagraph("Made by ", t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 30);
                t.Colour = VRCOSCColour.Gray9;
            });
            metadataTextFlow.AddText(SourceModule.Value.Author, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 30);
                t.Colour = VRCOSCColour.GrayE;
            });

            settingsFlowContainer.Child = new AttributeFlow
            {
                Title = "Settings",
                AttributesList = SourceModule.Value.Settings.Values.ToList()
            };

            outputParametersFlowContainer.Child = new AttributeFlow
            {
                Title = "Output Parameters",
                AttributesList = SourceModule.Value.OutputParameters.Values.ToList()
            };

            if (SourceModule.Value.HasSettings)
            {
                settingsSeparator.Show();
                settingsFlowContainer.Show();
            }
            else
            {
                settingsSeparator.Hide();
                settingsFlowContainer.Hide();
            }

            if (SourceModule.Value.HasOutputParameters)
            {
                outputParametersSeparator.Show();
                outputParametersFlowContainer.Show();
            }
            else
            {
                outputParametersSeparator.Hide();
                outputParametersFlowContainer.Hide();
            }
        }, true);
    }
}
