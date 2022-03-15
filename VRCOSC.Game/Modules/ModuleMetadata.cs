// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

namespace VRCOSC.Game.Modules;

public class ModuleMetadata
{
    public Dictionary<string, ModuleAttributeMetadata> Settings { get; } = new();
    public Dictionary<string, ModuleAttributeMetadata> Parameters { get; } = new();
}

public struct ModuleAttributeMetadata
{
    public string DisplayName;
    public string Description;
}
