namespace VRCOSC.Game.Modules;

public class TestModule : Module
{
    public override string Title => "Test";
    public override string Description => "A test module";

    public TestModule()
    {
        CreateSetting("teststring", "Test String", "This is a test string", "This is a test string");
        CreateSetting("testbool", "Test Bool", "This is a test boolean", false);
        CreateSetting("testint", "Test Int", "This is a test integer", 0);

        CreateParameter("testparameter", "Test Parameter", "A parameter that is the first one in this module", "/test/parameter");
        CreateParameter("testparameter2", "Another Test Parameter", "Another parameter that comes second", "/test/parameter2");
        CreateParameter("testparameter3", "One More Test Parameter", "The final parameter in this module", "/test/parameter3");
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
