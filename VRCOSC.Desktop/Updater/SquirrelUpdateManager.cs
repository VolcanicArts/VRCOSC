// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Squirrel;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Desktop.Updater;

public class SquirrelUpdateManager : VRCOSCUpdateManager
{
    private GithubUpdateManager updateManager;
    private const string development_access_token = null;
    private const string repo = @"https://github.com/VolcanicArts/VRCOSC";

    [Resolved]
    private GameHost host { get; set; }

    public override async void CheckForUpdate(bool useDelta = true)
    {
        try
        {
            Logger.Log("Attempting to find update...");
            updateManager ??= new GithubUpdateManager(repo, false, development_access_token);

            if (!updateManager.IsInstalledApp)
            {
                Logger.Log("Cannot update. Not installed app");
                return;
            }

            Show();

            try
            {
                Logger.Log("Checking for update...");

                SetPhase(UpdatePhase.Check);
                var updateInfo = await updateManager.CheckForUpdate(!useDelta, UpdateProgress).ConfigureAwait(false);

                if (updateInfo.ReleasesToApply.Count == 0)
                {
                    Hide();
                    return;
                }

                Logger.Log("Found updates to apply!");

                SetPhase(UpdatePhase.Download);
                await updateManager.DownloadReleases(updateInfo.ReleasesToApply, UpdateProgress).ConfigureAwait(false);

                SetPhase(UpdatePhase.Install);
                await updateManager.ApplyReleases(updateInfo, UpdateProgress).ConfigureAwait(false);

                SetPhase(UpdatePhase.Success);
            }
            catch (Exception)
            {
                //delta update may have failed due to the installed version being too outdated. Retry without trying for delta
                if (useDelta)
                {
                    CheckForUpdate(false);
                    return;
                }

                throw;
            }
        }
        catch (Exception e)
        {
            SetPhase(UpdatePhase.Fail);

            Logger.Error(e, "Updater Error");
        }
    }

    protected override void RequestRestart()
    {
        UpdateManager.RestartAppWhenExited().ContinueWith(_ => host.Exit()).ConfigureAwait(false);
    }
}
