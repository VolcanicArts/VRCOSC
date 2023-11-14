// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.SDK;

namespace VRCOSC.Modules;

[ModuleTitle("Test Module")]
[ModuleDescription("This is a test module", "This is a description of the module that is known for being a test module")]
[ModuleType(ModuleType.Generic)]
public class TestModule : Module
{
    private float foo;

    protected override Task OnModuleStart()
    {
        foo = 0f;
        SendParameter("SomeParameter", true);
        return Task.CompletedTask;
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 500)]
    private void onModuleUpdate()
    {
        SendParameter("SomeFloatParameter", foo += 0.01f);
        Log("This is a test 2");
    }

    protected override Task OnModuleStop()
    {
        SendParameter("SomeParameter", false);
        return Task.CompletedTask;
    }
}
