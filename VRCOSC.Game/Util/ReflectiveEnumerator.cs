// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VRCOSC.Game.Util;

public static class ReflectiveEnumerator
{
    public static List<T>? GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
    {
        List<T>? objects = Assembly.GetAssembly(typeof(T))
                                   ?.GetTypes()
                                   .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
                                   .Select(type => (T)Activator.CreateInstance(type, constructorArgs)!)
                                   .ToList();
        return objects;
    }

    public static Type? GetEnumTypeFromName(string enumName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(enumName)).FirstOrDefault(type => type?.IsEnum ?? false);
    }
}
