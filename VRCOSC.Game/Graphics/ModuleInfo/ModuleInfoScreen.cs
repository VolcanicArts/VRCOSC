// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleInfo;

public partial class ModuleInfoScreen : BaseScreen
{
    private FillFlowContainer<DrawableInfoCard> infoFlow = null!;
    private FillFlowContainer<DrawableParameterAttribute> parameterAttributeFlow = null!;

    [Resolved(name: "InfoModule")]
    private Bindable<Module?> infoModule { get; set; } = null!;

    protected override BaseHeader CreateHeader() => new ModuleInfoHeader();

    protected override Drawable CreateBody() => new VRCOSCScrollContainer
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        ClampExtension = 0,
        ScrollContent =
        {
            Child = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Spacing = new Vector2(0, 10),
                Direction = FillDirection.Vertical,
                LayoutEasing = Easing.OutQuad,
                LayoutDuration = 150,
                Children = new Drawable[]
                {
                    infoFlow = new FillFlowContainer<DrawableInfoCard>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 5),
                        Direction = FillDirection.Vertical,
                        LayoutEasing = Easing.OutQuad,
                        LayoutDuration = 150
                    },
                    parameterAttributeFlow = new FillFlowContainer<DrawableParameterAttribute>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 5),
                        Direction = FillDirection.Vertical,
                        LayoutEasing = Easing.OutQuad,
                        LayoutDuration = 150
                    }
                }
            }
        }
    };

    protected override void LoadComplete()
    {
        infoModule.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            infoFlow.Clear();
            parameterAttributeFlow.Clear();

            infoFlow.AddRange(e.NewValue.InfoList.Select(infoString => new DrawableInfoCard(infoString)));
            parameterAttributeFlow.AddRange(e.NewValue.Parameters.Values.Select(parameterAttribute => new DrawableParameterAttribute(parameterAttribute)));

            infoFlow.Alpha = infoFlow.Any() ? 1 : 0;
            parameterAttributeFlow.Alpha = parameterAttributeFlow.Any() ? 1 : 0;
        }, true);
    }
}
