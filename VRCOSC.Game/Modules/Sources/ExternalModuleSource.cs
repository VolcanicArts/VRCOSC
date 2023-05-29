// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules.Sources;

public class ExternalModuleSource : DLLModuleSource
{
    private const string custom_directory = "modules";

    private readonly Storage storage;

    public ExternalModuleSource(Storage storage)
    {
        this.storage = storage;
    }

    public override IEnumerable<Type> Load()
    {
        var moduleDirectoryPath = storage.GetStorageForDirectory(custom_directory).GetFullPath(string.Empty, true);

        var moduleList = new List<Type>();
        Directory.GetFiles(moduleDirectoryPath, "*.dll", SearchOption.AllDirectories).ForEach(dllPath => moduleList.AddRange(LoadModulesFromDLL(dllPath)));
        return moduleList;
    }
}
