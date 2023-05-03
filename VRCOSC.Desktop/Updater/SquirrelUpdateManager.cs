// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Threading.Tasks;
using Squirrel;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Desktop.Updater;

public partial class SquirrelUpdateManager : VRCOSCUpdateManager
{
    private const string repo = "https://github.com/VolcanicArts/VRCOSC";

    private readonly GithubUpdateManager updateManager;
    private UpdateInfo? updateInfo;
    private bool useDelta;

    public SquirrelUpdateManager()
    {
        updateManager = new GithubUpdateManager(repo);
        initialise();
    }

    private void initialise()
    {
        updateInfo = null;
        useDelta = true;
    }

    protected override Task PrepareUpdateAsync() => UpdateManager.RestartAppWhenExited();

    public override async Task PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

    private async Task checkForUpdateAsync()
    {
        Log("Checking for updates");

        if (!updateManager.IsInstalledApp)
        {
            Log("Portable app detected. Cancelled update check");
            return;
        }

        try
        {
            updateInfo = await updateManager.CheckForUpdate(!useDelta);

            if (!updateInfo.ReleasesToApply.Any())
            {
                Log("No updates found");
                initialise();
                return;
            }

            Log($"{updateInfo.ReleasesToApply.Count} updates found");

            if (ApplyUpdatesImmediately)
                await ApplyUpdatesAsync();
            else
                PostUpdateAvailableNotification();
        }
        catch (Exception e)
        {
            PostFailNotification();
            LogError(e);
            initialise();
        }
    }

    protected override async Task ApplyUpdatesAsync()
    {
        Log("Attempting to apply updates");

        if (updateInfo is null)
            throw new InvalidOperationException("Cannot apply updates without checking");

        var progressNotification = PostProgressNotification();

        try
        {
            await updateManager.DownloadReleases(updateInfo.ReleasesToApply, p => progressNotification.Progress = map(p / 100f, 0, 1, 0, 0.5f));
            await updateManager.ApplyReleases(updateInfo, p => progressNotification.Progress = map(p / 100f, 0, 1, 0.5f, 1));
            PostSuccessNotification();
            Log("Update successfully applied");
            initialise();
        }
        catch (Exception e)
        {
            progressNotification.Close();

            // Update may have failed due to the installed version being too outdated
            // Retry without trying for delta
            if (useDelta)
            {
                useDelta = false;
                await checkForUpdateAsync();
                return;
            }

            PostFailNotification();
            LogError(e);
            initialise();
        }
    }

    private static float map(float source, float sMin, float sMax, float dMin, float dMax)
    {
        return dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        updateManager.Dispose();
    }
}
