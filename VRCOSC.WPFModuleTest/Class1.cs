using VRCOSC.App.Modules;
using VRCOSC.App.Parameters;

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

        RegisterParameter<bool>(TestModuleParameter.Parameter1, "VRCOSC/TestModule/Parameter1", ParameterMode.ReadWrite, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter2, "VRCOSC/TestModule/Parameter1", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter3, "VRCOSC/TestModule/Parameter1", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter4, "VRCOSC/TestModule/Parameter1", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter5, "VRCOSC/TestModule/Parameter1", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter6, "VRCOSC/TestModule/Parameter1", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter7, "VRCOSC/TestModule/Parameter1", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter8, "VRCOSC/TestModule/Parameter1", ParameterMode.Read, "My Parameter", "This is my parameter");

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

    private enum TestModuleParameter
    {
        Parameter1,
        Parameter2,
        Parameter3,
        Parameter4,
        Parameter5,
        Parameter6,
        Parameter7,
        Parameter8
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
