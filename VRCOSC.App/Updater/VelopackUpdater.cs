// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using NuGet.Versioning;
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

    public VelopackUpdater()
    {
        SettingsManager.GetInstance().GetObservable<UpdateChannel>(VRCOSCSetting.UpdateChannel).Subscribe(async () =>
        {
            constructUpdateManager();
            await CheckForUpdatesAsync();
            if (updateInfo is null) return;

            var shouldUpdate = PresentUpdate();
            if (shouldUpdate) await ExecuteUpdateAsync();
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

    public bool IsInstalled() => updateManager.IsInstalled;
    public bool IsUpdateAvailable() => updateInfo is not null;

    public async Task CheckForUpdatesAsync()
    {
        Logger.Log("Checking for update");

        if (!IsInstalled()) return;

        updateInfo = await updateManager.CheckForUpdatesAsync();
        Logger.Log(updateInfo is null ? "No updates available" : "Updates available");
    }

    public bool PresentUpdate()
    {
        try
        {
            Debug.Assert(updateInfo is not null);

            // switching channels will cause an update to the same version. No need to update
            if (SemanticVersion.Parse(AppManager.Version) == updateInfo.TargetFullRelease.Version) return false;

            var upgradeMessage = $"A new update is available for VRCOSC!\nWould you like to update?\n\nCurrent Version: {AppManager.Version}\nNew Version: {updateInfo.TargetFullRelease.Version}";
            var downgradeMessage = $"Updating will downgrade due to switching channels.\nAre you sure you want to downgrade?\n\nCurrent Version: {AppManager.Version}\nNew Version: {updateInfo.TargetFullRelease.Version}";

            var result = MessageBox.Show(updateInfo.IsDowngrade ? downgradeMessage : upgradeMessage, "VRCOSC Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
            return result == MessageBoxResult.Yes;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "An error occurred when trying to present the update");
            return false;
        }
    }

    public async Task ExecuteUpdateAsync()
    {
        try
        {
            Debug.Assert(updateInfo is not null);

            await updateManager.DownloadUpdatesAsync(updateInfo);
            SettingsManager.GetInstance().GetObservable<UpdateChannel>(VRCOSCMetadata.InstalledUpdateChannel).Value = SettingsManager.GetInstance().GetValue<UpdateChannel>(VRCOSCSetting.UpdateChannel);
            updateManager.ApplyUpdatesAndRestart(null);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "An error occured when trying to execute the update");
        }
    }
}