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
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed partial class ModuleEditingContent : Container
{
    private readonly SpriteText titleText;
    private readonly SeparatedAttributeFlow settings;
    private readonly BasicScrollContainer scrollContainer;
    private readonly FillFlowContainer<SeparatedAttributeFlow> separatedAttributeFlowFlow;

    [Resolved(name: "EditingModule")]
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
                    Children = new Drawable[]
                    {
                        titleText = new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = FrameworkFont.Regular.With(size: 75),
                            Colour = ThemeManager.Current[ThemeAttribute.Text],
                            Margin = new MarginPadding
                            {
                                Vertical = 10
                            }
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
                                settings = new SeparatedAttributeFlow()
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

            separatedAttributeFlowFlow.ForEach(child => child.Clear());
            scrollContainer.ScrollToStart();

            titleText.Text = $"{editingModule.Value.Title} Settings";

            settings.Replace(editingModule.Value.Settings.Values);
            settings.Alpha = editingModule.Value.HasSettings ? 1 : 0;
        }, true);
    }

    private sealed partial class SeparatedAttributeFlow : Container
    {
        private readonly AttributeFlow attributeFlow;

        public SeparatedAttributeFlow()
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
                attributeFlow = new AttributeFlow()
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
