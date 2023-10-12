// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

        loadingScreen = new LoadingScreen();
        mainScreen = new MainScreen();

        baseScreenStack.Push(loadingScreen);
    }

    protected override async void LoadComplete()
    {
        base.LoadComplete();

        LoadingAction.Value = "Loading graphics";
        LoadingProgress.Value = 0f;
        await LoadComponentAsync(mainScreen);
        LoadingProgress.Value = 1f;

        LoadingAction.Value = "Complete!";
        LoadingProgress.Value = 1f;
        Scheduler.Add(() => baseScreenStack.Push(mainScreen), false);
    }
}
