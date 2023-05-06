// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using PInvoke;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Startup;

public class StartupManager
{
    private readonly TerminalLogger logger = new("VRCOSC");

    public readonly BindableList<Bindable<string>> FilePaths = new();

    public void Start()
    {
        FilePaths.ForEach(filePath =>
        {
            try
            {
                if (!File.Exists(filePath.Value)) return;

                var processName = new FileInfo(filePath.Value).Name.ToLowerInvariant().Replace(@".exe", string.Empty);

                if (Process.GetProcessesByName(processName).Any()) return;

                Process.Start(filePath.Value);
                logger.Log($"Running file {filePath.Value}");
            }
            catch (Win32Exception)
            {
                logger.Log($"Failed to run {filePath.Value}");
            }
        });
    }
}
