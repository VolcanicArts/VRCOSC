using VRCOSC.App.Modules;
using VRCOSC.App.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.WPFModuleTest;

[ModuleTitle("Test Module")]
[ModuleDescription("This is my test module")]
[ModuleType(ModuleType.Generic)]
public class TestModule : AvatarModule
{
    protected override void OnPreLoad()
    {
        CreateToggle(TestModuleSetting.TestSetting, "Test Setting", "This is a test setting", false);
        CreateToggle(TestModuleSetting.TestSetting2, "Another Test Setting", "This is another test setting to test the settings", true);
        CreateToggle(TestModuleSetting.TestSetting3, "Lone Wolf", "This is a setting with no group", false);
        CreateTextBox(TestModuleSetting.StringSetting, "String Setting", "This is a string setting", "Woooo");
        CreateTextBox(TestModuleSetting.IntSetting, "Int Setting", "This is an int setting", 10);
        CreateSlider(TestModuleSetting.IntSliderSetting, "Int Slider Setting", "Blah", 5, 1, 15);
        CreateSlider(TestModuleSetting.FloatSliderSetting, "Float Slider Setting", "foo", 0f, 0f, 9f);
        CreateTextBoxList(TestModuleSetting.TextBoxList, "Text Box List", "bar", new List<string> { "This is a test entry", "This is another test entry" });

        RegisterParameter<bool>(TestModuleParameter.Parameter1, "VRCOSC/TestModule/Parameter1", ParameterMode.ReadWrite, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter2, "VRCOSC/TestModule/Parameter2", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter3, "VRCOSC/TestModule/Parameter3", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter4, "VRCOSC/TestModule/Parameter4", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter5, "VRCOSC/TestModule/Parameter5", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter6, "VRCOSC/TestModule/Parameter6", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter7, "VRCOSC/TestModule/Parameter7", ParameterMode.Read, "My Parameter", "This is my parameter");
        RegisterParameter<bool>(TestModuleParameter.Parameter8, "VRCOSC/TestModule/Parameter8", ParameterMode.Read, "My Parameter", "This is my parameter");

        CreateGroup("Best Friends", TestModuleSetting.TestSetting, TestModuleSetting.TestSetting2);
    }

    private bool someValue;

    [ModuleUpdate(ModuleUpdateMode.Custom, false)]
    private void onModuleUpdate()
    {
        SendParameter(TestModuleParameter.Parameter1, someValue);
        someValue = !someValue;
    }

    protected override void OnAnyParameterReceived(ReceivedParameter receivedParameter)
    {
        Logger.Log(receivedParameter.Name);
    }

    private enum TestModuleSetting
    {
        TestSetting,
        TestSetting2,
        TestSetting3,
        StringSetting,
        IntSetting,
        IntSliderSetting,
        FloatSliderSetting,
        TextBoxList
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
    protected override void OnPreLoad()
    {
        CreateToggle(TestModuleSetting.TestSetting, "Test Setting", "This is a test setting", false);
        CreateToggle(TestModuleSetting.TestSetting2, "Another Test Setting", "This is another test setting to test the settings", true);
        CreateToggle(TestModuleSetting.TestSetting3, "Lone Wolf", "This is a setting with no group", false);
        CreateTextBox(TestModuleSetting.StringSetting, "String Setting", "This is a string setting", "Woooo");

        CreateGroup("Best Friends", TestModuleSetting.TestSetting, TestModuleSetting.TestSetting2);
    }

    private enum TestModuleSetting
    {
        TestSetting,
        TestSetting2,
        TestSetting3,
        StringSetting
    }
}
