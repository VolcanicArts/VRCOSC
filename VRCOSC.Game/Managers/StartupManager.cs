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
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Managers;

public class StartupManager : ICanSerialise
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
        FilePaths.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (Bindable<string> newItem in e.NewItems)
                {
                    newItem.BindValueChanged(_ => Serialise());
                }
            }

            Serialise();
        });

        Deserialise();
    }

    public void Deserialise()
    {
        var data = serialiser.Deserialise();

        if (data is null) return;

        FilePaths.AddRange(data.Select(model => new Bindable<string>(model.Path)));
    }

    public void Serialise()
    {
        serialiser.Serialise();
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
