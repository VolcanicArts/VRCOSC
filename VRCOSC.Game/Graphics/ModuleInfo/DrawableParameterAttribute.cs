// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleInfo;

public partial class DrawableParameterAttribute : Container
{
    private readonly ModuleParameter parameterAttribute;

    public DrawableParameterAttribute(ModuleParameter parameterAttribute)
    {
        this.parameterAttribute = parameterAttribute;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];

        TextFlowContainer textFlow;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both
            },
            textFlow = new TextFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5)
            }
        };

        textFlow.AddText($"{parameterAttribute.Name} - {parameterAttribute.Description}", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });

        textFlow.NewParagraph();

        textFlow.AddParagraph($"Name: {parameterAttribute.ParameterName}", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 17);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });

        textFlow.AddParagraph($"Type: {parameterAttribute.ExpectedType.ToReadableName()}", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 17);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });

        textFlow.AddParagraph($"Writes to VRC: {parameterAttribute.Mode.HasFlagFast(ParameterMode.Write)}", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 17);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });

        textFlow.AddParagraph($"Reads from VRC: {parameterAttribute.Mode.HasFlagFast(ParameterMode.Read)}", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 17);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });
    }
}
