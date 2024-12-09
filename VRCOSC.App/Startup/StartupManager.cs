// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Startup.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Startup;

public class StartupManager
{
    private static StartupManager? instance;
    public static StartupManager GetInstance() => instance ??= new StartupManager();

    public ObservableCollection<StartupInstance> Instances { get; } = [];

    private readonly SerialisationManager serialisationManager;

    private StartupManager()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new StartupManagerSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Load()
    {
        serialisationManager.Deserialise();

        Instances.OnCollectionChanged((newItems, _) =>
        {
            foreach (var newInstance in newItems)
            {
                newInstance.FileLocation.Subscribe(_ => serialisationManager.Serialise());
                newInstance.Arguments.Subscribe(_ => serialisationManager.Serialise());
            }
        }, true);

        Instances.OnCollectionChanged((_, _) => serialisationManager.Serialise());
    }

    public void OpenFileLocations()
    {
        foreach (var startupInstance in Instances)
        {
            var fileLocation = startupInstance.FileLocation.Value;
            var arguments = startupInstance.Arguments.Value;

            try
            {
                if (!File.Exists(fileLocation))
                {
                    ExceptionHandler.Handle($"File location '{fileLocation}' does not exist when attempting to startup");
                    continue;
                }

                if (fileLocation.EndsWith(".exe"))
                {
                    var processName = new FileInfo(fileLocation).Name.ToLowerInvariant().Replace(".exe", string.Empty);
                    if (Process.GetProcessesByName(processName).Length > 0) continue;
                }

                Process.Start(new ProcessStartInfo(fileLocation, arguments)
                {
                    WorkingDirectory = Path.GetDirectoryName(fileLocation),
                    UseShellExecute = true
                });
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Failed to start '{fileLocation}'");
            }
        }
    }
}

public class StartupInstance
{
    public Observable<string> FileLocation { get; } = new(string.Empty);
    public Observable<string> Arguments { get; } = new(string.Empty);
}