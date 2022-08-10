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

public sealed class ModuleEditingScreen : Container
{
    [Cached]
    public Bindable<Module?> SourceModule { get; } = new();

    public ModuleEditingScreen()
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
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                CornerRadius = 5,
                EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = VRCOSCColour.Gray4
                    },
                    moduleEditingContent = new ModuleEditingContent
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both
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
}
