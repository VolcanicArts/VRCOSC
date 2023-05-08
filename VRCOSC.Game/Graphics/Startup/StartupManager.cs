// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Graphics.Startup.Serialisation;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Startup;

public class StartupManager
{
    private readonly TerminalLogger logger = new("VRCOSC");
    private readonly StartupSerialiser serialiser;

    public readonly BindableList<Bindable<string>> FilePaths = new();

    public StartupManager(Storage storage, NotificationContainer notification)
    {
        serialiser = new StartupSerialiser(storage, notification, this);
    }

    public void Load()
    {
        var data = serialiser.Load();

        if (data is not null)
        {
            data.ForEach(model =>
            {
                var bindable = new Bindable<string>(model.Path);
                FilePaths.Add(bindable);
                bindable.BindValueChanged(_ => serialiser.Save());
            });

            serialiser.Save();
        }

        FilePaths.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (Bindable<string> newItem in e.NewItems)
                {
                    newItem.BindValueChanged(_ => serialiser.Save());
                }
            }

            serialiser.Save();
        });
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
