// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using VRCOSC.App.OSC.Client;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router;

public class RouterManager
{
    private static RouterManager? instance;
    public static RouterManager GetInstance() => instance ??= new RouterManager();

    public ObservableCollection<RouterInstance> Routes { get; } = new();

    private readonly List<OscSender> senders = new();

    public void Load()
    {
        Routes.Add(new RouterInstance
        {
            Name = "Test",
            Address = IPAddress.Loopback.ToString(),
            Port = 9003
        });

        Routes.Add(new RouterInstance
        {
            Name = "Test",
            Address = IPAddress.Loopback.ToString(),
            Port = 9003
        });

        Routes.Add(new RouterInstance
        {
            Name = "Test",
            Address = IPAddress.Loopback.ToString(),
            Port = 9003
        });
    }

    public void Start()
    {
        foreach (var route in Routes)
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(route.Address), route.Port);

                Logger.Log($"Starting router instance `{route.Name}` on {endpoint}");

                var sender = new OscSender();
                sender.Initialise(endpoint);
                sender.Enable();

                senders.Add(sender);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Failed to start router instance named {route.Name}");
            }
        }

        AppManager.GetInstance().VRChatOscClient.OnDataReceived += onDataReceived;
    }

    public void Stop()
    {
        Logger.Log("Stopping router instances");

        AppManager.GetInstance().VRChatOscClient.OnDataReceived -= onDataReceived;

        foreach (var sender in senders)
        {
            sender.Disable();
        }

        senders.Clear();
    }

    private void onDataReceived(byte[] data)
    {
        foreach (var sender in senders)
        {
            sender.Send(data);
        }
    }
}
