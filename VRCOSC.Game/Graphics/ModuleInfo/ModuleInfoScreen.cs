// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleInfo;

public partial class ModuleInfoScreen : BaseScreen
{
    private FillFlowContainer<DrawableParameterAttribute> parameterAttributeFlow = null!;

    [Resolved(name: "InfoModule")]
    private Bindable<Module?> infoModule { get; set; } = null!;

    protected override BaseHeader CreateHeader() => new ModuleInfoHeader();

    protected override Drawable CreateBody() => new BasicScrollContainer
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        ClampExtension = 0,
        ScrollbarVisible = false,
        ScrollContent =
        {
            Child = parameterAttributeFlow = new FillFlowContainer<DrawableParameterAttribute>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Spacing = new Vector2(5, 5),
                Direction = FillDirection.Full,
                LayoutEasing = Easing.OutQuad,
                LayoutDuration = 150
            }
        }
    };

    protected override void LoadComplete()
    {
        infoModule.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            parameterAttributeFlow.Clear();

            e.NewValue.Parameters.Values.ForEach(parameterAttribute =>
            {
                parameterAttributeFlow.Add(new DrawableParameterAttribute(parameterAttribute));
            });
        }, true);
    }
}
