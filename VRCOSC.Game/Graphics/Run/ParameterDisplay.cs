// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.TabBar;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Graphics.Run;

public sealed partial class ParameterDisplay : Container
{
    [Resolved]
    private Bindable<Tab> selectedTab { get; set; } = null!;

    public string Title { get; init; } = null!;

    private readonly SortedDictionary<string, ParameterEntry> parameterDict = new();
    private FillFlowContainer<ParameterEntry> parameterFlow = null!;

    public ParameterDisplay()
    {
        Masking = true;
        BorderThickness = 3;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 40),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = Title,
                            Font = FrameworkFont.Regular.With(size: 30),
                            Colour = ThemeManager.Current[ThemeAttribute.Text]
                        }
                    },
                    new Drawable[]
                    {
                        new BasicScrollContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            ScrollbarVisible = false,
                            ClampExtension = 0,
                            Padding = new MarginPadding
                            {
                                Vertical = 1.5f,
                                Horizontal = 3
                            },
                            Child = parameterFlow = new FillFlowContainer<ParameterEntry>
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    }
                }
            }
        };
    }

    public void ClearContent()
    {
        parameterFlow.Clear();
        parameterDict.Clear();
    }

    public void AddEntry(string key, object value)
    {
        if (selectedTab.Value != Tab.Run && key != VRChatOscConstants.ADDRESS_CHATBOX_INPUT && key != VRChatOscConstants.ADDRESS_CHATBOX_TYPING) return;

        Schedule(() =>
        {
            var valueStr = value.ToString() ?? "Invalid Object";

            if (parameterDict.TryGetValue(key, out var existingEntry))
            {
                existingEntry.Value.Value = valueStr;
            }
            else
            {
                var newEntry = new ParameterEntry
                {
                    Key = key,
                    Value = { Value = valueStr }
                };

                parameterDict.Add(key, newEntry);
                parameterFlow.Add(newEntry);

                parameterDict.ForEach(pair =>
                {
                    var (_, entry) = pair;
                    var positionOfEntry = parameterDict.Values.ToList().IndexOf(entry);
                    parameterFlow.SetLayoutPosition(entry, parameterDict.Count - positionOfEntry);
                });
            }
        });
    }
}
