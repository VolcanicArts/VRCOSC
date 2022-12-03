// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Squirrel;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Settings;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Desktop.Updater;

public partial class SquirrelUpdateManager : VRCOSCUpdateManager
{
    private GithubUpdateManager? updateManager;
    private UpdateInfo? updateInfo;

    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private VRCOSCConfigManager config { get; set; } = null!;

    public override async Task CheckForUpdate(string repo, bool useDelta = true)
    {
        try
        {
            updateManager ??= new GithubUpdateManager(repo);
            if (!updateManager.IsInstalledApp) return;

            try
            {
                updateInfo = await updateManager.CheckForUpdate(!useDelta).ConfigureAwait(false);
                if (updateInfo.ReleasesToApply.Count == 0) return;

                var updateMode = config.Get<UpdateMode>(VRCOSCSetting.UpdateMode);

                if (updateMode == UpdateMode.Auto)
                    await ApplyUpdates();
                else
                    PostCheckNotification();
            }
            catch (Exception)
            {
                //delta update may have failed due to the installed version being too outdated. Retry without trying for delta
                if (useDelta)
                {
                    await CheckForUpdate(repo, false);
                    return;
                }

                throw;
            }
        }
        catch (Exception e)
        {
            PostFailNotification();
            Logger.Error(e, "Updater Error");
        }
    }

    protected override async Task ApplyUpdates()
    {
        if (updateManager is null || updateInfo is null)
            throw new InvalidOperationException("Cannot apply updates without checking");

        try
        {
            var notification = PostProgressNotification();
            await updateManager.DownloadReleases(updateInfo.ReleasesToApply, p => notification.Progress = map(p / 100f, 0, 1, 0, 0.5f)).ConfigureAwait(false);
            await updateManager.ApplyReleases(updateInfo, p => notification.Progress = map(p / 100f, 0, 1, 0.5f, 1)).ConfigureAwait(false);
            PostSuccessNotification();
        }
        catch (Exception)
        {
            PostFailNotification();
        }
    }

    private static float map(float source, float sMin, float sMax, float dMin, float dMax)
    {
        return dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
    }

    protected override void RequestRestart()
    {
        UpdateManager.RestartAppWhenExited().ContinueWith(_ => host.Exit()).ConfigureAwait(false);
    }
}
