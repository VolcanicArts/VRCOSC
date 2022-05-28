// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public sealed class ModuleGroupCard : VRCOSCButton
{
    private readonly ModuleType moduleType;

    [Resolved]
    private ModuleSelection ModuleSelection { get; set; }

    public ModuleGroupCard(ModuleType moduleType)
    {
        this.moduleType = moduleType;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Size = new Vector2(0.8f, 50);
        CornerRadius = 5;
        BackgroundColour = VRCOSCColour.Gray1;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new SpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Text = moduleType.ToString()
        });
    }

    protected override bool OnClick(ClickEvent e)
    {
        ModuleSelection.SelectedType.Value = moduleType;
        return base.OnClick(e);
    }
}
