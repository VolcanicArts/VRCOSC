using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleSettingStringContainer : ModuleSettingContainer
{
    public ModuleSettingString? SourceSetting { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        if (SourceSetting == null)
            throw new ArgumentNullException(nameof(SourceSetting));

        Child = new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(500, 100),
            Colour = Colour4.Black
        };
    }
}
