// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using VRCOSC.Game.Screens;
using VRCOSC.Game.Screens.Loading;

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
    private void load()
    {
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
        Scheduler.AddDelayed(() => LoadingProgress.Value += 0.01f, 30, true);

        await Task.Delay(3000);

        LoadingAction.Value = "Complete!";
        Scheduler.Add(() => loadingScreen.Exit(), false);
    }
}
