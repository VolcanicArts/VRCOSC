using Markdig.Helpers;
using osu.Framework.Allocation;
using VRCOSC.Game.Graphics.Containers.Terminal;

namespace VRCOSC.Game.Modules;

public class TestModule : Module
{
    [Resolved]
    private TerminalContainer terminalContainer { get; set; }

    private const string module_name = nameof(TestModule);

    public override string Title => "Test";
    public override string Description => "A test module";

    public override OrderedList<ModuleSetting> Settings => new()
    {
        new ModuleSettingBool("testboolean", "Test Boolean", "This is to test booleans"),
        new ModuleSettingInt("testint", "Test Integer", "This is to test integers"),
        new ModuleSettingString("teststring", "Test String", "This is to test strings")
    };

    public override void Start()
    {
        Scheduler.AddDelayed(() => terminalContainer.Log(module_name, "This is a test"), 1000, true);
    }

    public override void Stop()
    {
        Scheduler.CancelDelayedTasks();
    }
}
