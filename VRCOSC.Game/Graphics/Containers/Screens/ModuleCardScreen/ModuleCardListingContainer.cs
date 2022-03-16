// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Drawables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleCardListingContainer : Container
{
    private const float footer_height = 60;

    [Resolved]
    private ModuleManager ModuleManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer<ModuleCardGroupContainer> moduleCardGroupFlow;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray4
            },
            new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ClampExtension = 20,
                ScrollbarVisible = false,
                Child = moduleCardGroupFlow = new FillFlowContainer<ModuleCardGroupContainer>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding
                    {
                        Top = 10,
                        Bottom = 10 + footer_height,
                        Left = 10,
                        Right = 10
                    }
                }
            },
            new LineSeparator
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(0.95f, 5),
                Colour = Colour4.Black.Opacity(0.5f),
                Y = -footer_height
            },
            new ModuleCardListingFooter
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Height = footer_height
            }
        };

        ModuleManager.Modules.ForEach(pair =>
        {
            var (moduleType, modules) = pair;

            moduleCardGroupFlow.Add(new ModuleCardGroupContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 20,
                BorderThickness = 3,
                ModuleType = moduleType,
                Modules = modules
            });
        });
    }
}
