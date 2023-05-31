// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed partial class ModulesHeader : BaseHeader
{
    [Resolved]
    private ModulesScreen modulesScreen { get; set; } = null!;

    protected override string Title => "Modules";
    protected override string SubTitle => "Select modules and edit settings/parameters";

    protected override Drawable CreateRightShoulder() => new Container
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        Child = new IconButton
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Size = new Vector2(0.8f),
            Icon = FontAwesome.Solid.Download,
            IconShadow = true,
            CornerRadius = 10,
            BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
            Action = () => modulesScreen.ShowRepoListing()
        }
    };
}
