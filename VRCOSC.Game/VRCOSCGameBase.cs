using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osuTK;
using VRCOSC.Resources;

namespace VRCOSC.Game;

public class VRCOSCGameBase : osu.Framework.Game
{
    protected VRCOSCGameBase()
    {
        base.Content.Add(Content = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = new Vector2(1366, 768)
        });
    }

    protected override Container<Drawable> Content { get; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));
    }
}
