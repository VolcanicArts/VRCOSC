// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules.SDK.Graphics.Addons;

namespace VRCOSC.Game.Modules.SDK.Attributes;

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

public class ButtonModuleSettingAddon : ModuleSettingAddon
{
    /// <summary>
    /// The text for this <see cref="ButtonModuleSettingAddon"/>
    /// </summary>
    internal readonly string Text;

    /// <summary>
    /// The background colour for this <see cref="ButtonModuleSettingAddon"/>
    /// </summary>
    internal readonly Colour4 Colour;

    /// <summary>
    /// The action to execute when this <see cref="ButtonModuleSettingAddon"/> is clicked
    /// </summary>
    internal readonly Action Action;

    public ButtonModuleSettingAddon(string text, Colour4 colour, Action action)
        : base(typeof(DrawableButtonModuleSettingAddon))
    {
        Text = text;
        Colour = colour;
        Action = action;
    }
}
