using VRCOSC.App.Modules;

namespace VRCOSC.WPFModuleTest;

[ModuleTitle("Test Module")]
[ModuleDescription("This is my test module")]
[ModuleType(ModuleType.Generic)]
public class TestModule : AvatarModule
{
    public override void OnPreLoad()
    {
        CreateToggle(TestModuleSetting.TestSetting, "Test Setting", "This is a test setting", false);
        CreateToggle(TestModuleSetting.TestSetting2, "Another Test Setting", "This is another test setting to test the settings", true);
        CreateToggle(TestModuleSetting.TestSetting3, "Lone Wolf", "This is a setting with no group", false);
        CreateTextBox(TestModuleSetting.StringSetting, "String Setting", "This is a string setting", "Woooo");

        CreateGroup("Best Friends", TestModuleSetting.TestSetting, TestModuleSetting.TestSetting2);
    }

    public override Task<bool> OnModuleStart()
    {
        return Task.FromResult(true);
    }

    private enum TestModuleSetting
    {
        TestSetting,
        TestSetting2,
        TestSetting3,
        StringSetting
    }
}

[ModuleTitle("Test Module 2")]
[ModuleDescription("This is my 2nd test module")]
[ModuleType(ModuleType.Generic)]
public class TestModule2 : AvatarModule
{
    public override void OnPreLoad()
    {
        CreateToggle(TestModuleSetting.TestSetting, "Test Setting", "This is a test setting", false);
        CreateToggle(TestModuleSetting.TestSetting2, "Another Test Setting", "This is another test setting to test the settings", true);
        CreateToggle(TestModuleSetting.TestSetting3, "Lone Wolf", "This is a setting with no group", false);
        CreateTextBox(TestModuleSetting.StringSetting, "String Setting", "This is a string setting", "Woooo");

        CreateGroup("Best Friends", TestModuleSetting.TestSetting, TestModuleSetting.TestSetting2);
    }

    public override Task<bool> OnModuleStart()
    {
        return Task.FromResult(true);
    }

    private enum TestModuleSetting
    {
        TestSetting,
        TestSetting2,
        TestSetting3,
        StringSetting
    }
}
