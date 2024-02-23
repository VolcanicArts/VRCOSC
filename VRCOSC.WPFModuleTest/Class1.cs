using VRCOSC.App.Modules;

namespace VRCOSC.WPFModuleTest;

[ModuleTitle("Test Module")]
[ModuleDescription("This is my test module")]
[ModuleType(ModuleType.Generic)]
public class TestModule : Module
{
    public override void OnPreLoad()
    {
        CreateToggle(TestModuleSetting.TestSetting, "Test Setting", "This is a test setting", false);
        CreateToggle(TestModuleSetting.TestSetting2, "Another Test Setting", "This is another test setting to test the settings", true);
    }

    public override Task<bool> OnModuleStart()
    {
        return Task.FromResult(true);
    }

    private enum TestModuleSetting
    {
        TestSetting,
        TestSetting2
    }
}
