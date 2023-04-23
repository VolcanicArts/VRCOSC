// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class DrawableModuleVariables : Container
{
    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    private readonly string module;
    private readonly List<ClipVariableMetadata> clipVariables;

    public DrawableModuleVariables(string module, List<ClipVariableMetadata> clipVariables)
    {
        this.module = module;
        this.clipVariables = clipVariables;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer variableFlow;

        Child = variableFlow = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 5)
        };

        variableFlow.Add(new SpriteText
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Text = gameManager.ModuleManager.GetModuleName(module),
            Font = FrameworkFont.Regular.With(size: 20),
            Colour = ThemeManager.Current[ThemeAttribute.Text]
        });

        clipVariables.ForEach(clipVariable =>
        {
            variableFlow.Add(new DrawableVariable(clipVariable));
        });
    }
}
