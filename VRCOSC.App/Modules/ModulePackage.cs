// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Reflection;

namespace VRCOSC.App.Modules;

public class ModulePackage
{
    public Assembly Assembly { get; }
    public bool Remote { get; }

    public string DisplayName => Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "UNKNOWN";

    public ModulePackage(Assembly assembly, bool remote)
    {
        Assembly = assembly;
        Remote = remote;
    }
}
