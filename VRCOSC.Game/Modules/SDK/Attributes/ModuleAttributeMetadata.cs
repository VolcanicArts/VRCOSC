// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.Modules.SDK.Parameters;

namespace VRCOSC.Game.Modules.SDK.Attributes;

public class ModuleAttributeMetadata
{
    /// <summary>
    /// The title for this <see cref="ModuleAttribute"/>
    /// </summary>
    public readonly string Title;

    /// <summary>
    /// The description for this <see cref="ModuleAttribute"/>
    /// </summary>
    public readonly string Description;

    protected ModuleAttributeMetadata(string title, string description)
    {
        Title = title;
        Description = description;
    }
}

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

public class ModuleParameterMetadata : ModuleAttributeMetadata
{
    /// <summary>
    /// The mode for this <see cref="ModuleParameter"/>
    /// </summary>
    public readonly ParameterMode Mode;

    /// <summary>
    /// The expected type for this <see cref="ModuleParameter"/>
    /// </summary>
    public readonly Type ExpectedType;

    public ModuleParameterMetadata(string title, string description, ParameterMode mode, Type expectedType)
        : base(title, description)
    {
        Mode = mode;
        ExpectedType = expectedType;
    }
}
