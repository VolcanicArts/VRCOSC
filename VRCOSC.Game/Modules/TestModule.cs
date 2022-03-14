using osu.Framework.Allocation;
using VRCOSC.Game.Graphics.Containers.Terminal;

namespace VRCOSC.Game.Modules;

public class TestModule : Module
{
    [Resolved]
    private TerminalContainer terminalContainer { get; set; }

    private const string module_name = nameof(TestModule);

    public override void Start()
    {
        Scheduler.AddDelayed(() => terminalContainer.Log(module_name, "This is a test"), 1000, true);
    }

    public override void Stop()
    {
        Scheduler.CancelDelayedTasks();
    }
}
