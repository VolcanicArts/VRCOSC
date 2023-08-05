// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.App;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Startup;

public partial class DrawableStartupDataSpawner : StartupDataFlowEntry
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(40),
                Circular = true,
                IconShadow = true,
                Icon = FontAwesome.Solid.Plus,
                BackgroundColour = ThemeManager.Current[ThemeAttribute.Success],
                Action = () => appManager.StartupManager.Instances.Add(new StartupInstance())
            }
        };
    }
}
