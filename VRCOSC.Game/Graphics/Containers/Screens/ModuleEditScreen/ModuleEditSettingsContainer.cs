// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Module;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditScreen;

public class ModuleEditSettingsContainer : FillFlowContainer
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
                Spacing = new Vector2(0, 10)
            },
        };

        SourceModule.DataManager.Settings.ForEach(pair =>
        {
            var (key, setting) = pair;

            switch (setting)
            {
                case StringModuleSetting:
                    settingsFlow.Add(new ModuleSettingStringContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;

                case IntModuleSetting:
                    settingsFlow.Add(new ModuleSettingIntContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;

                case BoolModuleSetting:
                    settingsFlow.Add(new ModuleSettingBoolContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;

                case EnumModuleSetting enumModuleSetting:
                    Type type = typeof(ModuleSettingEnumContainer<>).MakeGenericType(enumModuleSetting.Value.GetType());
                    ModuleSettingContainer instance = (ModuleSettingContainer)Activator.CreateInstance(type)!;
                    instance.Key = key;
                    instance.SourceModule = SourceModule;
                    settingsFlow.Add(instance);
                    break;
            }
        });
    }
}
