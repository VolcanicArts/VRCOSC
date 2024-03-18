// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows.Controls;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public class ModuleSettingMetadata : ModuleAttributeMetadata
{
    /// <summary>
    /// The WPF <see cref="Page"/>'s <see cref="Type"/> associated with this <see cref="ModuleSetting"/>
    /// </summary>
    public readonly Type PageType;

    public ModuleSettingMetadata(string title, string description, Type pageType)
        : base(title, description)
    {
        PageType = pageType;
    }
}
