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
using VRCOSC.Game.Util;

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

            if (setting.GetType() == typeof(StringModuleSetting))
            {
                settingsFlow.Add(new ModuleSettingStringContainer
                {
                    Key = key,
                    SourceModule = SourceModule
                });
            }
            else if (setting.GetType() == typeof(IntModuleSetting))
            {
                settingsFlow.Add(new ModuleSettingIntContainer
                {
                    Key = key,
                    SourceModule = SourceModule
                });
            }
            else if (setting.GetType() == typeof(BoolModuleSetting))
            {
                settingsFlow.Add(new ModuleSettingBoolContainer
                {
                    Key = key,
                    SourceModule = SourceModule
                });
            }
            else if (setting.GetType() == typeof(EnumModuleSetting))
            {
                var enumSetting = (EnumModuleSetting)setting;
                Type type = typeof(ModuleSettingEnumContainer<>).MakeGenericType(TypeUtils.GetTypeByName(enumSetting.EnumName));
                ModuleSettingContainer instance = (ModuleSettingContainer)Activator.CreateInstance(type);
                instance.Key = key;
                instance.SourceModule = SourceModule;
                settingsFlow.Add(instance);
            }
        });
    }
}
