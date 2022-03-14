using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Modules;

public class ModuleManager : Container<Module>
{
    public ModuleManager()
    {
        Children = new Module[]
        {
            new TestModule()
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Children.ForEach(module => module.Start());
    }
}
