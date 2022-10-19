// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed class ModuleEditingPopover : Container
{
    [Cached]
    public Bindable<Module?> SourceModule { get; } = new();

    public ModuleEditingPopover()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        RelativePositionAxes = Axes.X;
        Padding = new MarginPadding(40);
        X = 1;
    }

    [BackgroundDependencyLoader]
    private void load(VRCOSCGame game)
    {
        ModuleEditingContent moduleEditingContent;
        Children = new Drawable[]
        {
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
                BorderColour = VRCOSCColour.Gray0,
                BorderThickness = 2,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = VRCOSCColour.Gray4
                    },
                    moduleEditingContent = new ModuleEditingContent
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Vertical = 2.5f
                        }
                    }
                }
            }
        };

        game.EditingModule.ValueChanged += e =>
        {
            var module = e.NewValue;

            if (module is null)
                this.MoveToX(1, 1000, Easing.InQuint).Finally(_ => moduleEditingContent.Clear());
            else
                this.MoveToX(0, 1000, Easing.OutQuint);

            SourceModule.Value = module;
        };
    }

    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;
}
