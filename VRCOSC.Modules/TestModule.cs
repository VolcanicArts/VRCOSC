// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.SDK;
using VRCOSC.Game.Modules.SDK.Attributes;
using VRCOSC.Game.Modules.SDK.Parameters;

namespace VRCOSC.Modules;

[ModuleTitle("Test Module")]
[ModuleDescription("This is a test module", "This is a description of the module that is known for being a test module")]
[ModuleType(ModuleType.Generic)]
public class TestModule : Module
{
    private float foo;

    protected override void OnLoad()
    {
        RegisterParameter<float>(TestParameter.Test, "SomeTestParameter", "This is a test parameter", "This is a parameter description", ParameterMode.Write);

        CreateToggle(TestSetting.Test, "Test", "This is a test setting", false, false);
        CreateStringList(TestSetting.TestList, "Test List", "This is a test list", false, new[] { "This", "Is", "A", "Test" });
        CreateToggle(TestSetting.TestAgain, "Test Again", "This is another separate setting", true, false);

        CreateGroup("Credentials", TestSetting.Test, TestSetting.TestList);
    }

    protected override void OnPostLoad()
    {
        GetSettingContainer<ListStringModuleSetting>(TestSetting.TestList)!.Enabled.BindTo(GetSettingContainer<BoolModuleSetting>(TestSetting.Test)!.Attribute);

        GetSettingContainer<BoolModuleSetting>(TestSetting.Test)!.Attribute.Value = true;
    }

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
        SendParameter(TestParameter.Test, foo += 0.01f);

        Log($"This setting is {GetSetting<bool>(TestSetting.Test)}");
        Log($"The value in index 1 is {GetSetting<List<string>>(TestSetting.TestList)![1]}");
    }

    protected override Task OnModuleStop()
    {
        SendParameter("SomeParameter", false);
        return Task.CompletedTask;
    }

    private enum TestSetting
    {
        Test,
        TestList,
        TestAgain
    }

    private enum TestParameter
    {
        Test
    }
}
