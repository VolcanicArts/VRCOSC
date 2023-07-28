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
using VRCOSC.Game.Serialisation;
using VRCOSC.Game.Startup.Serialisation.V1;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Managers;

public class StartupManager
{
    private readonly TerminalLogger logger = new("VRCOSC");
    private SerialisationManager serialisationManager = null!;

    public readonly BindableList<Bindable<string>> FilePaths = new();

    public void Initialise(Storage storage, NotificationContainer notification)
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new StartupSerialiser(storage, notification, this));
    }

    public void Load()
    {
        Deserialise();

        FilePaths.BindCollectionChanged((_, _) => Serialise());

        FilePaths.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (Bindable<string> newItem in e.NewItems)
                {
                    newItem.BindValueChanged(_ => Serialise());
                }
            }
        }, true);
    }

    public void Deserialise()
    {
        serialisationManager.Deserialise();
    }

    public void Serialise()
    {
        serialisationManager.Serialise();
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
