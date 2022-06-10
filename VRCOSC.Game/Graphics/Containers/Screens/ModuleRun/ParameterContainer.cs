// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
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
        ParameterDisplay outgoingParameterDisplay;
        ParameterDisplay incomingParameterDisplay;

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
                outgoingParameterDisplay = new ParameterDisplay
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Padding = new MarginPadding
                    {
                        Vertical = 1.5f,
                        Horizontal = 3
                    },
                    Title = "Outgoing"
                },
                incomingParameterDisplay = new ParameterDisplay
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Padding = new MarginPadding
                    {
                        Vertical = 1.5f,
                        Horizontal = 3
                    },
                    Title = "Incoming"
                }
            }
        };

        moduleManager.OnParameterSent += (key, value) => outgoingParameterDisplay.AddEntry(key, value);
        moduleManager.OnParameterReceived += (key, value) => incomingParameterDisplay.AddEntry(key, value);
    }
}
