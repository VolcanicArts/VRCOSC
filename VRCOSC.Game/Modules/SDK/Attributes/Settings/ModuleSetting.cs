// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules.SDK.Attributes.Settings.Addons;

namespace VRCOSC.Game.Modules.SDK.Attributes.Settings;

public abstract class ModuleSetting : ModuleAttribute
{
    /// <summary>
    /// The metadata for this <see cref="ModuleSetting"/>
    /// </summary>
    internal new ModuleSettingMetadata Metadata => (ModuleSettingMetadata)base.Metadata;

    /// <summary>
    /// The UI component associated with this <see cref="ModuleSetting"/>.
    /// This creates a new instance each time this is called to allow for proper disposal of UI components
    /// </summary>
    internal Container GetDrawable() => (Container)Activator.CreateInstance(Metadata.DrawableModuleSettingType, this)!;

    /// <summary>
    /// A callback for checking to see if this <see cref="ModuleSetting"/> should be enabled
    /// </summary>
    public Func<bool> IsEnabled = () => true;

    /// <summary>
    /// Addons for this <see cref="ModuleSetting"/>
    /// </summary>
    internal readonly List<ModuleSettingAddon> Addons = new();

    protected ModuleSetting(ModuleSettingMetadata metadata)
        : base(metadata)
    {
    }

    /// <summary>
    /// Add a <see cref="ModuleSettingAddon"/> to this <see cref="ModuleSetting"/>
    /// </summary>
    /// <param name="addon">The <see cref="ModuleSettingAddon"/> to add</param>
    public ModuleSetting AddAddon(ModuleSettingAddon addon)
    {
        Addons.Add(addon);
        return this;
    }
}
