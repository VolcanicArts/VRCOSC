// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Serialisation;
using VRCOSC.Game.Startup.Serialisation.V1;
using VRCOSC.Game.Startup.Serialisation.V2;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Managers;

public class StartupManager
{
    private readonly TerminalLogger logger = new("VRCOSC");
    private SerialisationManager serialisationManager = null!;

    public readonly BindableList<StartupInstance> Instances = new();

    public void Initialise(Storage storage)
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new StartupSerialiserV1(storage, this));
        serialisationManager.RegisterSerialiser(2, new StartupSerialiserV2(storage, this));
    }

    public void Load()
    {
        Deserialise();

        Instances.BindCollectionChanged((_, _) => Serialise());

        Instances.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (StartupInstance newItem in e.NewItems)
                {
                    newItem.FilePath.BindValueChanged(_ => Serialise());
                    newItem.LaunchArguments.BindValueChanged(_ => Serialise());
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
        Instances.ForEach(instance =>
        {
            try
            {
                if (!File.Exists(instance.FilePath.Value)) return;

                var processName = new FileInfo(instance.FilePath.Value).Name.ToLowerInvariant().Replace(".exe", string.Empty);

                if (Process.GetProcessesByName(processName).Any()) return;

                Process.Start(new ProcessStartInfo(instance.FilePath.Value, instance.LaunchArguments.Value));
                logger.Log($"Running file {instance.FilePath.Value}" + (string.IsNullOrEmpty(instance.LaunchArguments.Value) ? string.Empty : $" with launch arguments {instance.LaunchArguments.Value}"));
            }
            catch (Exception)
            {
                logger.Log($"Failed to run {instance.FilePath.Value}");
            }
        });
    }
}

public class StartupInstance
{
    public Bindable<string> FilePath = new(string.Empty);
    public Bindable<string> LaunchArguments = new(string.Empty);
}
