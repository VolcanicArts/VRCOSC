// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Platform;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Serialisation;
using VRCOSC.Game.Startup.Serialisation.V1.Models;

namespace VRCOSC.Game.Startup.Serialisation.V1;

public class StartupSerialiserV1 : Serialiser<StartupManager, SerialisableStartupManagerV1>
{
    protected override string FileName => "startup.json";

    public StartupSerialiserV1(Storage storage, StartupManager startupManager)
        : base(storage, startupManager)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableStartupManagerV1 data)
    {
        Reference.Instances.ReplaceItems(data.FilePaths.Select(filepath => new StartupInstance
        {
            FilePath = { Value = filepath }
        }));

        return false;
    }
}
