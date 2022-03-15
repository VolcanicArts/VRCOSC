using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Modules;
using VRCOSC.Resources;

namespace VRCOSC.Game;

public class VRCOSCGameBase : osu.Framework.Game
{
    [Cached]
    private ModuleManager moduleManager = new();

    protected VRCOSCGameBase()
    {
        base.Content.Add(Content = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = new Vector2(1366, 768)
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Scheduler.AddDelayed(moduleManager.Update, (1d / 5d) * 1000d, true);
    }

    protected override Container<Drawable> Content { get; }

    [BackgroundDependencyLoader]
    private void load(FrameworkConfigManager configManager)
    {
        configManager.GetBindable<ExecutionMode>(FrameworkSetting.ExecutionMode).Value = ExecutionMode.SingleThread;
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));
    }
}
