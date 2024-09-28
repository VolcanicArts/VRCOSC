// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Velopack;
using Velopack.Sources;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Updater;

public class VelopackUpdater
{
    private const string repo_url = "https://github.com/VolcanicArts/VRCOSC";

    private UpdateManager updateManager = null!;
    private UpdateInfo? updateInfo;

    public bool IsUpdateAvailable => updateInfo is not null;

    public VelopackUpdater()
    {
        SettingsManager.GetInstance().GetObservable<UpdateChannel>(VRCOSCSetting.UpdateChannel).Subscribe(async () =>
        {
            constructUpdateManager();
            await CheckForUpdatesAsync();
            ShowUpdateIfAvailable();
        });

        constructUpdateManager();
    }

    private void constructUpdateManager()
    {
        var updateChannel = SettingsManager.GetInstance().GetValue<UpdateChannel>(VRCOSCSetting.UpdateChannel);
        var allowPreRelease = updateChannel == UpdateChannel.Beta;

        updateManager = new UpdateManager(new GithubSource(repo_url, null, allowPreRelease), new UpdateOptions
        {
            ExplicitChannel = updateChannel.ToString().ToLowerInvariant(),
            AllowVersionDowngrade = true
        });
    }

    public async Task<bool> CheckForUpdatesAsync()
    {
        Logger.Log("Checking for update");

        if (!updateManager.IsInstalled)
        {
            Logger.Log("Portable app detected. Cancelling update check");
            updateInfo = null;
            return false;
        }

        updateInfo = await updateManager.CheckForUpdatesAsync();
        Logger.Log(updateInfo is null ? "No updates available" : "Updates available");
        return updateInfo is not null;
    }

    public async void ShowUpdateIfAvailable()
    {
        if (!IsUpdateAvailable) return;

        var upgradeMessage = $"A new update is available! Would you like to update?\n\nCurrent Version: {AppManager.Version}\nNew Version: {updateInfo!.TargetFullRelease.Version}";
        var downgradeMessage = $"Updating will downgrade due to switching channels. Are you sure you want to downgrade?\n\nCurrent Version: {AppManager.Version}\nNew Version: {updateInfo!.TargetFullRelease.Version}";

        var result = MessageBox.Show(updateInfo.IsDowngrade ? downgradeMessage : upgradeMessage, "Update Available", MessageBoxButtons.YesNo);
        if (result == DialogResult.No) return;

        try
        {
            await updateManager.DownloadUpdatesAsync(updateInfo);
            updateManager.ApplyUpdatesAndRestart(null);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "An error occurred when trying to update");
        }
    }
}
