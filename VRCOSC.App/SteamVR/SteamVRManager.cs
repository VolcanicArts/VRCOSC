// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.IO;
using System.Text.Json;
using VRCOSC.App.OpenVR;
using VRCOSC.App.OpenVR.Device;

namespace VRCOSC.App.SteamVR;

public class SteamVRManager
{
    private string? lighthouseConsoleExe { get; set; }

    private OpenVRManager openVRManager => AppManager.GetInstance().OpenVRManager;

    internal SteamVRManager()
    {
        initialiseExecutables();
    }

    private void initialiseExecutables()
    {
        if (!File.Exists(OpenVRManager.VRPATH_FILE)) return;

        var openVRPaths = JsonSerializer.Deserialize<OpenVRPaths>(File.ReadAllText(OpenVRManager.VRPATH_FILE));
        if (openVRPaths is null) return;

        var runtimePath = openVRPaths.Runtime[0];
        lighthouseConsoleExe = Path.Join(runtimePath, "tools", "lighthouse", "bin", "win64", "lighthouse_console.exe");
    }

    public void ShutdownDevice(TrackedDevice device)
    {
        if (!openVRManager.Initialised || lighthouseConsoleExe is null) return;

        var startInfo = new ProcessStartInfo(lighthouseConsoleExe)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = $"/serial {device.DongleId} poweroff"
        };

        Process.Start(startInfo);
    }
}