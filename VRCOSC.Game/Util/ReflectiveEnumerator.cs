using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VRCOSC.Game.Util;

public static class ReflectiveEnumerator
{
    public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
    {
        List<T> objects = Assembly.GetAssembly(typeof(T))
                                  .GetTypes()
                                  .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
                                  .Select(type => (T)Activator.CreateInstance(type, constructorArgs))
                                  .ToList();
        return objects;
    }
}
