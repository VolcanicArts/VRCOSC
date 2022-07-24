// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.ModuleRun;

public class ParameterDisplay : Container
{
    public string Title { get; init; }

    private readonly Dictionary<string, ParameterEntry> parameterDict = new();
    private FillFlowContainer<ParameterEntry> parameterFlow;

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        BorderThickness = 2;
        BorderColour = VRCOSCColour.Gray0;

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
                            },
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

    public void AddEntry(string key, object value) => Schedule(() =>
    {
        var valueStr = value.ToString() ?? "Invalid Object";

        if (parameterDict.ContainsKey(key))
        {
            var existingEntry = parameterDict[key];
            existingEntry.Value.Value = valueStr;
            existingEntry.Value.TriggerChange();
        }
        else
        {
            var newEntry = new ParameterEntry
            {
                Key = key,
                Value = { Value = valueStr }
            };

            parameterFlow.Add(newEntry);
            parameterDict.Add(key, newEntry);
        }
    });
}
