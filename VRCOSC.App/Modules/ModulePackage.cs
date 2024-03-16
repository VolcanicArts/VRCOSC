// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Reflection;
using System.Windows;

namespace VRCOSC.App.Modules;

public class ModulePackage
{
    public Assembly Assembly { get; }
    public bool Remote { get; }

    public string DisplayName => Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "UNKNOWN";
    public Visibility LocalVisibility => Remote ? Visibility.Collapsed : Visibility.Visible;

    public ModulePackage(Assembly assembly, bool remote)
    {
        Assembly = assembly;
        Remote = remote;
    }
}
