// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed class ModuleEditingContent : Container
{
    private TextFlowContainer metadataTextFlow = null!;
    private SeparatedAttributeFlow settings = null!;
    private SeparatedAttributeFlow parameters = null!;
    private BasicScrollContainer scrollContainer = null!;
    private FillFlowContainer<SeparatedAttributeFlow> separatedAttributeFlowFlow = null!;

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
                        separatedAttributeFlowFlow = new FillFlowContainer<SeparatedAttributeFlow>()
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5),
                            Children = new[]
                            {
                                settings = new SeparatedAttributeFlow("Settings"),
                                parameters = new SeparatedAttributeFlow("Parameters")
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

            settings.Replace(sourceModule.Value.Settings.Values);
            parameters.Replace(sourceModule.Value.OutputParameters.Values);

            settings.Alpha = sourceModule.Value.HasSettings ? 1 : 0;
            parameters.Alpha = sourceModule.Value.HasOutputParameters ? 1 : 0;
        }, true);
    }

    public new void Clear()
    {
        metadataTextFlow.Clear();
        separatedAttributeFlowFlow.ForEach(child => child.Clear());
        scrollContainer.ScrollToStart();
    }

    private class SeparatedAttributeFlow : Container
    {
        private AttributeFlow attributeFlow = null!;
        private readonly string title;

        public SeparatedAttributeFlow(string title)
        {
            this.title = title;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new LineSeparator
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                attributeFlow = new AttributeFlow(title),
            };
        }

        public void Replace(IEnumerable<ModuleAttribute> attributeData)
        {
            attributeFlow.AttributesList.Clear();
            attributeFlow.AttributesList.AddRange(attributeData);
        }

        public new void Clear()
        {
            attributeFlow.Clear();
        }
    }
}
