// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.SDK.Attributes;

public class ModuleAttributeMetadata
{
    /// <summary>
    /// The title for this <see cref="ModuleSetting"/>
    /// </summary>
    public readonly string Title;

    /// <summary>
    /// The description for this <see cref="ModuleSetting"/>
    /// </summary>
    public readonly string Description;

    /// <summary>
    /// The type to create for the UI of this <see cref="ModuleSetting"/>
    /// </summary>
    public readonly Type DrawableModuleAttributeType;

    protected ModuleAttributeMetadata(string title, string description, Type drawableModuleAttributeType)
    {
        Title = title;
        Description = description;
        DrawableModuleAttributeType = drawableModuleAttributeType;
    }
}

public class ModuleSettingMetadata : ModuleAttributeMetadata
{
    /// <summary>
    /// Whether to mark this <see cref="ModuleSetting"/> as required for the module to work
    /// </summary>
    public readonly bool Required;

    public ModuleSettingMetadata(string title, string description, Type drawableModuleAttributeType, bool required)
        : base(title, description, drawableModuleAttributeType)
    {
        Required = required;
    }
}

public class ModuleParameterMetadata : ModuleAttributeMetadata
{
    public ModuleParameterMetadata(string title, string description, Type drawableModuleAttributeType)
        : base(title, description, drawableModuleAttributeType)
    {
    }
}
