// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Startup;

public class StartupManager
{
    private readonly TerminalLogger logger = new("VRCOSC");
    private readonly Storage storage;

    public readonly BindableList<Bindable<string>> FilePaths = new();

    public StartupManager(Storage storage)
    {
        this.storage = storage;
    }

    public void Load()
    {
        if (storage.Exists("startup.json"))
        {
            var data = JsonConvert.DeserializeObject<List<StartupModel>>(File.ReadAllText(storage.GetFullPath("startup.json")));

            data?.ForEach(model =>
            {
                var bindable = new Bindable<string>(model.Path);
                FilePaths.Add(bindable);
                bindable.BindValueChanged(_ => Save());
            });
        }

        FilePaths.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (Bindable<string> newItem in e.NewItems)
                {
                    newItem.BindValueChanged(_ => Save());
                }
            }

            Save();
        });

        Save();
    }

    public void Save()
    {
        var data = new List<StartupModel>();
        FilePaths.ForEach(path => data.Add(new StartupModel(path.Value)));

        using var stream = storage.CreateFileSafely("startup.json");
        using var writer = new StreamWriter(stream);
        writer.Write(JsonConvert.SerializeObject(data));
    }

    public void Start()
    {
        FilePaths.ForEach(filePath =>
        {
            try
            {
                if (!File.Exists(filePath.Value)) return;

                var processName = new FileInfo(filePath.Value).Name.ToLowerInvariant().Replace(@".exe", string.Empty);

                if (Process.GetProcessesByName(processName).Any()) return;

                Process.Start(filePath.Value);
                logger.Log($"Running file {filePath.Value}");
            }
            catch (Exception)
            {
                logger.Log($"Failed to run {filePath.Value}");
            }
        });
    }
}
