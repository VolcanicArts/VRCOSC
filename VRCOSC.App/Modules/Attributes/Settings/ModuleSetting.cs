// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows.Controls;

namespace VRCOSC.App.Modules.Attributes.Settings;

public abstract class ModuleSetting : ModuleAttribute
{
    /// <summary>
    /// The metadata for this <see cref="ModuleSetting"/>
    /// </summary>
    public new ModuleSettingMetadata Metadata => (ModuleSettingMetadata)base.Metadata;

    /// <summary>
    /// Creates a new <see cref="Page"/> instance as set in the <see cref="Metadata"/>
    /// </summary>
    public Page PageInstance => (Page)Activator.CreateInstance(Metadata.PageType, this)!;

    /// <summary>
    /// A callback for checking to see if this <see cref="ModuleSetting"/> should be enabled
    /// </summary>
    public Func<bool> IsEnabled = () => true;

    protected ModuleSetting(ModuleSettingMetadata metadata)
        : base(metadata)
    {
    }
}
