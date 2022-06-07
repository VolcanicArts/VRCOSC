// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditScreen;

public class ModuleEditContainer : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    [Cached]
    public Bindable<Modules.Module> SourceModule { get; } = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Padding = new MarginPadding(40);

        InternalChild = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 20,
            EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
            BorderThickness = 3,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray3,
                },
                new Container
                {
                    Name = "Content",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(4),
                    Child = new ModuleEditInnerContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            }
        };
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key != Key.Escape) return base.OnKeyDown(e);

        ScreenManager.FinishEditingModule();
        return true;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        return true;
    }

    protected override bool OnClick(ClickEvent e)
    {
        return true;
    }

    protected override bool OnHover(HoverEvent e)
    {
        return true;
    }
}
