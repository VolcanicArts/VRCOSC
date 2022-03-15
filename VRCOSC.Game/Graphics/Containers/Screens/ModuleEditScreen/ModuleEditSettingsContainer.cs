using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Module;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditScreen;

public class ModuleEditSettingsContainer : Container
{
    public Modules.Module SourceModule { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer<ModuleSettingContainer> settingsFlow;

        InternalChildren = new Drawable[]
        {
            new SpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Font = FrameworkFont.Regular.With(size: 50),
                Text = "Settings"
            },
            settingsFlow = new FillFlowContainer<ModuleSettingContainer>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5)
            },
        };

        SourceModule.Data.Settings.Keys.ForEach(key =>
        {
            var moduleSettingData = SourceModule.Data.Settings[key];

            switch (moduleSettingData)
            {
                case string:
                    settingsFlow.Add(new ModuleSettingStringContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;

                case bool:
                    settingsFlow.Add(new ModuleSettingBoolContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;

                case int:
                case long:
                    settingsFlow.Add(new ModuleSettingIntContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;
            }
        });
    }
}
