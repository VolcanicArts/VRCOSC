// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class SettingsFlow : AttributeFlow
{
    protected override string Title => "Settings";

    protected override List<ModuleAttributeData> GetAttributeList(Module source)
    {
        return source.Settings.Values.ToList();
    }
}
