// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.SDK.Attributes.Settings.Addons;

public class ModuleSettingAddon
{
    /// <summary>
    /// The type to create for the UI of this <see cref="ModuleSettingAddon"/>
    /// </summary>
    private readonly Type drawableModuleSettingAddonType;

    /// <summary>
    /// The UI component associated with this <see cref="ModuleSettingAddon"/>.
    /// This creates a new instance each time this is called to allow for proper disposal of UI components
    /// </summary>
    internal Container GetDrawableModuleSettingAddon() => (Container)Activator.CreateInstance(drawableModuleSettingAddonType, this)!;

    protected ModuleSettingAddon(Type drawableModuleSettingAddonType)
    {
        this.drawableModuleSettingAddonType = drawableModuleSettingAddonType;
    }
}
