// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed partial class ModuleRunPopover : PopoverScreen
{
    private readonly TerminalContainer terminal;
    private readonly ParameterContainer parameters;

    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    public ModuleRunPopover()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Dark]
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(15),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.35f),
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
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        gameManager.State.BindValueChanged(e =>
        {
            switch (e.NewValue)
            {
                case GameManagerState.Starting:
                    Show();
                    break;

                case GameManagerState.Stopped:
                    Hide();
                    break;
            }
        }, true);
    }

    protected override void HideComplete()
    {
        terminal.Reset();
        parameters.ClearParameters();
    }

    protected override void Close() => gameManager.Stop();
}
