// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Settings;

public class SettingEnumCard<T> : SettingBaseCard where T : Enum
{
    [BackgroundDependencyLoader]
    private void load()
    {
        BasicDropdown<T> dropdown;

        Add(new Container
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Size = new Vector2(50, 25),
            Padding = new MarginPadding(15),
            Child = dropdown = new BasicDropdown<T>
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.X,
                Items = Enum.GetValues(typeof(T)).Cast<T>(),
                Current = { Value = (T)Enum.ToObject(typeof(T), ((EnumModuleSetting)SourceModule.DataManager.Settings[Key]).Value) }
            }
        });

        dropdown.Current.BindValueChanged(e =>
        {
            SourceModule.DataManager.UpdateEnumSetting(Key, e.NewValue);
        });
    }
}
