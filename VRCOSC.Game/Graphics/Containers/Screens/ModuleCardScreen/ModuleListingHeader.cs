// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Drawables;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleListingHeader : Container
{
    [Resolved]
    private ModuleSelection ModuleSelection { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        SpriteText headerText;
        Children = new Drawable[]
        {
            new LineSeparator
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(0.95f, 5),
                Colour = Colour4.Black.Opacity(0.5f),
            },
            headerText = new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = FrameworkFont.Regular.With(size: 50),
                Shadow = true
            }
        };

        ModuleSelection.SelectedType.BindValueChanged(e =>
        {
            headerText.Text = e.NewValue.ToString();
        }, true);
    }
}
