// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using System.Windows.Forms;
using Velopack;
using Velopack.Sources;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;
using Application = System.Windows.Application;

namespace VRCOSC.App.Updater;

public partial class VelopackUpdater
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
        });

        constructUpdateManager();
    }

    private void constructUpdateManager()
    {
        var updateChannel = SettingsManager.GetInstance().GetValue<UpdateChannel>(VRCOSCSetting.UpdateChannel);

        updateManager = new UpdateManager(new GithubSource(repo_url, null, true), new UpdateOptions
        {
            ExplicitChannel = updateChannel.ToString().ToLowerInvariant(),
            AllowVersionDowngrade = true
        });
    }

    public async Task CheckForUpdatesAsync()
    {
        Logger.Log("Checking for update");

        if (!updateManager.IsInstalled)
        {
            Logger.Log("Portable app detected. Cancelling update check");
            updateInfo = null;
            return;
        }

        updateInfo = await updateManager.CheckForUpdatesAsync();
        Logger.Log(updateInfo is null ? "No updates available" : "Update available");
    }

    public async void ShowUpdate()
    {
        var result = MessageBox.Show($"A new update is available. Would you like to update?\n\nCurrent Version: {AppManager.Version}\nNew Version: {updateInfo!.TargetFullRelease.Version}", "Update Available", MessageBoxButtons.YesNo);
        if (result == DialogResult.No) return;

        await executeUpdate();
    }

    private async Task executeUpdate()
    {
        if (updateInfo is null) return;

        Logger.Log("Executing update");

        await downloadUpdate();
        applyUpdate();
    }

    private async Task downloadUpdate()
    {
        Logger.Log("Downloading update");
        await updateManager.DownloadUpdatesAsync(updateInfo!, handleDownloadProgress);
        Logger.Log("Update complete");
    }

    private void applyUpdate()
    {
        Logger.Log("Applying update");
        updateManager.WaitExitThenApplyUpdates(updateInfo!);
        Application.Current.MainWindow!.Close();
    }

    private void handleDownloadProgress(int progress)
    {
        Logger.Log($"Progress: {progress}");
    }
}
