using VRCOSC.App.Modules;

namespace VRCOSC.WPFModuleTest;

[ModuleTitle("Test Module")]
[ModuleDescription("This is my test module")]
[ModuleType(ModuleType.Generic)]
public class TestModule : Module
{
    public override Task<bool> OnModuleStart()
    {
        return Task.FromResult(true);
    }
}
