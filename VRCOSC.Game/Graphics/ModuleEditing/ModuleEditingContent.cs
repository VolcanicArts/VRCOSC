// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Drawables;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public class ModuleEditingContent : Container
{
    private TextFlowContainer metadataTextFlow = null!;
    private Container settings = null!;
    private AttributeFlow settingsFlowContainer = null!;
    private Container parameters = null!;
    private AttributeFlow outputParametersFlowContainer = null!;
    private BasicScrollContainer scrollContainer = null!;

    [BackgroundDependencyLoader]
    private void load(VRCOSCGame game, Bindable<Module?> sourceModule)
    {
        Children = new Drawable[]
        {
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
                        settings = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new LineSeparator
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre
                                },
                                settingsFlowContainer = new AttributeFlow("Settings"),
                            }
                        },
                        parameters = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new LineSeparator
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre
                                },
                                outputParametersFlowContainer = new AttributeFlow("Parameters")
                            }
                        }
                    }
                }
            },
            new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Size = new Vector2(80),
                Padding = new MarginPadding(10),
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 10,
                    Icon = FontAwesome.Solid.Get(0xf00d),
                    Action = () => game.EditingModule.Value = null
                },
            }
        };

        sourceModule.BindValueChanged(_ =>
        {
            if (sourceModule.Value is null) return;

            metadataTextFlow.AddText(sourceModule.Value.Title, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 75);
            });
            metadataTextFlow.AddParagraph(sourceModule.Value.Description, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 40);
                t.Colour = VRCOSCColour.Gray9;
            });
            metadataTextFlow.AddParagraph("Made by ", t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 30);
                t.Colour = VRCOSCColour.Gray9;
            });
            metadataTextFlow.AddText(sourceModule.Value.Author, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 30);
                t.Colour = VRCOSCColour.GrayE;
            });

            settingsFlowContainer.AttributesList.Clear();
            settingsFlowContainer.AttributesList.AddRange(sourceModule.Value.Settings.Values.ToList());

            outputParametersFlowContainer.AttributesList.Clear();
            outputParametersFlowContainer.AttributesList.AddRange(sourceModule.Value.OutputParameters.Values.ToList());

            settings.Alpha = sourceModule.Value.HasSettings ? 1 : 0;
            parameters.Alpha = sourceModule.Value.HasOutputParameters ? 1 : 0;
        }, true);
    }

    public new void Clear()
    {
        metadataTextFlow.Clear();
        settingsFlowContainer.Clear();
        outputParametersFlowContainer.Clear();
        scrollContainer.ScrollToStart();
    }
}
