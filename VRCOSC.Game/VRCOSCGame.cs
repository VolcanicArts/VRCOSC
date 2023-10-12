// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Screens;
using VRCOSC.Game.Screens.Loading;
using VRCOSC.Game.Screens.Main;

namespace VRCOSC.Game;

[Cached]
public abstract partial class VRCOSCGame : VRCOSCGameBase
{
    private ScreenStack baseScreenStack = null!;

    private Screen mainScreen = null!;
    private Screen loadingScreen = null!;

    /// <summary>
    /// For use when <see cref="LoadingScreen"/> is pushed
    /// </summary>
    public Bindable<string> LoadingAction = new(string.Empty);

    /// <summary>
    /// For use when <see cref="LoadingScreen"/> is pushed
    /// </summary>
    public Bindable<float> LoadingProgress = new();

    [BackgroundDependencyLoader]
    private void load(GameHost host)
    {
        Window.Title = host.Name;

        Add(baseScreenStack = new ScreenStack());

        mainScreen = new MainScreen();
        loadingScreen = new LoadingScreen();

        baseScreenStack.Push(mainScreen);
        baseScreenStack.Push(loadingScreen);
    }

    protected override async void LoadComplete()
    {
        base.LoadComplete();

        LoadingAction.Value = "Loading some shit idk";
        Scheduler.AddDelayed(() => LoadingProgress.Value += 0.01f, 15, true);

        await Task.Delay(1500);

        LoadingAction.Value = "Complete!";
        Scheduler.Add(() => loadingScreen.Exit(), false);
    }
}
