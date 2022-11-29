// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.ModuleEditing;
using VRCOSC.Game.Graphics.ModuleRun;

namespace VRCOSC.Game.Graphics.ModuleListing;

[Cached]
public sealed partial class ModuleListingScreen : Container
{
    public ModuleListingScreen()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray5
            },
            new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 50),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 50)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Header
                        {
                            Depth = float.MinValue
                        }
                    },
                    new Drawable[]
                    {
                        new Listing()
                    },
                    new Drawable[]
                    {
                        new Footer()
                    }
                }
            },
            new ModuleEditingPopover(),
            new ModuleInfoPopover(),
            new ModuleRunPopover()
        };
    }
}
