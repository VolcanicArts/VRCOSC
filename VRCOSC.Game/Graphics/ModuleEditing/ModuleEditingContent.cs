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
using VRCOSC.Game.Modules.ChatBox;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed partial class ModuleEditingContent : Container
{
    private readonly SpriteText titleText;
    private readonly SeparatedAttributeFlow settings;
    private readonly SeparatedAttributeFlow parameters;
    private readonly BasicScrollContainer scrollContainer;
    private readonly FillFlowContainer<SeparatedAttributeFlow> separatedAttributeFlowFlow;
    private readonly SpriteText chatBoxNotice;

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
                        chatBoxNotice = new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = FrameworkFont.Regular.With(size: 25),
                            Colour = ThemeManager.Current[ThemeAttribute.SubText],
                            Text = "Looking for the ChatBox settings? They're in a new screen on the left bar",
                            Margin = new MarginPadding
                            {
                                Vertical = 5
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
                                settings = new SeparatedAttributeFlow(),
                                parameters = new SeparatedAttributeFlow
                                {
                                    Title = "Parameter Names",
                                    SubTitle = "Only edit these if you know what you are doing"
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
        base.LoadComplete();

        editingModule.BindValueChanged(_ =>
        {
            if (editingModule.Value is null) return;

            chatBoxNotice.Alpha = editingModule.Value.GetType().IsSubclassOf(typeof(ChatBoxModule)) ? 1 : 0;

            separatedAttributeFlowFlow.ForEach(child => child.Clear());
            scrollContainer.ScrollToStart();

            titleText.Text = $"{editingModule.Value.Title} Settings";

            settings.Replace(editingModule.Value.Settings.Values);
            settings.Alpha = editingModule.Value.HasSettings ? 1 : 0;

            parameters.Replace(editingModule.Value.Parameters.Values);
            parameters.Alpha = editingModule.Value.HasParameters ? 1 : 0;
        }, true);
    }

    private sealed partial class SeparatedAttributeFlow : FillFlowContainer
    {
        public string Title { get; init; } = string.Empty;
        public string SubTitle { get; init; } = string.Empty;

        private AttributeFlow attributeFlow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(0, 5);
            Direction = FillDirection.Vertical;

            Children = new Drawable[]
            {
                new LineSeparator
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FrameworkFont.Regular.With(size: 60),
                    Colour = ThemeManager.Current[ThemeAttribute.Text],
                    Text = Title,
                    Alpha = string.IsNullOrEmpty(Title) ? 0 : 1
                },
                new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FrameworkFont.Regular.With(size: 25),
                    Colour = ThemeManager.Current[ThemeAttribute.SubText],
                    Margin = new MarginPadding
                    {
                        Vertical = 10
                    },
                    Text = SubTitle,
                    Alpha = string.IsNullOrEmpty(SubTitle) ? 0 : 1
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
