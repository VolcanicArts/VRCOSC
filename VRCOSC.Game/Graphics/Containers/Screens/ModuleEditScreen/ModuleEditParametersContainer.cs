// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Module.ModuleOscParameter;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditScreen;

public class ModuleEditParametersContainer : FillFlowContainer
{
    [Resolved]
    private Bindable<Modules.Module> SourceModule { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer<ModuleOscParameterContainer> parametersFlow;

        InternalChildren = new Drawable[]
        {
            new SpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Font = FrameworkFont.Regular.With(size: 50),
                Text = "Output Parameters"
            },
            parametersFlow = new FillFlowContainer<ModuleOscParameterContainer>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10)
            }
        };

        SourceModule.BindValueChanged(_ =>
        {
            if (SourceModule.Value == null) return;

            parametersFlow.Clear();

            SourceModule.Value.DataManager.Parameters.Keys.ToList().ForEach(key =>
            {
                parametersFlow.Add(new ModuleOscParameterContainer
                {
                    Key = key,
                    SourceModule = SourceModule.Value
                });
            });
        });
    }
}
