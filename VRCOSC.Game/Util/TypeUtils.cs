// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;

namespace VRCOSC.Game.Util;

public static class TypeUtils
{
    public static Type GetTypeByName(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .FirstOrDefault(t => t.Name.Contains(name)) ?? throw new InvalidOperationException("Could not find enum of name " + name);
    }
}
