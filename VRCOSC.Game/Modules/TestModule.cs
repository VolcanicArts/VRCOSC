using Markdig.Helpers;

namespace VRCOSC.Game.Modules;

public class TestModule : Module
{
    public override string Title => "Test";
    public override string Description => "A test module";

    public override ModuleParametersManager ParametersManager => new(new OrderedList<ModuleOscParameter>()
    {
        new("testparameter", "Test Parameter", "A parameter that is the first one in this module", "/test/parameter"),
        new("testparameter2", "Another Test Parameter", "Another parameter that comes second", "/test/parameter2"),
        new("testparameter3", "One More Test Parameter", "The final parameter in this module", "/test/parameter3")
    });

    public TestModule()
    {
        CreateSetting("teststring", "This is a test string");
        CreateSetting("testbool", false);
        CreateSetting("testing", 0);
    }

    public override void Start()
    {
        Terminal.Add("Starting test module");
    }

    public override void Stop()
    {
        Terminal.Add("Stopping test module");
    }
}
