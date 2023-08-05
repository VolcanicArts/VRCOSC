// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Platform;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Serialisation;
using VRCOSC.Game.Startup.Serialisation.V2.Models;

namespace VRCOSC.Game.Startup.Serialisation.V2;

public class StartupSerialiserV2 : Serialiser<StartupManager, SerialisableStartupManagerV2>
{
    protected override string FileName => "startup.json";

    public StartupSerialiserV2(Storage storage, StartupManager startupManager)
        : base(storage, startupManager)
    {
    }

    protected override bool ExecuteAfterDeserialisation(StartupManager startupManager, SerialisableStartupManagerV2 data)
    {
        startupManager.Instances.ReplaceItems(data.Instances.Select(instance => new StartupInstance
        {
            FilePath = { Value = instance.FilePath },
            LaunchArguments = { Value = instance.LaunchArguments }
        }));

        return false;
    }
}
