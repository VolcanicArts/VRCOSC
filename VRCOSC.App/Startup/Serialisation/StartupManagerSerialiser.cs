// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Startup.Serialisation;

public class StartupManagerSerialiser : Serialiser<StartupManager, SerialisableStartupManager>
{
    protected override string Directory => "configuration";
    protected override string FileName => "startup.json";

    public StartupManagerSerialiser(Storage storage, StartupManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableStartupManager data)
    {
        foreach (var serialisableStartupInstance in data.Instances)
        {
            Reference.Instances.Add(new StartupInstance
            {
                FileLocation = { Value = serialisableStartupInstance.FileLocation },
                Arguments = { Value = serialisableStartupInstance.Arguments }
            });
        }

        return false;
    }
}