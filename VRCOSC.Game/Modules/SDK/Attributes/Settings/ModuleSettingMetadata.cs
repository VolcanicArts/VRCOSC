// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.SDK.Attributes.Settings;

public class ModuleSettingMetadata : ModuleAttributeMetadata
{
    /// <summary>
    /// The type to create for the UI of this <see cref="ModuleSetting"/>
    /// </summary>
    public readonly Type DrawableModuleSettingType;

    public ModuleSettingMetadata(string title, string description, Type drawableModuleSettingType)
        : base(title, description)
    {
        DrawableModuleSettingType = drawableModuleSettingType;
    }
}
