// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Logging;

namespace VRCOSC.Game.Modules.Sources;

public class InternalModuleSource : DLLModuleSource
{
    public override IEnumerable<Type> Load()
    {
        var dllPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories).FirstOrDefault(fileName => fileName.Contains("VRCOSC.Modules"));

        if (string.IsNullOrEmpty(dllPath))
        {
            Logger.Log("Could not find internal module assembly");
            return new List<Type>();
        }

        return LoadModulesFromDLL(dllPath);
    }
}
