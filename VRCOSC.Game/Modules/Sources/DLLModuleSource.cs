// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VRCOSC.Game.Modules.Sources;

public abstract class DLLModuleSource : IModuleSource
{
    protected IEnumerable<Type> LoadModulesFromDLL(string dllPath)
    {
        return Assembly.LoadFile(dllPath).GetTypes().Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract);
    }

    public abstract IEnumerable<Type> Load();
}
