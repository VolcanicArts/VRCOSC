// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed class ModuleEditingContent : Container
{
    private readonly TextFlowContainer metadataTextFlow;
    private readonly SeparatedAttributeFlow settings;
    private readonly SeparatedAttributeFlow parameters;
    private readonly BasicScrollContainer scrollContainer;
    private readonly FillFlowContainer<SeparatedAttributeFlow> separatedAttributeFlowFlow;

    [Resolved]
    private Bindable<Module?> editingModule { get; set; } = null!;

    public ModuleEditingContent()
    {
        Children = new Drawable[]
        {
            scrollContainer = new BasicScrollContainer
            {
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
                        separatedAttributeFlowFlow = new FillFlowContainer<SeparatedAttributeFlow>
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
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        editingModule.BindValueChanged(_ =>
        {
            if (editingModule.Value is null) return;

            metadataTextFlow.Clear();
            separatedAttributeFlowFlow.ForEach(child => child.Clear());
            scrollContainer.ScrollToStart();

            metadataTextFlow.AddText(editingModule.Value.Title, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 75);
            });
            metadataTextFlow.AddParagraph(editingModule.Value.Description, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 40);
                t.Colour = VRCOSCColour.Gray9;
            });
            metadataTextFlow.AddParagraph("Made by ", t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 30);
                t.Colour = VRCOSCColour.Gray9;
            });
            metadataTextFlow.AddText(editingModule.Value.Author, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 30);
                t.Colour = VRCOSCColour.GrayE;
            });

            settings.Replace(editingModule.Value.Settings.Values);
            parameters.Replace(editingModule.Value.OutputParameters.Values);

            settings.Alpha = editingModule.Value.HasSettings ? 1 : 0;
            parameters.Alpha = editingModule.Value.HasOutgoingParameters ? 1 : 0;
        }, true);
    }

    private sealed class SeparatedAttributeFlow : Container
    {
        private readonly AttributeFlow attributeFlow;

        public SeparatedAttributeFlow(string title)
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
