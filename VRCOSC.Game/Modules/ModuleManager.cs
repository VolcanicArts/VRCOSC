// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Containers.Screens;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public sealed class ModuleManager : Container<ModuleGroup>
{
    private bool running;
    private bool autoStarted;

    public readonly OscClient OSCClient = new();

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; }

    [Resolved]
    private ScreenManager screenManager { get; set; }

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        List<Module> modules = ReflectiveEnumerator.GetEnumerableOfType<Module>();

        var moduleStorage = storage.GetStorageForDirectory("modules");

        foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
        {
            var moduleGroup = new ModuleGroup(type);

            foreach (var module in modules.Where(module => module.ModuleType.Equals(type)))
            {
                module.Initialise(moduleStorage, OSCClient);
                module.CreateAttributes();
                module.PerformLoad();
                moduleGroup.Add(new ModuleContainer(module));
            }

            Add(moduleGroup);
        }

        Scheduler.Add(checkForVrChat);
        Scheduler.AddDelayed(checkForVrChat, 10000, true);
    }

    private void checkForVrChat()
    {
        var vrChat = Process.GetProcessesByName("vrchat");

        var autoStartStop = configManager.Get<bool>(VRCOSCSetting.AutoStartStop);

        if (vrChat.Length != 0 && autoStartStop && !running && !autoStarted)
        {
            screenManager.ShowTerminal();
            autoStarted = true;
        }

        if (vrChat.Length == 0 && autoStartStop && running)
        {
            screenManager.HideTerminal();
            autoStarted = false;
        }
    }

    public void Start()
    {
        OSCClient.Enable();
        this.ForEach(child => child.Start());
        running = true;
    }

    public void Stop()
    {
        running = false;
        this.ForEach(child => child.Stop());
        OSCClient.Disable();
    }

    protected override void Dispose(bool isDisposing)
    {
        if (running) Stop();
        OSCClient.Dispose();
        base.Dispose(isDisposing);
    }
}
