// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.Sidebar;
using VRCOSC.Game.Graphics.Updater;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game;

[Cached]
public abstract class VRCOSCGame : VRCOSCGameBase
{
    [Cached]
    private ModuleManager moduleManager = new();

    private VRCOSCUpdateManager updateManager = null!;

    public Bindable<Tabs> SelectedTab = new();
    public Bindable<string> SearchTermFilter = new(string.Empty);
    public Bindable<ModuleType?> TypeFilter = new();
    public Bindable<Module?> EditingModule = new();
    public BindableBool ModulesRunning = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            moduleManager,
            new MainContent(),
            updateManager = CreateUpdateManager()
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        updateManager.CheckForUpdate();
    }

    protected abstract VRCOSCUpdateManager CreateUpdateManager();
}
