// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed class ModuleRunPopover : PopoverScreen
{
    private readonly TerminalContainer terminal;
    private readonly ParameterContainer parameters;

    [Resolved]
    private BindableBool modulesRunning { get; set; } = null!;

    public ModuleRunPopover()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray3
            },
            new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        terminal = new TerminalContainer(),
                        parameters = new ParameterContainer()
                    }
                }
            },
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        modulesRunning.ValueChanged += e =>
        {
            if (e.NewValue)
                Show();
            else
                Hide();
        };
    }

    public override void Show()
    {
        terminal.Clear();
        parameters.ClearParameters();
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
        modulesRunning.Value = false;
    }
}
