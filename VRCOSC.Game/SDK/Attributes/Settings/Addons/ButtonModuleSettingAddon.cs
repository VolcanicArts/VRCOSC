// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;
using VRCOSC.Game.SDK.Graphics.Addons;

namespace VRCOSC.Game.SDK.Attributes.Settings.Addons;

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
