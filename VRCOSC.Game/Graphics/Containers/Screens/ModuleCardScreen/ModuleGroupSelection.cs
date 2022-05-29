// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using VRCOSC.Game.Modules.Stack;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleGroupSelection : Container
{
    [Resolved]
    private ModuleManager ModuleManager { get; set; }

    public ModuleGroupSelection()
    {
        Masking = true;
        EdgeEffect = new EdgeEffectParameters
        {
            Colour = Color4.Black.Opacity(0.6f),
            Radius = 5f,
            Type = EdgeEffectType.Shadow
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer moduleGroupFlow;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray3
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 50),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = FrameworkFont.Regular.With(size: 30),
                            Text = "Group Filter"
                        }
                    },
                    new Drawable[]
                    {
                        moduleGroupFlow = new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5)
                        },
                    }
                }
            }
        };

        ModuleManager.ForEach(moduleGroup => moduleGroupFlow.Add(new ModuleGroupCard(moduleGroup.Type)));
    }
}
