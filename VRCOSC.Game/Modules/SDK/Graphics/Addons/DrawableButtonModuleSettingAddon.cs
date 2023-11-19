// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.SDK.Attributes;

namespace VRCOSC.Game.Modules.SDK.Graphics.Addons;

public partial class DrawableButtonModuleSettingAddon : DrawableModuleSettingAddon<ButtonModuleSettingAddon>
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new TextButton
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.X,
            Width = 0.5f,
            Height = 30,
            TextContent = ModuleSettingAddon.Text,
            TextFont = Fonts.REGULAR.With(size: 25),
            CornerRadius = 5,
            Action = ModuleSettingAddon.Action,
            BackgroundColour = ModuleSettingAddon.Colour
        });
    }

    public DrawableButtonModuleSettingAddon(ButtonModuleSettingAddon moduleSettingAddon)
        : base(moduleSettingAddon)
    {
    }
}
