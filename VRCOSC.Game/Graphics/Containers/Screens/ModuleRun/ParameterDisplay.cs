// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using CoreOSC;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;

public class ParameterDisplay : Container
{
    public string Title { get; init; }

    private FillFlowContainer<ParameterEntry> parameterFlow;

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
                            Text = Title
                        }
                    },
                    new Drawable[]
                    {
                        parameterFlow = new FillFlowContainer<ParameterEntry>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.5f,
                            Padding = new MarginPadding
                            {
                                Vertical = 1.5f,
                                Horizontal = 3
                            }
                        },
                    }
                }
            }
        };
    }

    public void ClearContent()
    {
        parameterFlow.Clear();
    }

    public void AddEntry(string key, object value)
    {
        Scheduler.Add(() => addEntry(key, value));
    }

    private void addEntry(string key, object value)
    {
        if (value is OscTrue) value = true;
        if (value is OscFalse) value = false;

        var valueStr = value.ToString() ?? "Invalid Object";

        bool successful = false;

        parameterFlow.ForEach(entry =>
        {
            if (!entry.Key.Equals(key)) return;

            entry.Value.Value = valueStr;
            successful = true;
        });

        if (successful) return;

        parameterFlow.Add(new ParameterEntry
        {
            Key = key,
            Value = { Value = valueStr }
        });
    }
}
