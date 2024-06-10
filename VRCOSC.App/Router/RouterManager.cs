// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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
            Address = IPAddress.Loopback,
            Port = 9003
        });
    }

    public void Start()
    {
        foreach (var routerInstance in Routes)
        {
            var endpoint = new IPEndPoint(routerInstance.Address, routerInstance.Port);

            Logger.Log($"Starting router instance on {endpoint}");

            var sender = new OscSender();
            sender.Initialise(endpoint);
            sender.Enable();

            senders.Add(sender);
        }

        AppManager.GetInstance().VRChatOscClient.OnDataReceived += onDataReceived;
    }

    public void Stop()
    {
        AppManager.GetInstance().VRChatOscClient.OnDataReceived -= onDataReceived;

        foreach (var sender in senders)
        {
            Logger.Log($"Stopping router instance on {sender.EndPoint}");

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
