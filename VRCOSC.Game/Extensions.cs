// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Reflection;

namespace VRCOSC.Game;

public static class AssemblyExtensions
{
    public static T? GetAssemblyAttribute<T>(this Assembly ass) where T : Attribute
    {
        var attributes = ass.GetCustomAttributes(typeof(T), false);
        return attributes.Length == 0 ? null : attributes.OfType<T>().SingleOrDefault();
    }
}
