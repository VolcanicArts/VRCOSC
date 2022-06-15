// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class OutputParametersFlow : AttributeFlow
{
    protected override string Title => "Output Parameters";

    protected override List<ModuleAttributeData> GetAttributeList(Module source)
    {
        return source.OutputParameters.Values.ToList();
    }
}
