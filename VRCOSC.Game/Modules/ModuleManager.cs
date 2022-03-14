using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.Terminal;

namespace VRCOSC.Game.Modules;

public class ModuleManager : Container<Module>
{
    [Resolved]
    private TerminalContainer terminalContainer { get; set; }

    private const string module_name = nameof(ModuleManager);

    public ModuleManager()
    {
        Children = new Module[]
        {
            new TestModule()
        };
    }

    public void Start()
    {
        terminalContainer.Log(module_name, "Starting all modules", LogState.Important);
        Children.ForEach(module => module.Start());
    }
}
