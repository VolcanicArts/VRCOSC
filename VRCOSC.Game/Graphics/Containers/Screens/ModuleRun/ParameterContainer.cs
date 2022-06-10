// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;

public sealed class ParameterContainer : Container
{
    [Resolved]
    private ModuleManager moduleManager { get; set; }

    public ParameterContainer()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer<ParameterEntry> parameterFlow;

        Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            BorderThickness = 3,
            Masking = true,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray2,
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(1.5f)
                },
                parameterFlow = new FillFlowContainer<ParameterEntry>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Vertical = 1.5f,
                        Horizontal = 3
                    }
                }
            }
        };

        moduleManager.OnParameterSent += (key, value) =>
        {
            bool successful = false;
            parameterFlow.ForEach(entry =>
            {
                if (!entry.Key.Equals(key)) return;

                entry.Value.Value = value.ToString() ?? "Invalid Object";
                successful = true;
            });

            if (!successful)
            {
                parameterFlow.Add(new ParameterEntry
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Key = key,
                    Value = { Value = value.ToString() ?? "Invalid Object" }
                });
            }
        };
    }
}
