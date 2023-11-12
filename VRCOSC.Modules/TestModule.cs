// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.SDK;

namespace VRCOSC.Modules;

[ModuleTitle("Test Module")]
[ModuleDescription("This is a test module", "This is a description of the module that is known for being a test module")]
[ModuleType(ModuleType.Generic)]
public class TestModule : Module
{
    protected override Task OnModuleStart()
    {
        Console.WriteLine("I'm started!");
        return Task.CompletedTask;
    }

    protected override Task OnModuleStop()
    {
        Console.WriteLine("I'm stopped");
        return Task.CompletedTask;
    }
}
